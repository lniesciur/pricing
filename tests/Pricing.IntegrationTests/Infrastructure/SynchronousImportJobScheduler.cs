using Microsoft.Extensions.DependencyInjection;
using Pricing.Import.Application;
using Pricing.Import.Application.UseCases.ProcessDeviceImport;

namespace Pricing.IntegrationTests.Infrastructure;

// Replaces HangfireImportJobScheduler in tests: processes the job synchronously so that
// every upload test is also an implicit end-to-end test — no polling required.
internal sealed class SynchronousImportJobScheduler(IServiceScopeFactory scopeFactory) : IImportJobScheduler
{
    public void EnqueueDeviceImportProcessing(Guid jobId)
    {
        using var scope = scopeFactory.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<ProcessDeviceImportUseCase>();
        useCase.ExecuteAsync(jobId).GetAwaiter().GetResult();
    }
}
