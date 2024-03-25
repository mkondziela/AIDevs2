namespace AIDAppApi.Services
{
    public interface IOpenAiService
    {
        Task<List<int>> ModerateSentencesAsync(string[] sentences);
        Task<BloggerResult> BloggerAsync();
    }
}