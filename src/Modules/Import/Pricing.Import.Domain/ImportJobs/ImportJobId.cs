namespace Pricing.Import.Domain.ImportJobs;

public record ImportJobId(Guid Value)
{
    public static ImportJobId New() => new(Guid.NewGuid());
}
