using AIFileAnalizator.Api.Dto.Request;
using AIFileAnalizator.Api.Services.Interfaces;
using AIFileAnalizator.Api.Services;
using AIFileAnalizator.Api.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IOllamaService, OllamaService>();
builder.Services.AddSingleton<IRagService, RagService>();
var app = builder.Build();


app.UseCors();


app.MapPost("/ask", async (AskRequest request, IOllamaService ollamaService) =>
{
    var result = await ollamaService.AskAsync(request.Prompt);                                                                   
    if (string.IsNullOrEmpty(result))     
        return Results.Problem("Ollama не вернул ответ.");     
    return Results.Ok(new { response = result });
});


app.MapPost("/upload", async (HttpRequest request, IOllamaService ollamaService) =>
{
    var file = request.Form.Files.FirstOrDefault();
    if (file == null || file.Length == 0)
        return Results.BadRequest("Файл не загружен.");

    var prompt = request.Form["prompt"].ToString();
    if (string.IsNullOrWhiteSpace(prompt))
        return Results.BadRequest("Промпт пустой.");

    using var reader = new StreamReader(file.OpenReadStream());
    var fileContent = await reader.ReadToEndAsync();

    var fullPrompt = $"""
        {prompt}

        Вот содержимое файла:
        ---
        {fileContent}         
        """;

    var response = await ollamaService.AskAsync(fullPrompt);
    return Results.Ok(new { response });
})
.Accepts<IFormFile>("multipart/form-data");


app.MapPost("/rag/ask", async (AskRequest request, IRagService service) =>
{
    var response = await service.AskWithContextAsync(request.Prompt);
    return Results.Ok(new { response });
});

app.MapPost("/rag/chunk", async (ChunkRequest request, IRagService service) =>
{
    await service.UploadChunkAsync(request.Text, request.Id);
    return Results.Ok(new { status = "Chunk uploaded" });
});

app.MapPost("/rag/upload", async (HttpRequest request, IRagService ragService) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file is null)
        return Results.BadRequest("Файл не был предоставлен.");

    try
    {
        await ragService.UploadFileAsync(file);
        return Results.Ok(new { message = "Файл успешно загружен в контекст." });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});






app.Run();
