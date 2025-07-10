using System.Text.Json;
using AIFileAnalizator.Api.Helpers;
using AIFileAnalizator.Api.Options;
using AIFileAnalizator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace AIFileAnalizator.Api.Services;

public class RagService : IRagService
{               
    private readonly QdrantClient _qdrantClient;
    private readonly string _collectionName;
    private readonly string _embeddingName;
    private readonly string _modelName;
    private readonly Kernel _kernel;

    public RagService(IOptions<QdrantOptions> options, Kernel kernel, QdrantClient client)
    {          
        _qdrantClient = client;
        _collectionName = options.Value.CollectionName;
        _embeddingName = options.Value.EmbeddingModelName;
        _modelName = options.Value.Model;
        _kernel = kernel;
    }

    public async Task UploadChunkAsync(string text, string? optionalId = null)
    {
        await EnsureCollectionExistsAsync();

        var embedding = await GetEmbeddingAsync(text);
        var id = VectorIdGenerator.NewGuidId();     
        var payload = new Dictionary<string, Value>
        {
            ["text"] = text
        };
        var points = new PointStruct
        {
            Id = new PointId { Uuid = id },
            Vectors = new Vectors
            {
                Vector = embedding
            },
            Payload = { payload }
        };
       await _qdrantClient.UpsertAsync(collectionName: _collectionName, points: [points]);
      
    }

    public async Task<string> AskWithContextAsync(string userQuestion)
    {
        await EnsureCollectionExistsAsync();

        var queryEmbedding = await GetEmbeddingAsync(userQuestion);        

        var result = await _qdrantClient.SearchAsync(_collectionName, queryEmbedding.ToArray());

        var context = result
            .Select(r => r.Payload["text"].StringValue)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        var fullPrompt = $"""
        Используй контекст ниже, чтобы ответить на вопрос.

        Контекст:
        {string.Join("\n---\n", context)}

        Вопрос:
        {userQuestion}

        Ответ:
        """;

        var response = await _kernel.InvokePromptAsync(fullPrompt);
        return response?.ToString() ?? string.Empty;
    }

    public async Task UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл не передан или пуст.");

        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        var id = file.FileName;
        await UploadChunkAsync(content, id);
    }

    private async Task<float[]> GetEmbeddingAsync(string text)
    {
        var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var embedding = await embeddingService.GenerateEmbeddingAsync(text, _kernel);
        return embedding.ToArray();
    }

    private async Task EnsureCollectionExistsAsync()
    {
        var exists = await _qdrantClient.CollectionExistsAsync(_collectionName); 

        if (!exists)
        {
            await _qdrantClient.CreateCollectionAsync(_collectionName, new VectorParams
            {
                Size = 768,
                Distance = Distance.Cosine
            });
        }
    }
}
