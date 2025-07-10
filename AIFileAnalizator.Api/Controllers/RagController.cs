using Microsoft.AspNetCore.Mvc;
using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Services.Interfaces;

[ApiController]
[Route("rag/generate")]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;

    public RagController(IRagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskWithContext([FromBody] AskRequest request)
    {
        var response = await _ragService.AskWithContextAsync(request.Prompt);
        return Ok(new { response });
    }

    [HttpPost("chunk")]
    public async Task<IActionResult> UploadChunk([FromBody] ChunkRequest request)
    {
        await _ragService.UploadChunkAsync(request.Text, request.Id);
        return Ok(new { status = "Chunk uploaded" });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file is null)
            return BadRequest("Файл не был предоставлен.");
        try
        {
            await _ragService.UploadFileAsync(file);
            return Ok(new { message = "Файл успешно загружен в контекст." });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
} 