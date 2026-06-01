using BuildingBlocks.CQRS;
using Catalog.Domain.Products.Entities;

namespace Catalog.Application.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    List<string> Category,
    string Description,
    decimal Price,
    string ImageFile,
    int StockAmount) : ICommand<CreateProductResult>;

public record CreateProductResult(Guid Id);

internal class CreateProductHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Category = command.Category,
            ImageFile = command.ImageFile,
            Description = command.Description,
            Price = command.Price,
            StockAmount = command.StockAmount
        };

        return new CreateProductResult(product.Id);
    }
}
