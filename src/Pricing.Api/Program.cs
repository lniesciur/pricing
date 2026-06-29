using FastEndpoints;
using FastEndpoints.Swagger;
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

app.UseStaticFiles();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Errors.UseProblemDetails();
});
app.UseSwaggerGen(uiConfig: ui =>
{
    ui.DocExpansion = "list";
    ui.DefaultModelsExpandDepth = 1;
});

app.Run();

public partial class Program;
