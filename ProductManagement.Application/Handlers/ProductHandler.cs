using MediatR;
using ProductManagement.Application.Commands;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Queries;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.Application.Handlers
{

    public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        private readonly IRepository<Product> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CreateProductHandler> _logger;

        public CreateProductHandler(IRepository<Product> repository, ICacheService cacheService, ILogger<CreateProductHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = new Product
                {
                    Name = request.Product.Name,
                    Description = request.Product.Description,
                    Price = request.Product.Price,
                    Stock = request.Product.Stock,
                    Category = request.Product.Category,
                    SKU = request.Product.SKU
                };

                var createdProduct = await _repository.AddAsync(product);

                // Cache invalidation
                await _cacheService.RemovePatternAsync("products:*");

                _logger.LogInformation("Product created with ID: {ProductId}", createdProduct.Id);

                return new ProductResponse
                {
                    Id = createdProduct.Id,
                    Name = createdProduct.Name,
                    Description = createdProduct.Description,
                    Price = createdProduct.Price,
                    Stock = createdProduct.Stock,
                    Category = createdProduct.Category,
                    SKU = createdProduct.SKU,
                    IsActive = createdProduct.IsActive,
                    CreatedAt = createdProduct.CreatedAt,
                    UpdatedAt = createdProduct.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }
    }

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        private readonly IRepository<Product> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UpdateProductHandler> _logger;

        public UpdateProductHandler(IRepository<Product> repository, ICacheService cacheService, ILogger<UpdateProductHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingProduct = await _repository.GetByIdAsync(request.Product.Id);
                if (existingProduct == null)
                    throw new KeyNotFoundException($"Product with ID {request.Product.Id} not found");

                existingProduct.Name = request.Product.Name;
                existingProduct.Description = request.Product.Description;
                existingProduct.Price = request.Product.Price;
                existingProduct.Stock = request.Product.Stock;
                existingProduct.Category = request.Product.Category;
                existingProduct.SKU = request.Product.SKU;
                existingProduct.IsActive = request.Product.IsActive;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(existingProduct);

                // Cache invalidation
                await _cacheService.RemovePatternAsync("products:*");
                await _cacheService.RemoveAsync($"product:{existingProduct.Id}");

                _logger.LogInformation("Product updated with ID: {ProductId}", existingProduct.Id);

                return new ProductResponse
                {
                    Id = existingProduct.Id,
                    Name = existingProduct.Name,
                    Description = existingProduct.Description,
                    Price = existingProduct.Price,
                    Stock = existingProduct.Stock,
                    Category = existingProduct.Category,
                    SKU = existingProduct.SKU,
                    IsActive = existingProduct.IsActive,
                    CreatedAt = existingProduct.CreatedAt,
                    UpdatedAt = existingProduct.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", request.Product.Id);
                throw;
            }
        }
    }

    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IRepository<Product> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeleteProductHandler> _logger;

        public DeleteProductHandler(IRepository<Product> repository, ICacheService cacheService, ILogger<DeleteProductHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var exists = await _repository.ExistsAsync(request.Id);
                if (!exists)
                    return false;

                await _repository.DeleteAsync(request.Id);

                // Cache invalidation
                await _cacheService.RemovePatternAsync("products:*");
                await _cacheService.RemoveAsync($"product:{request.Id}");

                _logger.LogInformation("Product deleted with ID: {ProductId}", request.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", request.Id);
                throw;
            }
        }
    }

    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductResponse>>
    {
        private readonly IRepository<Product> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetAllProductsHandler> _logger;

        public GetAllProductsHandler(IRepository<Product> repository, ICacheService cacheService, ILogger<GetAllProductsHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PagedResult<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"products:page:{request.Page}:size:{request.PageSize}:category:{request.Category ?? "all"}:search:{request.SearchTerm ?? "all"}";

                var cachedResult = await _cacheService.GetAsync<PagedResult<ProductResponse>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Products retrieved from cache");
                    return cachedResult;
                }

                var allProducts = await _repository.GetAllAsync();
                var filteredProducts = allProducts.Where(p => !p.IsDeleted && p.IsActive);

                if (!string.IsNullOrEmpty(request.Category))
                {
                    filteredProducts = filteredProducts.Where(p =>
                        p.Category != null && p.Category.Contains(request.Category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    filteredProducts = filteredProducts.Where(p =>
                        p.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (p.Description != null && p.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                var totalRecords = filteredProducts.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

                var pagedProducts = filteredProducts
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        Category = p.Category,
                        SKU = p.SKU,
                        IsActive = p.IsActive,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToList();

                var result = new PagedResult<ProductResponse>
                {
                    Data = pagedProducts,
                    TotalRecords = totalRecords,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };

                // Cache for 5 minutes
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Products retrieved from database and cached");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }
    }

    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductResponse?>
    {
        private readonly IRepository<Product> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<GetProductByIdHandler> _logger;

        public GetProductByIdHandler(IRepository<Product> repository, ICacheService cacheService, ILogger<GetProductByIdHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"product:{request.Id}";

                var cachedProduct = await _cacheService.GetAsync<ProductResponse>(cacheKey);
                if (cachedProduct != null)
                {
                    _logger.LogInformation("Product retrieved from cache: {ProductId}", request.Id);
                    return cachedProduct;
                }

                var product = await _repository.GetByIdAsync(request.Id);
                if (product == null || product.IsDeleted)
                    return null;

                var result = new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    Category = product.Category,
                    SKU = product.SKU,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                // Cache for 10 minutes
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

                _logger.LogInformation("Product retrieved from database and cached: {ProductId}", request.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product: {ProductId}", request.Id);
                throw;
            }
        }
    }
}
