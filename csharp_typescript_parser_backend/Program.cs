using NSwag.Annotations;
using TypeScriptParserBackend.DTOs;
using TypeScriptParserBackend.Parsing;
using TypeScriptParserBackend.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.PostProcess = d =>
    {
        d.Info.Title = "C# TypeScript Parser API";
        d.Info.Description = "Minimal endpoints to parse TypeScript, list variables, and regenerate code.";
        d.Info.Version = "1.0.0";
        d.Tags = new[]
        {
            new NSwag.OpenApiTag { Name = "Parsing", Description = "TypeScript parsing operations" }
        }.ToList();
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Configure OpenAPI/Swagger
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.Path = "/docs";
});

// Health check endpoint
app.MapGet("/", () => new { message = "Healthy" });

// Instantiate parser
var parser = new TypeScriptParser();

/// <summary>
/// Parses the provided TypeScript source into structured information.
/// </summary>
app.MapPost("/api/parse",
    (ParseRequest request) =>
    {
        var result = parser.Parse(request.Source ?? string.Empty);
        return Results.Json(result, TypeScriptSerializer.GetOptions());
    });

/// <summary>
/// Aggregates class variables (fields) from the parsed result of the provided source.
/// </summary>
app.MapPost("/api/variables",
    (ParseRequest request) =>
    {
        var parsed = parser.Parse(request.Source ?? string.Empty);
        var vars = parser.Variables(parsed);
        return Results.Json(vars, TypeScriptSerializer.GetOptions());
    });

/// <summary>
/// Generates TypeScript code string from either the provided source (parsed first) or provided parseResult object.
/// If className is provided, targets that class when regenerating.
/// </summary>
app.MapPost("/api/tostring",
    (ToStringRequest request) =>
    {
        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var parsed = parser.Parse(request.Source);
            var ts = parser.ToString(parsed, request.ClassName);
            return Results.Text(ts);
        }

        if (request.ParseResult is not null)
        {
            var ts = parser.ToString(request.ParseResult, request.ClassName);
            return Results.Text(ts);
        }

        return Results.BadRequest(new { error = "Provide either 'source' or 'parseResult'." });
    });

app.Run();

/// <summary>
/// Request DTO for /api/tostring endpoint.
/// </summary>
public class ToStringRequest
{
    /// <summary>
    /// Optional TypeScript source to parse then regenerate.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Optional className to target when regenerating.
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Optional pre-parsed result to regenerate from.
    /// </summary>
    public ParseResponse? ParseResult { get; set; }
}