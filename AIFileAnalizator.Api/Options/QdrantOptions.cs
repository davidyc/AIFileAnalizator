namespace AIFileAnalizator.Api.Options;

public class QdrantOptions
{        
    public string CollectionName { get; set; } = "documents";
    public string EmbeddingModelName { get; set; } = "nomic-embed-text";
    public string Model { get; set; }
}
