using AutoMapper;
using Ecommerce.API.Data;
using Ecommerce.API.DTOs;
using Ecommerce.API.Exceptions;
using Ecommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _context.Products.ToListAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);

        //return await _context.Products
        //    .Select(p => new ProductDto
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        Description = p.Description,
        //        Price = p.Price
        //    })
        //    .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null) 
            throw new NotFoundException($"product with id {id} not found!");

        return _mapper.Map<ProductDto>(product);

        //return new ProductDto
        //{
        //    Id = product.Id,
        //    Name = product.Name,
        //    Description = product.Description,
        //    Price = product.Price
        //};
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        //var product = new Product
        //{
        //    Name = dto.Name,
        //    Description = dto.Description,
        //    Price = dto.Price,
        //    Stock = dto.Stock
        //};

        var product = _mapper.Map<Product>(dto);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);

        //return new ProductDto
        //{
        //    Id = product.Id,
        //    Name = product.Name,
        //    Description = product.Description,
        //    Price = product.Price
        //};
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) 
            throw new NotFoundException($"product with id {id} not found!");

        //product.Name = dto.Name;
        //product.Description = dto.Description;
        //product.Price = dto.Price;
        //product.Stock = dto.Stock;

        _mapper.Map(dto, product);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
