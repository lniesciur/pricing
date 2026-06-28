using FastEndpoints;
using FluentValidation;
using Pricing.Inventory.Contracts.Example;

namespace Pricing.Inventory.Api.Endpoints.Example;

public class CreateExampleValidator : Validator<CreateExampleRequest>
{
    public CreateExampleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
