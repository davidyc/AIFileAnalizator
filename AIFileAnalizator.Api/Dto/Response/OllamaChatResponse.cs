namespace AIFileAnalizator.Api.Dto.Response;

public record OllamaChatResponse(MessageContent Message, bool Done);

public record MessageContent(string Role, string Content);
