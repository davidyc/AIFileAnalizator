using Microsoft.AspNetCore.Mvc;
using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Services.Interfaces;

[ApiController]
[Route("generate")]
public class GenerateController : ControllerBase
{
    private readonly IOllamaService _ollamaService;

    public GenerateController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var result = await _ollamaService.GenerateAsync(request.Prompt);
        if (string.IsNullOrEmpty(result))
            return Problem("Ollama не вернул ответ.");
        return Ok(new { response = result });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string prompt)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не загружен.");
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest("Промпт пустой.");

        using var reader = new StreamReader(file.OpenReadStream());
        var fileContent = await reader.ReadToEndAsync();

        var fullPrompt = $"""
            {prompt}

            Вот содержимое файла:
            ---
            {fileContent}         
            """;

        var response = await _ollamaService.GenerateAsync(fullPrompt);
        return Ok(new { response });
    }
} 