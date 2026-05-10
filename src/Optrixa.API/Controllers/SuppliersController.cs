using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Optrixa.Application.Common;
using Optrixa.Domain.Entities;
using Optrixa.Domain.Interfaces;
using Optrixa.Infrastructure.Persistence;

namespace Optrixa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;

    public SuppliersController(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers()
    {
        var suppliers = await _uow.Suppliers.GetWithExpensesAsync();
        return Ok(ApiResponse<IEnumerable<Supplier>>.Ok(suppliers));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSupplier(int id)
    {
        var supplier = await _uow.Suppliers.GetWithExpensesByIdAsync(id);
        if (supplier is null)
            return NotFound(ApiResponse<Supplier>.Fail("Supplier not found."));
        return Ok(ApiResponse<Supplier>.Ok(supplier));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSupplier([FromBody] Supplier supplier)
    {
        await _uow.Suppliers.AddAsync(supplier);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<Supplier>.Ok(supplier, "Supplier created."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSupplier(
        int id, [FromBody] Supplier supplier)
    {
        if (id != supplier.Id) return BadRequest("ID mismatch.");
        supplier.UpdatedAt = DateTime.UtcNow;
        await _uow.Suppliers.UpdateAsync(supplier);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<Supplier>.Ok(supplier, "Supplier updated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(id);
        if (supplier is null)
            return NotFound(ApiResponse<bool>.Fail("Supplier not found."));

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _uow.Suppliers.UpdateAsync(supplier);
        await _uow.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Supplier deleted."));
    }

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> RecordPayment(
        int id, [FromBody] RecordPaymentDto dto)
    {
        // Load supplier directly from context
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (supplier is null)
            return NotFound(ApiResponse<bool>.Fail("Supplier not found."));

        if (dto.Amount <= 0)
            return BadRequest(ApiResponse<bool>.Fail(
                "Payment amount must be greater than zero."));

        var outstanding = supplier.TotalPurchased - supplier.TotalPaid;

        if (dto.Amount > outstanding)
            return BadRequest(ApiResponse<bool>.Fail(
                $"Payment exceeds outstanding balance of {outstanding:C}."));

        // Update supplier balance
        supplier.TotalPaid += dto.Amount;
        supplier.UpdatedAt = DateTime.UtcNow;

        // Load unpaid expenses directly — tracked by EF Core
        var unpaidExpenses = await _context.Expenses
            .Where(e =>
                e.SupplierId == id &&
                !e.IsPaid &&
                !e.IsDeleted)
            .OrderBy(e => e.ExpenseDate)
            .ToListAsync();

        var remaining = dto.Amount;

        foreach (var expense in unpaidExpenses)
        {
            if (remaining <= 0) break;

            if (expense.Amount <= remaining)
            {
                // Fully pay this expense
                expense.IsPaid = true;
                expense.PaidAt = DateTime.UtcNow;
                expense.UpdatedAt = DateTime.UtcNow;
                remaining -= expense.Amount;
            }
            else
            {
                // Payment covers part of this expense
                // We can't split an expense row so just mark it
                // as partially acknowledged — leave it unpaid
                // but reduce remaining to 0
                remaining = 0;
            }
        }

        // Save everything in one transaction
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true,
            $"Payment of {dto.Amount:C} recorded successfully."));
    }
}

public class RecordPaymentDto
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}