using NSubstitute;
using Pricing.Inventory.Application.UseCases.GetManufacturers;
using Pricing.Inventory.Domain.Manufacturers;

namespace Pricing.Inventory.Application.UnitTests.UseCases.GetManufacturers;

public sealed class GetManufacturersUseCaseTests
{
    private readonly IManufacturerRepository _repository = Substitute.For<IManufacturerRepository>();
    private readonly GetManufacturersUseCase _sut;

    public GetManufacturersUseCaseTests()
    {
        _sut = new GetManufacturersUseCase(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoManufacturers_ReturnsEmptyList()
    {
        // Arrange
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Manufacturer>().AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Manufacturers);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturersExist_ReturnsMappedDtos()
    {
        // Arrange
        var apple = Manufacturer.Create("APPLE", "Apple Inc.");
        var samsung = Manufacturer.Create("SAMSUNG", "Samsung Electronics");

        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Manufacturer> { apple, samsung }.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Manufacturers.Count);

        var appleDto = result.Value.Manufacturers.Single(m => m.Code == "APPLE");
        Assert.Equal("Apple Inc.", appleDto.Name);

        var samsungDto = result.Value.Manufacturers.Single(m => m.Code == "SAMSUNG");
        Assert.Equal("Samsung Electronics", samsungDto.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSingleManufacturerExists_ReturnsSingleDto()
    {
        // Arrange
        var sony = Manufacturer.Create("SONY", "Sony Corporation");
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Manufacturer> { sony }.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Manufacturers);
        Assert.Equal("SONY", dto.Code);
        Assert.Equal("Sony Corporation", dto.Name);
    }

    [Fact]
    public async Task ExecuteAsync_AlwaysReturnsSuccess()
    {
        // Arrange — empty repository is a valid state, not an error
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Manufacturer>().AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturersExist_DtoCountMatchesRepositoryCount()
    {
        // Arrange
        var manufacturers = new List<Manufacturer>
        {
            Manufacturer.Create("APPLE", "Apple Inc."),
            Manufacturer.Create("SAMSUNG", "Samsung Electronics"),
            Manufacturer.Create("SONY", "Sony Corporation"),
        };

        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(manufacturers.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(manufacturers.Count, result.Value!.Manufacturers.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManufacturersExist_DtoCodeAndNameMatchDomainEntity()
    {
        // Arrange
        var lg = Manufacturer.Create("LG", "LG Electronics");
        _repository.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Manufacturer> { lg }.AsReadOnly());

        // Act
        var result = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!.Manufacturers);
        Assert.Equal(lg.Code, dto.Code);
        Assert.Equal(lg.Name, dto.Name);
    }
}
