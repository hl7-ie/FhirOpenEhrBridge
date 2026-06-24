using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FhirOpenEhrBridge.Application.DependencyInjection;
using FhirOpenEhrBridge.Application.Translation;
using FhirOpenEhrBridge.Domain.Models.OpenEhr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// ---------------------------------------------------------------------------
// FHIR-OpenEHR-Bridge — capability demo.
//
// Composes the Application layer exactly as a host would, then runs the sample
// payloads from ./samples through the translation engine in both directions.
//
//   dotnet run --project samples/FhirOpenEhrBridge.Demo
// ---------------------------------------------------------------------------

var services = new ServiceCollection();
services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
services.AddBridgeApplication();
using var provider = services.BuildServiceProvider();
var translator = provider.GetRequiredService<ITranslationService>();

var writeOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
var readOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

var baseDir = AppContext.BaseDirectory;
var fhirDir = Path.Combine(baseDir, "samples", "fhir");
var openEhrDir = Path.Combine(baseDir, "samples", "openehr");

Banner("FHIR-OpenEHR-Bridge :: capability demo");

// 1) FHIR -> openEHR --------------------------------------------------------
Section("1. FHIR  ->  openEHR   (Patient -> demographics composition)");
foreach (var file in EnumerateJson(fhirDir))
{
    Item(Path.GetFileName(file));
    var result = translator.FhirToOpenEhr(File.ReadAllText(file));

    if (result.Succeeded)
    {
        var d = result.Value!.Demographics;
        Console.WriteLine($"   OK  -> {d.GivenName} {d.FamilyName}, gender={d.Gender}, dob={d.BirthDate ?? "n/a"}, " +
                          $"identifiers={d.Identifiers.Count}, subject={result.Value.EhrStatus.SubjectId}");
        PrintWarnings(result.Issues);
        Console.WriteLine(Indent(JsonSerializer.Serialize(result.Value, writeOptions)));
    }
    else
    {
        Console.WriteLine("   REJECTED:");
        PrintIssues(result.Issues);
    }
}

// 2) openEHR -> FHIR --------------------------------------------------------
Section("2. openEHR  ->  FHIR   (demographics composition -> Bundle)");
foreach (var file in EnumerateJson(openEhrDir))
{
    Item(Path.GetFileName(file));
    var composition = JsonSerializer.Deserialize<OpenEhrComposition>(File.ReadAllText(file), readOptions)!;
    var result = translator.OpenEhrToFhir(composition);

    if (result.Succeeded)
    {
        Console.WriteLine("   OK  -> FHIR Bundle:");
        PrintWarnings(result.Issues);
        Console.WriteLine(Indent(Prettify(result.Value!)));
    }
    else
    {
        Console.WriteLine("   REJECTED:");
        PrintIssues(result.Issues);
    }
}

// 3) Round trip -------------------------------------------------------------
Section("3. Round trip   (FHIR -> openEHR -> FHIR keeps the core data)");
var original = File.ReadAllText(Path.Combine(fhirDir, "patient-full.json"));
var toOpenEhr = translator.FhirToOpenEhr(original);
var backToFhir = translator.OpenEhrToFhir(toOpenEhr.Value!);
var bundle = JsonNode.Parse(backToFhir.Value!)!;
var patient = bundle["entry"]![0]!["resource"]!;
Console.WriteLine($"   original Patient.id      : patient-full-001");
Console.WriteLine($"   round-tripped Patient.id : {patient["id"]}");
Console.WriteLine($"   family / given           : {patient["name"]![0]!["family"]} / {patient["name"]![0]!["given"]![0]}");
Console.WriteLine($"   gender                   : {patient["gender"]}");
Console.WriteLine($"   first identifier         : {patient["identifier"]![0]!["value"]}");

Console.WriteLine();
Banner("Demo complete.");
return 0;

// ---------------------------------------------------------------------------
// Local helpers
// ---------------------------------------------------------------------------
static IEnumerable<string> EnumerateJson(string dir) =>
    Directory.Exists(dir)
        ? Directory.GetFiles(dir, "*.json").OrderBy(f => f, StringComparer.Ordinal)
        : Enumerable.Empty<string>();

static string Prettify(string json) => JsonNode.Parse(json)!.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

static string Indent(string text) =>
    string.Join(Environment.NewLine, text.Split('\n').Select(l => "      " + l.TrimEnd('\r')));

static void PrintWarnings(IReadOnlyList<FhirOpenEhrBridge.Domain.Validation.ValidationIssue> issues)
{
    foreach (var i in issues)
    {
        Console.WriteLine($"   ! {i.Severity}: {i.Message}");
    }
}

static void PrintIssues(IReadOnlyList<FhirOpenEhrBridge.Domain.Validation.ValidationIssue> issues)
{
    foreach (var i in issues)
    {
        Console.WriteLine($"      - [{i.Severity}] {i.Message}{(i.Location is null ? "" : $" ({i.Location})")}");
    }
}

static void Banner(string text)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 70));
    Console.WriteLine("  " + text);
    Console.WriteLine(new string('=', 70));
}

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', 70));
}

static void Item(string name)
{
    Console.WriteLine();
    Console.WriteLine($" • {name}");
}
