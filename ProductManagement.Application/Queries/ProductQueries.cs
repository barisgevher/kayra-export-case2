using MediatR;
using ProductManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Application.Queries
{
    public class GetAllProductsQuery : IRequest<PagedResult<ProductResponse>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }

        public GetAllProductsQuery(int page, int pageSize, string? category = null, string? searchTerm = null)
        {
            Page = page;
            PageSize = pageSize;
            Category = category;
            SearchTerm = searchTerm;
        }
    }

    public class GetProductByIdQuery : IRequest<ProductResponse?>
    {
        public int Id { get; set; }

        public GetProductByIdQuery(int id)
        {
            Id = id;
        }
    }
}
