using FhirOpenEhrBridge.Application.DependencyInjection;
using FhirOpenEhrBridge.Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FHIR-OpenEHR-Bridge API",
        Version = "v1",
        Description = "Bidirectional translation engine between HL7 FHIR and openEHR payloads."
    });
});

// Compose the Clean Architecture layers.
builder.Services.AddBridgeApplication();
builder.Services.AddBridgeInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

/// <summary>
/// Exposed so integration/BDD tests can spin up the API in-memory via
/// <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program;
