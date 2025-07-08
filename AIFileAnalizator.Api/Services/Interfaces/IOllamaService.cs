using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Dto.Response;

namespace AIFileAnalizator.Api.Services.Interfaces;
public interface IOllamaService
{
    Task<string?> GenerateAsync(string prompt);
    Task<MessageContent?> ChatAsync(IEnumerable<ChatMessage> messages);
}