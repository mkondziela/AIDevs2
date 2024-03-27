using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Services
{
    public interface IOpenAiService
    {
        Task<List<int>> ModerateSentencesAsync(string[] sentences);
        Task<BloggerResult> BloggerAsync();
        Task<string> LiarAsync(string sentence);
        Task<string> InpromptAsync(InpromptFilteredData input);
        Task<List<float>> EmbeddingAsync(string sentence);
    }
}