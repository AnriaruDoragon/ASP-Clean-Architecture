using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Products.Commands.CreateProduct;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Features.Products;

public class CreateProductCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly DbSet<Product> _products;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _products = Substitute.For<DbSet<Product>>();
        _context.Products.Returns(_products);
        _handler = new CreateProductCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProductAndReturnId()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: "Test Description",
            Price: 99.99m,
            StockQuantity: 10);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _products.Received(1).Add(Arg.Is<Product>(p =>
            p.Name == command.Name &&
            p.Description == command.Description &&
            p.Price == command.Price &&
            p.StockQuantity == command.StockQuantity));

        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Test Product",
            Description: null,
            Price: 49.99m,
            StockQuantity: 5);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _products.Received(1).Add(Arg.Is<Product>(p =>
            p.Name == command.Name &&
            p.Description == null));
    }
}
