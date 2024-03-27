
using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;


namespace AIDAppApi.Services
{
    public class AiDevsService : IAiDevsService
    {
        private readonly HttpClient _httpClient;
        private readonly AiDevsConfig _aiDevsConfig;

        public AiDevsService(IOptionsMonitor<AiDevsConfig> optionsMonitor)
        {
            _aiDevsConfig = optionsMonitor.CurrentValue;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetTaskAsync(string taskId, CancellationToken cancellationToken = default)
        {
            var tokenId = await GetTokenForTaskAsync(taskId, cancellationToken);

            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content;
            }

            throw new Exception();
        }

        public async Task<string> SolveHelloApi(CancellationToken cancellationToken = default)
        {
            var taskId = "helloapi";
            var tokenId = await GetTokenForTaskAsync(taskId, cancellationToken);
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var taskResponse = await _httpClient.PostAsync(uri, null,cancellationToken);
            if (taskResponse.IsSuccessStatusCode)
            {
                var content = await taskResponse.Content.ReadFromJsonAsync<HelloApiResponse>(cancellationToken);
                var result = await SendAnswerAsync<AnswerRequest<string>>(tokenId, new AnswerRequest<string>(content!.cookie));

                return result;
            }

            throw new Exception();
        }

        public async Task<ModerationResponse> GetModerationTaskAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var taskResponse = await _httpClient.PostAsync(uri, null, cancellationToken);
            if (taskResponse.IsSuccessStatusCode)
            {
                var result = await taskResponse.Content.ReadFromJsonAsync<ModerationResponse>(cancellationToken);
                return result!;
            }
            throw new Exception();
        }

        public async Task<string> GetTokenForTaskAsync(string taskId, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/token/{taskId}");
            TokenRequest request = new(_aiDevsConfig.Key);

            var response = await _httpClient.PostAsJsonAsync(uri, request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

                return content!.token.ToString();
            }

            throw new Exception();
        }

        public async Task<string> GetContentForLiarTaskAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("question", "Give me name of any city")
            });

            var response = await _httpClient.PostAsync(uri, formContent, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LiarResponse>(cancellationToken);

                return result!.answer!; 
            }

            throw new Exception();
        }

        public async Task<InpromptFilteredData> GetContentForInpromptTaskAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<InpromptResponse>(cancellationToken);
                var name = GetName(result!.question);
                var filteredInput = result.input.Where((x) => x.Contains(name)).ToArray();

                var filteredData = new InpromptFilteredData(result.question, filteredInput);

                return filteredData!; 
            }

            throw new Exception();
        }

        public async Task<string> SendAnswerAsync<T>(string tokenId, T answer, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/answer/{tokenId}");
            AnswerRequest<T> request = new (answer);

            var response = await _httpClient.PostAsJsonAsync(uri, answer, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content;
            }

            throw new Exception();
        }


        private static string? GetName(string sentence)
        {
            string pattern = @"\b[A-Z][a-z]*\b";
            Match match = Regex.Match(sentence, pattern);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
        }

        public record TokenRequest(string apikey);
        public record TokenResponse(int code, string msg, string token);
        public record LiarResponse(int code, string msg, string answer);
        public record InpromptResponse(int code, string msg, string[] input, string question);
        public record InpromptFilteredData(string? question, string[]? input);

        public record HelloApiResponse(int code, string msg, string cookie);
        public record ModerationResponse(int code, string msg, string[] input);
       

        public record AnswerRequest<T>(T answer);
    }
}