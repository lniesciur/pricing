namespace Pricing.Import.Application.FileReading;

public interface IRowValidator<TRow>
{
    IEnumerable<string> Validate(TRow row);
}
