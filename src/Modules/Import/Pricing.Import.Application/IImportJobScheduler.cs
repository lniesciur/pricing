namespace Pricing.Import.Application;

public interface IImportJobScheduler
{
    void EnqueueDeviceImportProcessing(Guid jobId);
}
