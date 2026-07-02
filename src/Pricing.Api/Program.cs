using FastEndpoints;
using FastEndpoints.Swagger;
using Hangfire;
using Pricing.Import.Api;
using Pricing.Inventory.Api;
using Pricing.Rating.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Pricing API";
            s.Version = "v1";
        };
    });

builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddImportModule(builder.Configuration);
builder.Services.AddRatingModule(builder.Configuration);

var app = builder.Build();

await app.Services.StartInventoryModuleAsync();
await app.Services.StartImportModuleAsync();

app.UseHangfireDashboard("/hangfire");
app.UseStaticFiles();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Errors.UseProblemDetails();
    c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
app.UseSwaggerGen(uiConfig: ui =>
{
    ui.DocExpansion = "list";
    ui.DefaultModelsExpandDepth = 1;
});

app.Run();

public partial class Program;
