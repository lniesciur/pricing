using Hangfire;
using Pricing.Import.Application;
using Pricing.Import.Application.UseCases.ProcessDeviceImport;

namespace Pricing.Import.Infrastructure;

public class HangfireImportJobScheduler(IBackgroundJobClient backgroundJobClient) : IImportJobScheduler
{
    public void EnqueueDeviceImportProcessing(Guid jobId) =>
        backgroundJobClient.Enqueue<ProcessDeviceImportUseCase>(h => h.ExecuteAsync(jobId, CancellationToken.None));
}
