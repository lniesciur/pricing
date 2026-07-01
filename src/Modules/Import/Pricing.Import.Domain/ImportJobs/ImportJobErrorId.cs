namespace Pricing.Import.Domain.ImportJobs;

public record ImportJobErrorId(Guid Value)
{
    public static ImportJobErrorId New() => new(Guid.NewGuid());
}
