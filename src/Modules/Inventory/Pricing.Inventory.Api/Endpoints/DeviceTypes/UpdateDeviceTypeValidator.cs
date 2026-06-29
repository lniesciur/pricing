using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.DeviceTypes;

namespace Pricing.Inventory.Api.Endpoints.DeviceTypes;

public class UpdateDeviceTypeValidator : Validator<UpdateDeviceTypeRequest>
{
    public UpdateDeviceTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
