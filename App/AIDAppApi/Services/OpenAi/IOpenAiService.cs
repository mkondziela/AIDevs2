using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Services
{
    public interface IOpenAiService
    {
        Task<List<int>> ModerateSentencesAsync(string[] sentences, CancellationToken ct = default);
        Task<BloggerResult> BloggerAsync(CancellationToken ct = default);
        Task<string> LiarAsync(string sentence, CancellationToken ct = default);
        Task<string> InpromptAsync(InpromptFilteredData input, CancellationToken ct = default);
        Task<List<float>> EmbeddingAsync(string sentence, CancellationToken ct = default);
        Task<string> WisperAsync(string fileUrl, CancellationToken ct = default);
        Task<FunctionDefinition> FunctionsAsync(string userPrompt, CancellationToken ct = default);
        Task<string> RodoAsync(string userPrompt, CancellationToken ct = default);
        Task<string> ScraperAsync(ScraperData request, CancellationToken ct = default);
        Task<string> WhoAmI(List<string> hints, CancellationToken token);
    }
}