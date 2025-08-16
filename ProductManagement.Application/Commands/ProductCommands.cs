using MediatR;
using ProductManagement.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Application.Commands
{
    public class CreateProductCommand : IRequest<ProductResponse>
    {
        public ProductCreateRequest Product { get; set; }

        public CreateProductCommand(ProductCreateRequest product)
        {
            Product = product;
        }
    }

    public class UpdateProductCommand : IRequest<ProductResponse>
    {
        public ProductUpdateRequest Product { get; set; }

        public UpdateProductCommand(ProductUpdateRequest product)
        {
            Product = product;
        }
    }

    public class DeleteProductCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}
