using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class AddDeviceSubtypeValidator : Validator<AddDeviceSubtypeRequest>
{
    public AddDeviceSubtypeValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
