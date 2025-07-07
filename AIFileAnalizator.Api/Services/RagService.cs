using System.Text.Json;
using AIFileAnalizator.Api.Helpers;
using AIFileAnalizator.Api.Options;
using AIFileAnalizator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AIFileAnalizator.Api.Services;

public class RagService : IRagService
{               
    private readonly QdrantClient _qdrantClient;
    private readonly string _collectionName;
    private readonly string _embeddingName;
    private readonly string _modelName;
    private HttpClient _httpClient;

    public RagService(IOptions<QdrantOptions> options, HttpClient httpClient)
    {
        _qdrantClient = new QdrantClient(options.Value.Host, options.Value.Port);
        _collectionName = options.Value.CollectionName;
        _embeddingName = options.Value.EmbeddingModelName;
        _modelName = options.Value.Model;
        _httpClient = httpClient;
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

        var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", new
        {
            model = _modelName,
            prompt = fullPrompt,
            stream = false
        });

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("response").GetString();
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
        var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/embeddings", new
        {
            model = _embeddingName,
            prompt = text
        });

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("embedding")
            .EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();
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
