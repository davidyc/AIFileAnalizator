namespace AIFileAnalizator.Api.Services.Interfaces;


public interface IRagService
{
    Task UploadChunkAsync(string text, string? optionalId = null);
    Task<string> AskWithContextAsync(string userQuestion);
    Task UploadFileAsync(IFormFile file);
}