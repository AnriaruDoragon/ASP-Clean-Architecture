using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        const string name = "Test Product";
        const string description = "Test Description";
        const decimal price = 99.99m;
        const int stockQuantity = 10;

        // Act
        var product = Product.Create(name, description, price, stockQuantity);

        // Assert
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.StockQuantity.Should().Be(stockQuantity);
        product.IsActive.Should().BeTrue();
        product.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowDomainException(string? name)
    {
        // Act
        Func<Product> act = () => Product.Create(name!, "Description", 10m, 5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Product name cannot be empty.");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowDomainException()
    {
        // Act
        Func<Product> act = () => Product.Create("Product", "Description", -10m, 5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Product price cannot be negative.");
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowDomainException()
    {
        // Act
        Func<Product> act = () => Product.Create("Product", "Description", 10m, -5);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Stock quantity cannot be negative.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = Product.Create("Original", "Original Desc", 50m, 10);
        const string newName = "Updated";
        const string newDescription = "Updated Desc";
        const decimal newPrice = 75m;

        // Act
        product.Update(newName, newDescription, newPrice);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void AdjustStock_WithValidPositiveQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = Product.Create("Product", null, 10m, 10);

        // Act
        product.AdjustStock(5);

        // Assert
        product.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void AdjustStock_WithValidNegativeQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = Product.Create("Product", null, 10m, 10);

        // Act
        product.AdjustStock(-3);

        // Assert
        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void AdjustStock_WithInsufficientStock_ShouldThrowDomainException()
    {
        // Arrange
        var product = Product.Create("Product", null, 10m, 5);

        // Act
        Action act = () => product.AdjustStock(-10);

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Insufficient stock.");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var product = Product.Create("Product", null, 10m, 5);

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var product = Product.Create("Product", null, 10m, 5);
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }
}
