using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class UpdateDeviceSubtypeValidator : Validator<UpdateDeviceSubtypeRequest>
{
    public UpdateDeviceSubtypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
