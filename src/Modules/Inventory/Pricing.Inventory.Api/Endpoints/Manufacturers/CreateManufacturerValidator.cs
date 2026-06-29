using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.Manufacturers;

namespace Pricing.Inventory.Api.Endpoints.Manufacturers;

public class CreateManufacturerValidator : Validator<CreateManufacturerRequest>
{
    public CreateManufacturerValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
