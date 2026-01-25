using Application.Features.Products.Commands.CreateProduct;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.Features.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "Description", 99.99m, 10);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        // Arrange
        var command = new CreateProductCommand(name!, "Description", 99.99m, 10);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        // Arrange
        string longName = new string('a', 201);
        var command = new CreateProductCommand(longName, "Description", 99.99m, 10);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        string longDescription = new string('a', 1001);
        var command = new CreateProductCommand("Product", longDescription, 99.99m, 10);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 1000 characters.");
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProductCommand("Product", "Description", -1m, 10);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be non-negative.");
    }

    [Fact]
    public void Validate_WithNegativeStockQuantity_ShouldHaveError()
    {
        // Arrange
        var command = new CreateProductCommand("Product", "Description", 99.99m, -1);

        // Act
        TestValidationResult<CreateProductCommand>? result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
            .WithErrorMessage("Stock quantity must be non-negative.");
    }
}
