using Microsoft.AspNetCore.Mvc;
using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Services.Interfaces;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly IOllamaService _ollamaService;

    public ChatController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskChatRequest request)
    {
        var result = await _ollamaService.ChatAsync(request.Messages);
        if (result == null)
            return Problem("Ollama не вернул ответ.");
        return Ok(new { response = result });
    }
} 