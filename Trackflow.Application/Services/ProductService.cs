using Microsoft.EntityFrameworkCore;
using Trackflow.Application.DTOs;
using Trackflow.Domain.Entities;
using Trackflow.Infrastructure.Data;

namespace Trackflow.Application.Services;

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Customer)
            .OrderBy(p => p.UrunAdi)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.FirmaAdi,
                GTIN = p.GTIN,
                UrunAdi = p.UrunAdi,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Customer)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.FirmaAdi,
                GTIN = p.GTIN,
                UrunAdi = p.UrunAdi,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProductDto>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Products
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == customerId)
            .OrderBy(p => p.UrunAdi)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer.FirmaAdi,
                GTIN = p.GTIN,
                UrunAdi = p.UrunAdi,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            throw new ArgumentException("Customer not found");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            GTIN = dto.GTIN,
            UrunAdi = dto.UrunAdi
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            CustomerId = product.CustomerId,
            CustomerName = customer.FirmaAdi,
            GTIN = product.GTIN,
            UrunAdi = product.UrunAdi,
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
