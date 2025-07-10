using AIFileAnalizator.Api.Services.Interfaces;
using AIFileAnalizator.Api.Services;
using AIFileAnalizator.Api.Options;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));
builder.AddQdrantClient("qdrant");

builder.Services
    .AddKernel()
    .AddOllamaChatCompletion(
        modelId: builder.Configuration["Ollama:Model"] ?? "llama3",
        endpoint: new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434")
    )
    .AddOllamaTextEmbeddingGeneration(
        modelId: builder.Configuration["Qdrant:EmbeddingModelName"] ?? "nomic-embed-text",
        endpoint: new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434")
    );

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Remove AddHttpClient if not needed elsewhere
// builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOllamaService, OllamaService>();
builder.Services.AddSingleton<IRagService, RagService>();
builder.Services.AddControllers();  

var app = builder.Build();
app.UseCors();
app.MapControllers();
app.Run();
