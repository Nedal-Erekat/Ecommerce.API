using AutoMapper;
using Ecommerce.API.Data;
using Ecommerce.API.DTOs;
using Ecommerce.API.Exceptions;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    private const string ProductsCachePrefix = "products_all";

    public ProductService(AppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(int page, int pageSize)
    {
        var cacheKey = $"{ProductsCachePrefix}_{page}_{pageSize}";

        if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is IEnumerable<ProductDto> cached)
            return cached;

        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => _mapper.Map<ProductDto>(p))
            .ToListAsync();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        // Store in cache
        _cache.Set(
            cacheKey,
            products,
            options
        );

        return products;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) 
            throw new NotFoundException($"product with id {id} not found!");

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = _mapper.Map<Product>(dto);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // ❗ Invalidate cache
        InvalidateProductCache();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) 
            throw new NotFoundException($"product with id {id} not found!");

        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();

        // ❗ Invalidate cache
        InvalidateProductCache();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // ❗ Invalidate cache
        InvalidateProductCache();

        return true;
    }

    private void InvalidateProductCache()
    {
        // Simple strategy: clear all product cache
        // (acceptable for in-memory cache)
        _cache.Remove($"{ProductsCachePrefix}_*");
    }
}
