using Microsoft.EntityFrameworkCore;
using Trackflow.Domain.Entities;
using Trackflow.Infrastructure.Data;
using Trackflow.Shared.DTOs;

namespace Trackflow.Application.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _context.Customers
            .OrderBy(c => c.FirmaAdi)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirmaAdi = c.FirmaAdi,
                GLN = c.GLN,
                Aciklama = c.Aciklama,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirmaAdi = c.FirmaAdi,
                GLN = c.GLN,
                Aciklama = c.Aciklama,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirmaAdi = dto.FirmaAdi,
            GLN = dto.GLN,
            Aciklama = dto.Aciklama
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            FirmaAdi = customer.FirmaAdi,
            GLN = customer.GLN,
            Aciklama = customer.Aciklama,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return null;

        customer.FirmaAdi = dto.FirmaAdi;
        customer.GLN = dto.GLN;
        customer.Aciklama = dto.Aciklama;

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            FirmaAdi = customer.FirmaAdi,
            GLN = customer.GLN,
            Aciklama = customer.Aciklama,
            CreatedAt = customer.CreatedAt
        };
    }
}
