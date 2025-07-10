using AIFileAnalizator.Api.Dto;
using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Dto.Response;
using AIFileAnalizator.Api.Options;
using AIFileAnalizator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIFileAnalizator.Api.Services;

public class OllamaService : IOllamaService
{
    private readonly Kernel _kernel;

    public OllamaService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string?> GenerateAsync(string prompt)
    {
        // Use Kernel to generate text from a prompt
        var result = await _kernel.InvokePromptAsync(prompt);
        return result?.ToString();
    }

    public async Task<MessageContent?> ChatAsync(IEnumerable<ChatMessage> messages)
    {
        // Use Kernel's chat completion service
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        foreach (var msg in messages)
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else if (msg.Role == "assistant" || msg.Role == "ai")
                chatHistory.AddAssistantMessage(msg.Content);
            else
                chatHistory.AddMessage(new AuthorRole(msg.Role), msg.Content);
        }
        var result = await chatService.GetChatMessageContentAsync(chatHistory, kernel: _kernel);
        if (result == null) return null;
        return new MessageContent(result.Role.ToString().ToLower(), result.Content);
    }
}
