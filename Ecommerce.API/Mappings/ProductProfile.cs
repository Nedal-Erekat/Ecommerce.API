using AutoMapper;
using Ecommerce.API.DTOs;
using Ecommerce.API.Models;

namespace Ecommerce.API.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Entity → DTO
        CreateMap<Product, ProductDto>();

        // DTO → Entity
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
    }
}
