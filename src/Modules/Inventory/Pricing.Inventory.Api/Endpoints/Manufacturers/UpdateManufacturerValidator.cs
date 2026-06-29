using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.Manufacturers;

namespace Pricing.Inventory.Api.Endpoints.Manufacturers;

public class UpdateManufacturerValidator : Validator<UpdateManufacturerRequest>
{
    public UpdateManufacturerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
