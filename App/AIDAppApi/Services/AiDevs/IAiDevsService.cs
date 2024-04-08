using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Services
{
    public interface IAiDevsService
    {
        Task<string> GetTokenForTaskAsync(string taskId, CancellationToken ct = default);
        Task<UniversalResponse> GetTaskAsync(string taskId, CancellationToken ct = default);
        Task<AnswerResponse> SendAnswerAsync<T>(string tokenId, T answer, CancellationToken ct = default);
        Task<AnswerResponse> SolveHelloApi(CancellationToken ct = default);
        Task<string> GetContentForLiarTaskAsync(string tokenId, CancellationToken ct = default);
        Task<InpromptFilteredData> GetContentForInpromptTaskAsync(string tokenId, CancellationToken ct = default);
        Task<ModerationResponse> GetModerationTaskAsync(string tokenId, CancellationToken ct = default);
        Task<UniversalResponse> GetUniversalTaskAsync(string tokenId, CancellationToken ct = default);
        Task<ScraperData> GetScraperTaskAsync(string tokenId, CancellationToken ct = default);
        Task<List<UnknowNewsDataItem>> GetUnknowNewsDataAsync(CancellationToken ct = default);       
        Task<string> GetSearchQuestionAsync(string tokenId, CancellationToken ct = default);
        Task<List<PeopleDataItem>> GetPeopleDataAsync(CancellationToken ct = default);
        Task<string> GetPeopleQuestionAsync(string tokenId, CancellationToken ct = default);
    }
}