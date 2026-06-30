using FastEndpoints;
using FluentValidation;

namespace Pricing.Import.Api.Endpoints.DeviceImports;

public class UploadDeviceImportValidator : Validator<UploadDeviceImportRequest>
{
    public UploadDeviceImportValidator()
    {
        RuleFor(r => r.File)
            .NotNull()
            .WithMessage("File is required.");

        When(r => r.File is not null, () =>
        {
            RuleFor(r => r.File!.FileName)
                .NotEmpty()
                .Must(name => name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
                              name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only .csv and .xlsx files are supported.");
        });
    }
}
