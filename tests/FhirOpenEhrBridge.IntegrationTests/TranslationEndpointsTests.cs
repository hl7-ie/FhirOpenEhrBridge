using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FhirOpenEhrBridge.IntegrationTests;

/// <summary>
/// End-to-end HTTP tests that boot the real ASP.NET Core pipeline in-memory via
/// <see cref="WebApplicationFactory{TEntryPoint}"/>. No external FHIR server or
/// CDR is required — only the translation endpoints are exercised.
/// </summary>
public sealed class TranslationEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TranslationEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
    }

    [Fact]
    public async Task FhirToOpenEhr_ValidPatient_Returns200WithComposition()
    {
        var client = _factory.CreateClient();
        var patient = """
            {
              "resourceType": "Patient",
              "id": "int-001",
              "name": [{ "family": "Doe", "given": ["Jane"] }],
              "gender": "female",
              "birthDate": "1990-01-01"
            }
            """;
        using var content = new StringContent(patient, Encoding.UTF8, "application/fhir+json");

        var response = await client.PostAsync("/api/translate/fhir-to-openehr", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Doe",
            doc.RootElement.GetProperty("result").GetProperty("demographics").GetProperty("familyName").GetString());
    }

    [Fact]
    public async Task FhirToOpenEhr_WrongResourceType_Returns400()
    {
        var client = _factory.CreateClient();
        using var content = new StringContent(
            """{ "resourceType": "Observation", "status": "final" }""",
            Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/translate/fhir-to-openehr", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OpenEhrToFhir_ValidComposition_Returns200WithBundle()
    {
        var client = _factory.CreateClient();
        var composition = new
        {
            archetypeNodeId = "openEHR-EHR-COMPOSITION.demographics.v1",
            ehrStatus = new { subjectId = "int-002" },
            demographics = new
            {
                familyName = "Doe",
                givenName = "Jane",
                gender = "female",
                birthDate = "1990-01-01"
            }
        };

        var response = await client.PostAsJsonAsync("/api/translate/openehr-to-fhir", composition);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Bundle",
            doc.RootElement.GetProperty("result").GetProperty("resourceType").GetString());
    }
}
