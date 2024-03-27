using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Services
{
    public interface IAiDevsService
    {
        Task<string> GetTokenForTaskAsync(string taskId, CancellationToken cancellationToken = default);
        Task<string> GetTaskAsync(string taskId, CancellationToken cancellationToken = default);
        Task<string> SolveHelloApi(CancellationToken cancellationToken = default);
        Task<string> GetContentForLiarTaskAsync(string tokenId, CancellationToken cancellationToken = default);
        Task<InpromptFilteredData> GetContentForInpromptTaskAsync(string tokenId, CancellationToken cancellationToken = default);


        Task<ModerationResponse> GetModerationTaskAsync(string token, CancellationToken cancellationToken = default);
        Task<string> SendAnswerAsync<T>(string tokenId, T answer, CancellationToken cancellationToken = default);
    }
}