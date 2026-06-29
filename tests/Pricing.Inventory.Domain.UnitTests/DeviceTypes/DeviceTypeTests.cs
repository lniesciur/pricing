using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Domain.DeviceTypes.Events;

namespace Pricing.Inventory.Domain.UnitTests.DeviceTypes;

public class DeviceTypeTests
{
    private const string TypeCode = "SMARTPHONE";
    private const string TypeName = "Smartphone";
    private const string SubtypeCode = "IPHONE";
    private const string SubtypeName = "iPhone";

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    [Fact]
    public void Create_WhenValidCodeAndName_SetsCodeProperty()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Assert
        Assert.Equal(TypeCode, deviceType.Code);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_SetsNameProperty()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Assert
        Assert.Equal(TypeName, deviceType.Name);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_AssignsNonEmptyId()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Assert
        Assert.NotNull(deviceType.Id);
        Assert.NotEqual(Guid.Empty, deviceType.Id.Value);
    }

    [Fact]
    public void Create_WhenCalled_SubtypesListIsEmpty()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Assert
        Assert.Empty(deviceType.Subtypes);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_RaisesExactlyOneEvent()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        var events = deviceType.PopDomainEvents();

        // Assert
        Assert.Single(events);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_RaisesDeviceTypeCreatedEvent()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        var events = deviceType.PopDomainEvents();

        // Assert
        Assert.IsType<DeviceTypeCreated>(events[0]);
    }

    [Fact]
    public void Create_WhenValidCodeAndName_DeviceTypeCreatedEventCarriesCorrectData()
    {
        // Act
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        var events = deviceType.PopDomainEvents();
        var @event = Assert.IsType<DeviceTypeCreated>(events[0]);

        // Assert
        Assert.Equal(deviceType.Id, @event.Id);
        Assert.Equal(TypeCode, @event.Code);
        Assert.Equal(TypeName, @event.Name);
    }

    // -------------------------------------------------------------------------
    // Code immutability
    // -------------------------------------------------------------------------

    [Fact]
    public void Code_PropertyHasNoPublicSetter()
    {
        // Arrange
        var property = typeof(DeviceType).GetProperty(nameof(DeviceType.Code));

        // Assert — GetSetMethod() returns null when there is no public setter
        Assert.Null(property!.GetSetMethod());
    }

    // -------------------------------------------------------------------------
    // UpdateName
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateName_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        var result = deviceType.UpdateName("Updated Smartphone");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateName_WhenCalled_ChangesNameToNewValue()
    {
        // Arrange
        const string updatedName = "Updated Smartphone";
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        deviceType.UpdateName(updatedName);

        // Assert
        Assert.Equal(updatedName, deviceType.Name);
    }

    [Fact]
    public void UpdateName_WhenCalled_DoesNotAlterCodeProperty()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        deviceType.UpdateName("Updated Smartphone");

        // Assert
        Assert.Equal(TypeCode, deviceType.Code);
    }

    [Theory]
    [InlineData("Tablet")]
    [InlineData("Feature Phone")]
    [InlineData("A")]
    public void UpdateName_WithVariousValidNames_ChangesNameSuccessfully(string newName)
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        var result = deviceType.UpdateName(newName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, deviceType.Name);
    }

    // -------------------------------------------------------------------------
    // AddSubtype
    // -------------------------------------------------------------------------

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_ReturnsOkResult()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        var result = deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_AddsOneEntryToSubtypesList()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Assert
        Assert.Single(deviceType.Subtypes);
    }

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_SubtypeHasCorrectCodeAndName()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Assert
        var subtype = deviceType.Subtypes[0];
        Assert.Equal(SubtypeCode, subtype.Code);
        Assert.Equal(SubtypeName, subtype.Name);
    }

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_RaisesExactlyOneEvent()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.PopDomainEvents(); // discard DeviceTypeCreated

        // Act
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        var events = deviceType.PopDomainEvents();

        // Assert
        Assert.Single(events);
    }

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_RaisesDeviceSubtypeAddedEvent()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.PopDomainEvents(); // discard DeviceTypeCreated

        // Act
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        var events = deviceType.PopDomainEvents();

        // Assert
        Assert.IsType<DeviceSubtypeAdded>(events[0]);
    }

    [Fact]
    public void AddSubtype_WhenCodeIsUnique_DeviceSubtypeAddedEventCarriesCorrectData()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.PopDomainEvents(); // discard DeviceTypeCreated

        // Act
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        var events = deviceType.PopDomainEvents();
        var @event = Assert.IsType<DeviceSubtypeAdded>(events[0]);

        // Assert
        Assert.Equal(deviceType.Id, @event.TypeId);
        Assert.Equal(SubtypeCode, @event.Code);
        Assert.Equal(SubtypeName, @event.Name);
        // SubtypeId in the event must match the id of the newly added subtype
        Assert.Equal(deviceType.Subtypes[0].Id, @event.SubtypeId);
    }

    [Fact]
    public void AddSubtype_WhenMultipleUniqueCodesAdded_AllSubtypesPresentInList()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        deviceType.AddSubtype("IPHONE", "iPhone");
        deviceType.AddSubtype("ANDROID", "Android");
        deviceType.AddSubtype("WINDOWS", "Windows Phone");

        // Assert
        Assert.Equal(3, deviceType.Subtypes.Count);
    }

    [Fact]
    public void AddSubtype_WhenCodeAlreadyExists_ReturnsFailResult()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        var result = deviceType.AddSubtype(SubtypeCode, "Duplicate");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void AddSubtype_WhenCodeAlreadyExists_ErrorMessageContainsSubtypeCode()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        var result = deviceType.AddSubtype(SubtypeCode, "Duplicate");

        // Assert
        Assert.Contains(SubtypeCode, result.Error);
    }

    [Fact]
    public void AddSubtype_WhenCodeAlreadyExists_DoesNotAddDuplicateSubtypeToList()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        deviceType.AddSubtype(SubtypeCode, "Duplicate");

        // Assert
        Assert.Single(deviceType.Subtypes);
    }

    [Fact]
    public void AddSubtype_WhenCodeAlreadyExists_DoesNotRaiseDomainEvent()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        deviceType.PopDomainEvents(); // discard all prior events

        // Act
        deviceType.AddSubtype(SubtypeCode, "Duplicate");
        var events = deviceType.PopDomainEvents();

        // Assert
        Assert.Empty(events);
    }

    // -------------------------------------------------------------------------
    // UpdateSubtypeName
    // -------------------------------------------------------------------------

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeExists_ReturnsOkResult()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        var result = deviceType.UpdateSubtypeName(SubtypeCode, "Updated iPhone");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeExists_ChangesSubtypeNameToNewValue()
    {
        // Arrange
        const string updatedName = "Updated iPhone";
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        deviceType.UpdateSubtypeName(SubtypeCode, updatedName);

        // Assert
        Assert.Equal(updatedName, deviceType.Subtypes[0].Name);
    }

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeExists_DoesNotChangeSubtypeCode()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        deviceType.UpdateSubtypeName(SubtypeCode, "Updated Name");

        // Assert
        Assert.Equal(SubtypeCode, deviceType.Subtypes[0].Code);
    }

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeNotFound_ReturnsFailResult()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        var result = deviceType.UpdateSubtypeName("NONEXISTENT", "Some Name");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeNotFound_ErrorMessageContainsSubtypeCode()
    {
        // Arrange
        const string nonExistentCode = "NONEXISTENT";
        var deviceType = DeviceType.Create(TypeCode, TypeName);

        // Act
        var result = deviceType.UpdateSubtypeName(nonExistentCode, "Some Name");

        // Assert
        Assert.Contains(nonExistentCode, result.Error);
    }

    [Fact]
    public void UpdateSubtypeName_WhenSubtypeNotFound_SubtypesListRemainsUnchanged()
    {
        // Arrange
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);

        // Act
        deviceType.UpdateSubtypeName("NONEXISTENT", "Some Name");

        // Assert — existing subtype is untouched
        Assert.Single(deviceType.Subtypes);
        Assert.Equal(SubtypeName, deviceType.Subtypes[0].Name);
    }

    [Fact]
    public void UpdateSubtypeName_WhenTwoSubtypesExist_OnlyTargetSubtypeNameChanges()
    {
        // Arrange
        const string secondCode = "ANDROID";
        const string secondName = "Android";
        var deviceType = DeviceType.Create(TypeCode, TypeName);
        deviceType.AddSubtype(SubtypeCode, SubtypeName);
        deviceType.AddSubtype(secondCode, secondName);

        // Act
        deviceType.UpdateSubtypeName(SubtypeCode, "New iPhone");

        // Assert
        Assert.Equal("New iPhone", deviceType.Subtypes.First(s => s.Code == SubtypeCode).Name);
        Assert.Equal(secondName, deviceType.Subtypes.First(s => s.Code == secondCode).Name);
    }
}
