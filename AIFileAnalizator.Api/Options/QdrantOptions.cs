namespace AIFileAnalizator.Api.Options;

public class QdrantOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "documents";
    public string EmbeddingModelName { get; set; } = "nomic-embed-text";
    public string Model { get; set; }
}
