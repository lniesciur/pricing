# Inventory Domain — Unit Test Plan (SPEC-007: Device Attributes)

## Domain types identified

| Kind            | Type              | File                                                                                           |
|-----------------|-------------------|-----------------------------------------------------------------------------------------------|
| Aggregate Root  | `Device`          | `src/Modules/Inventory/Pricing.Inventory.Domain/Devices/Device.cs`                           |
| Value Object    | `DeviceAttribute` | `src/Modules/Inventory/Pricing.Inventory.Domain/Devices/DeviceAttribute.cs`                  |
| Identity        | `DeviceId`        | `src/Modules/Inventory/Pricing.Inventory.Domain/Devices/DeviceId.cs`                         |
| Domain Events   | (none defined)    | No `RaiseDomainEvent` call exists in `Device.Create`; YAGNI, no consumer registered.         |

## Scope of this plan

This plan covers only cases introduced or changed by SPEC-007.
Existing cases in `tests/Pricing.Inventory.Domain.UnitTests/Devices/DeviceTests.cs` that already pass (EanCode, Name, TypeCode, SubtypeCode, ManufacturerCode, Id, immutability) are NOT duplicated here.

---

## Section 1 — DeviceAttribute (Value Object)

`src/Modules/Inventory/Pricing.Inventory.Domain/Devices/DeviceAttribute.cs`

---

### DA-001

Priority:
P1

Rule / Invariant:
Two `DeviceAttribute` instances with identical `Name` and `Value` must compare as equal (record structural equality).

Source:
C# `record` compiler-generated value equality.

Method:
Implicit equality operator (`==`) / `Equals`

Preconditions:
None.

Inputs:
`Name = "Color"`, `Value = "Red"` for both instances.

Case:
`DeviceAttribute_WhenNameAndValueAreIdentical_InstancesAreEqual`

Arrange:
```
var first  = new DeviceAttribute("Color", "Red");
var second = new DeviceAttribute("Color", "Red");
```

Act:
`var areEqual = first == second;`

Assert:
- `areEqual` is `true`
- `first.Equals(second)` is `true`

---

### DA-002

Priority:
P2

Rule / Invariant:
Instances with different `Name` are not equal.

Source:
C# `record` value equality.

Method:
`==`

Preconditions:
None.

Inputs:
`first = new DeviceAttribute("Color", "Red")`, `second = new DeviceAttribute("Size", "Red")`

Case:
`DeviceAttribute_WhenNamesAreDifferent_InstancesAreNotEqual`

Arrange:
```
var first  = new DeviceAttribute("Color", "Red");
var second = new DeviceAttribute("Size", "Red");
```

Act:
`var areEqual = first == second;`

Assert:
- `areEqual` is `false`

---

### DA-003

Priority:
P2

Rule / Invariant:
Instances with same `Name` but different `Value` are not equal.

Source:
C# `record` value equality.

Method:
`==`

Preconditions:
None.

Inputs:
`first = new DeviceAttribute("Color", "Red")`, `second = new DeviceAttribute("Color", "Blue")`

Case:
`DeviceAttribute_WhenValuesAreDifferent_InstancesAreNotEqual`

Arrange:
```
var first  = new DeviceAttribute("Color", "Red");
var second = new DeviceAttribute("Color", "Blue");
```

Act:
`var areEqual = first == second;`

Assert:
- `areEqual` is `false`

---

### DA-004

Priority:
P1

Rule / Invariant:
`Name` property returns the value provided at construction.

Source:
Positional record property `string Name`.

Method:
Constructor / property getter

Preconditions:
None.

Inputs:
`Name = "Color"`, `Value = "Red"`

Case:
`DeviceAttribute_WhenCreated_NamePropertyReturnsProvidedValue`

Arrange:
`var attr = new DeviceAttribute("Color", "Red");`

Act:
`var name = attr.Name;`

Assert:
- `name` equals `"Color"`

---

### DA-005

Priority:
P1

Rule / Invariant:
`Value` property returns the value provided at construction.

Source:
Positional record property `string Value`.

Method:
Constructor / property getter

Preconditions:
None.

Inputs:
`Name = "Color"`, `Value = "Red"`

Case:
`DeviceAttribute_WhenCreated_ValuePropertyReturnsProvidedValue`

Arrange:
`var attr = new DeviceAttribute("Color", "Red");`

Act:
`var value = attr.Value;`

Assert:
- `value` equals `"Red"`

---

### DA-006

Priority:
P1

Rule / Invariant:
`Name` property has no public setter (immutability).

Source:
C# `record` positional property is `init`-only — no generated `set` accessor.

Method:
Reflection — `GetSetMethod()`

Preconditions:
None.

Inputs:
N/A

Case:
`DeviceAttribute_NameProperty_HasNoPublicSetter`

Arrange:
`var property = typeof(DeviceAttribute).GetProperty(nameof(DeviceAttribute.Name));`

Act:
`var setter = property!.GetSetMethod();`

Assert:
- `setter` is `null`

---

### DA-007

Priority:
P1

Rule / Invariant:
`Value` property has no public setter (immutability).

Source:
C# `record` positional property is `init`-only.

Method:
Reflection — `GetSetMethod()`

Preconditions:
None.

Inputs:
N/A

Case:
`DeviceAttribute_ValueProperty_HasNoPublicSetter`

Arrange:
`var property = typeof(DeviceAttribute).GetProperty(nameof(DeviceAttribute.Value));`

Act:
`var setter = property!.GetSetMethod();`

Assert:
- `setter` is `null`

---

## Section 2 — Device.Create — Attribute behaviour (SPEC-007)

`src/Modules/Inventory/Pricing.Inventory.Domain/Devices/Device.cs`

All cases use these baseline inputs unless a specific input is overridden:

```
eanCode          = "5901234123457"
name             = "iPhone 15 Pro 256GB Black"
typeCode         = "SMARTPHONE"
subtypeCode      = "IPHONE"
manufacturerCode = "APPLE"
```

---

### DC-001

Priority:
P0

Rule / Invariant:
Attributes are optional. Omitting the `attributes` parameter (using the default `null`) is valid and produces a device with an empty `Attributes` collection.

Source:
`IReadOnlyList<DeviceAttribute>? attributes = null` default parameter; constructor guard `if (attributes is not null)`.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes` parameter omitted (uses default `null`).

Case:
`Create_WhenAttributesIsNull_AttributesPropertyIsEmpty`

Arrange:
No additional setup.

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE");`

Assert:
- `device.Attributes.Count` equals `0`
- No exception thrown

---

### DC-002

Priority:
P0

Rule / Invariant:
An explicitly supplied empty list is treated identically to `null`. The pattern guard `if (attributes is { Count: > 0 })` skips the duplicate check entirely; the device is created with an empty `Attributes` collection.

Source:
`if (attributes is { Count: > 0 })` guard — empty list short-circuits before validation.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = new List<DeviceAttribute>()`

Case:
`Create_WhenAttributesIsEmptyList_AttributesPropertyIsEmpty`

Arrange:
`var attributes = new List<DeviceAttribute>();`

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);`

Assert:
- `device.Attributes.Count` equals `0`
- No exception thrown

Notes:
This case is distinct from DC-001 because it exercises the `Count: > 0` branch of the pattern guard, confirming the guard does not crash on an empty (non-null) list.

---

### DC-003

Priority:
P0

Rule / Invariant:
A single attribute cannot be a duplicate of itself. Creating a device with exactly one attribute is always valid (no duplicate possible).

Source:
`.Where(g => g.Count() > 1)` — a group of one does not qualify as a duplicate.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("Color", "Red")]`

Case:
`Create_WhenSingleAttributeProvided_ReturnsDeviceWithOneAttribute`

Arrange:
`var attributes = new List<DeviceAttribute> { new("Color", "Red") };`

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);`

Assert:
- `device.Attributes.Count` equals `1`
- `device.Attributes[0].Name` equals `"Color"`
- `device.Attributes[0].Value` equals `"Red"`
- No exception thrown

---

### DC-004

Priority:
P0

Rule / Invariant:
Multiple attributes with unique names (case-insensitively distinct) produce a valid device containing all supplied attributes.

Source:
`GroupBy(...).Where(g => g.Count() > 1)` returns empty when all names are distinct — no exception is thrown.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
```
attributes = [
  new DeviceAttribute("Color",       "Black"),
  new DeviceAttribute("Storage",     "256GB"),
  new DeviceAttribute("Connectivity","5G")
]
```

Case:
`Create_WhenAttributesHaveUniqueNames_ReturnsDeviceWithAllAttributes`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("Color",        "Black"),
    new("Storage",      "256GB"),
    new("Connectivity", "5G")
};
```

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);`

Assert:
- `device.Attributes.Count` equals `3`
- `device.Attributes[0].Name` equals `"Color"`
- `device.Attributes[1].Name` equals `"Storage"`
- `device.Attributes[2].Name` equals `"Connectivity"`
- No exception thrown

---

### DC-005

Priority:
P0

Rule / Invariant:
Two attributes with the same `Name` (exact same case) cause `Device.Create` to throw `InvalidOperationException`. The exception message names the offending attribute.

Source:
`throw new InvalidOperationException($"Duplicate attribute names: {string.Join(", ", duplicates)}")`

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("Color", "Red"), new DeviceAttribute("Color", "Blue")]`

Case:
`Create_WhenAttributesHaveDuplicateNamesSameCase_ThrowsInvalidOperationException`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("Color", "Red"),
    new("Color", "Blue")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Duplicate attribute names: Color"`

Notes:
The group key is taken from the first element encountered. Both attributes are named `"Color"` (same case), so the key is `"Color"`.

---

### DC-006

Priority:
P0

Rule / Invariant:
Two attributes whose names differ only in case (`"color"` vs `"Color"`) are treated as duplicates by `StringComparer.OrdinalIgnoreCase`. `Device.Create` throws `InvalidOperationException`.

Source:
`GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)`

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("color", "Red"), new DeviceAttribute("Color", "Blue")]`

Case:
`Create_WhenAttributesHaveDuplicateNamesDifferentCase_ThrowsInvalidOperationException`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("color", "Red"),
    new("Color", "Blue")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Duplicate attribute names: color"` (group key comes from the first element: `"color"`)

Notes:
The expected message key is `"color"` (lower), not `"Color"`, because GroupBy takes the first-encountered string as the group key.

---

### DC-007

Priority:
P1

Rule / Invariant:
`OrdinalIgnoreCase` treats `"color"` and `"COLOR"` as duplicates.

Source:
`StringComparer.OrdinalIgnoreCase`

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("color", "Red"), new DeviceAttribute("COLOR", "Blue")]`

Case:
`Create_WhenAttributesHaveDuplicateNamesAllUpperCase_ThrowsInvalidOperationException`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("color", "Red"),
    new("COLOR", "Blue")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Duplicate attribute names: color"`

---

### DC-008

Priority:
P1

Rule / Invariant:
When multiple independent duplicate pairs exist in the same attribute list, all duplicate names appear in the exception message, separated by `", "`.

Source:
`string.Join(", ", duplicates)` where `duplicates` is a list of all offending group keys.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
```
attributes = [
  new DeviceAttribute("color", "Red"),
  new DeviceAttribute("Color", "Blue"),
  new DeviceAttribute("size",  "L"),
  new DeviceAttribute("Size",  "XL")
]
```

Case:
`Create_WhenAttributesHaveMultipleDuplicateNamePairs_ExceptionMessageContainsAllDuplicateNames`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("color", "Red"),
    new("Color", "Blue"),
    new("size",  "L"),
    new("Size",  "XL")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Duplicate attribute names: color, size"` (both group keys in encounter order; first element for each pair is `"color"` and `"size"`)

---

### DC-009

Priority:
P1

Rule / Invariant:
A duplicate found among three or more attributes (where only one name is duplicated and the rest are unique) still throws `InvalidOperationException` naming exactly the one duplicate.

Source:
`.Where(g => g.Count() > 1)` — only the offending group is included.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
```
attributes = [
  new DeviceAttribute("Color", "Red"),
  new DeviceAttribute("Size",  "256GB"),
  new DeviceAttribute("color", "Blue")
]
```

Case:
`Create_WhenOneAttributeNameIsDuplicatedAmongMultiple_ThrowsInvalidOperationException`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("Color", "Red"),
    new("Size",  "256GB"),
    new("color", "Blue")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Duplicate attribute names: Color"` (first-encountered group key is `"Color"`; `"Size"` is unique and absent from the message)

---

### DC-010

Priority:
P1

Rule / Invariant:
`Attributes` property preserves the original insertion order of the supplied list.

Source:
`_attributes.AddRange(attributes)` — `List<T>.AddRange` preserves enumeration order.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
```
attributes = [
  new DeviceAttribute("Storage",      "256GB"),
  new DeviceAttribute("Color",        "Black"),
  new DeviceAttribute("Connectivity", "5G")
]
```

Case:
`Create_WhenAttributesProvided_AttributesPropertyPreservesInputOrder`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("Storage",      "256GB"),
    new("Color",        "Black"),
    new("Connectivity", "5G")
};
```

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);`

Assert:
- `device.Attributes[0].Name` equals `"Storage"`
- `device.Attributes[1].Name` equals `"Color"`
- `device.Attributes[2].Name` equals `"Connectivity"`

---

### DC-011

Priority:
P1

Rule / Invariant:
`Device.Create` does not raise any domain events when it succeeds (no `RaiseDomainEvent` call exists in the method).

Source:
Absence of `RaiseDomainEvent` in `Device.Create`.

Method:
`Device.Create` + `AggregateRoot.PopDomainEvents`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("Color", "Black")]`

Case:
`Create_WhenAttributesProvided_NoDomainEventsRaised`

Arrange:
`var attributes = new List<DeviceAttribute> { new("Color", "Black") };`

Act:
```
var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);
var events = device.PopDomainEvents();
```

Assert:
- `events.Count` equals `0`

---

### DC-012

Priority:
P1

Rule / Invariant:
`Device.Create` does not raise any domain events when it succeeds without attributes.

Source:
Absence of `RaiseDomainEvent` in `Device.Create`.

Method:
`Device.Create` + `AggregateRoot.PopDomainEvents`

Preconditions:
None.

Inputs:
`attributes` omitted.

Case:
`Create_WhenNoAttributesProvided_NoDomainEventsRaised`

Arrange:
No additional setup.

Act:
```
var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE");
var events = device.PopDomainEvents();
```

Assert:
- `events.Count` equals `0`

---

### DC-013

Priority:
P0

Rule / Invariant:
An attribute with an empty string `Name` is rejected by `Device.Create` before the duplicate check runs. Empty attribute names are not permitted.

Source:
`var emptyNames = attributes.Where(a => string.IsNullOrWhiteSpace(a.Name)); if (emptyNames.Count > 0) throw new InvalidOperationException("Attribute name must not be empty or whitespace.");`

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("", "Red"), new DeviceAttribute("", "Blue")]`

Case:
`Create_WhenAttributeHasEmptyStringName_ThrowsInvalidOperationException`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("", "Red"),
    new("", "Blue")
};
```

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Attribute name must not be empty or whitespace."`

Notes:
The empty-name guard fires before the duplicate check, so the exception message is from the empty-name guard, not the duplicate guard.

---

### DC-014

Priority:
P0

Rule / Invariant:
A single attribute with a whitespace-only `Name` is rejected — the empty-name guard covers `string.IsNullOrWhiteSpace`, not just `string.IsNullOrEmpty`.

Source:
`string.IsNullOrWhiteSpace(a.Name)` guard in `Device.Create`.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("   ", "Red")]`

Case:
`Create_WhenAttributeHasWhitespaceOnlyName_ThrowsInvalidOperationException`

Arrange:
`var attributes = new List<DeviceAttribute> { new("   ", "Red") };`

Act:
`var ex = Assert.Throws<InvalidOperationException>(() => Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes));`

Assert:
- Exception type is `InvalidOperationException`
- `ex.Message` equals `"Attribute name must not be empty or whitespace."`

---

### DC-015

Priority:
P2

Rule / Invariant:
An attribute with an empty string `Value` is not validated; the device is created successfully.

Source:
Absence of an empty/whitespace guard for `Value` in both `DeviceAttribute` and `Device.Create`.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("Color", "")]`

Case:
`Create_WhenAttributeHasEmptyStringValue_DeviceIsCreated`

Arrange:
`var attributes = new List<DeviceAttribute> { new("Color", "") };`

Act:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes);`

Assert:
- `device.Attributes.Count` equals `1`
- `device.Attributes[0].Value` equals `""`
- No exception thrown

Notes:
Inferred observation. Confirm with the developer whether an empty `Value` is a valid business state.

---

### DC-016

Priority:
P3

Rule / Invariant:
`PopDomainEvents` clears the internal event list; a second call on the same aggregate returns an empty collection.

Source:
`_domainEvents.Clear()` in `AggregateRoot.PopDomainEvents`.

Method:
`AggregateRoot.PopDomainEvents` (tested via `Device`)

Preconditions:
A valid `Device` instance exists.

Inputs:
N/A

Case:
`Create_WhenPopDomainEventsCalledTwice_SecondCallReturnsEmptyList`

Arrange:
`var device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE");`

Act:
```
device.PopDomainEvents();                      // first call
var secondResult = device.PopDomainEvents();   // second call
```

Assert:
- `secondResult.Count` equals `0`

Notes:
Relevant as a regression guard for the dequeue contract of `AggregateRoot`. If a real domain event is added to `Device` in the future, this case ensures re-polling does not re-deliver it.

---

## Section 3 — Aggregate invariants (cross-method)

### AI-001

Priority:
P0

Rule / Invariant:
When `Device.Create` throws due to duplicate attribute names, no `Device` instance is returned. The aggregate never reaches an inconsistent state because it is never constructed.

Source:
`throw new InvalidOperationException(...)` before `return new(...)`.

Method:
`Device.Create`

Preconditions:
None.

Inputs:
`attributes = [new DeviceAttribute("Color", "Red"), new DeviceAttribute("color", "Blue")]`

Case:
`Create_WhenDuplicateAttributeNamesDetected_NoDeviceInstanceReturned`

Arrange:
```
var attributes = new List<DeviceAttribute>
{
    new("Color", "Red"),
    new("color", "Blue")
};
Device? device = null;
```

Act:
```
var ex = Record.Exception(
    () => device = Device.Create("5901234123457", "iPhone 15 Pro 256GB Black", "SMARTPHONE", "IPHONE", "APPLE", attributes)
);
```

Assert:
- `ex` is `InvalidOperationException`
- `device` is `null` (no partial instance was assigned before the exception)

---

## Coverage gaps

The following issues were identified from the code. They are inferred observations and require confirmation before adding enforcement.

### Gap 1 — No guard for empty or whitespace `Name` in `DeviceAttribute`

`DeviceAttribute` is a plain record with no validation. `Device.Create` validates uniqueness but does not check whether a `Name` is `null`, `""`, or `"   "`. A device with an attribute named `""` or `"   "` can currently be created. If the business rule is that attribute names must be non-blank, a guard (`string.IsNullOrWhiteSpace`) should be added and test cases DC-013 and DC-014 should be converted to P0 failure cases.

Recommendation: Confirm with the developer what the minimum valid `Name` is.

### Gap 2 — No guard for empty or whitespace `Value` in `DeviceAttribute`

`DeviceAttribute.Value` is unconstrained. An empty or whitespace `Value` passes through silently (DC-015). If an empty value is not a valid business state, add a guard.

Recommendation: Confirm with the developer whether an empty attribute value is acceptable.

### Gap 3 — No upper bound on the number of attributes

`Device.Create` accepts any number of attributes. There is no maximum. This may be intentional for a catalogue system, but if a practical upper bound exists (e.g., 50 attributes per device), it is currently unenforced.

Recommendation: Confirm with the developer whether a maximum should be imposed.

### Gap 4 — No guard for whitespace-only duplicate names

A name of `"   "` (three spaces) is distinct from `""` and from `"Color"` under `OrdinalIgnoreCase`. Two attributes both named `"   "` would be caught as duplicates, but the resulting exception message would be `"Duplicate attribute names:    "` which is hard to read. This is a cosmetic concern rather than a correctness bug, but it surfaces the missing upstream validation.

### Gap 5 — `DeviceAttribute.Name` nullability

`string Name` is a non-nullable reference type, so passing `null` is a compile-time error. No runtime test is needed. This gap is flagged only to confirm that the C# nullability annotations are trusted and that no `null!` suppression is used upstream.
