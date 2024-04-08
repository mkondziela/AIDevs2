using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
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

        public async Task<UniversalResponse> GetTaskAsync(string taskId, CancellationToken ct = default)
        {
            var tokenId = await GetTokenForTaskAsync(taskId, ct);

            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UniversalResponse>(ct);
                return result!;
            }

            throw new Exception($"error : {response.Content.ToString()}");
        }

        public async Task<AnswerResponse> SendAnswerAsync<T>(string tokenId, T answer, CancellationToken ct = default)       
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/answer/{tokenId}");
            AnswerRequest<T> request = new(answer);

            var response = await _httpClient.PostAsJsonAsync(uri, answer, ct);
            var content = await response.Content.ReadFromJsonAsync<AnswerResponse>(ct);

            return content;
        }

        public async Task<UniversalResponse> GetUniversalTaskAsync(string tokenId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var taskResponse = await _httpClient.PostAsync(uri, null, ct);
            if (taskResponse.IsSuccessStatusCode)
            {
                var result = await taskResponse.Content.ReadFromJsonAsync<UniversalResponse>(ct);
                return result!;
            }
            throw new Exception();
        }

        public async Task<AnswerResponse> SolveHelloApi(CancellationToken ct = default)
        {
            var taskId = "helloapi";
            var tokenId = await GetTokenForTaskAsync(taskId, ct);
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var taskResponse = await _httpClient.PostAsync(uri, null,ct);
            if (taskResponse.IsSuccessStatusCode)
            {
                var content = await taskResponse.Content.ReadFromJsonAsync<HelloApiResponse>(ct);
                var result = await SendAnswerAsync(tokenId, new AnswerRequest<string>(content!.cookie));

                return result;
            }

            throw new Exception();
        }

        public async Task<ModerationResponse> GetModerationTaskAsync(string tokenId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var taskResponse = await _httpClient.PostAsync(uri, null, ct);
            if (taskResponse.IsSuccessStatusCode)
            {
                var result = await taskResponse.Content.ReadFromJsonAsync<ModerationResponse>(ct);
                return result!;
            }
            throw new Exception();
        }

        public async Task<string> GetTokenForTaskAsync(string taskId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/token/{taskId}");
            TokenRequest request = new(_aiDevsConfig.Key);

            var response = await _httpClient.PostAsJsonAsync(uri, request, ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);

                return content!.token.ToString();
            }

            throw new Exception();
        }

        public async Task<string> GetContentForLiarTaskAsync(string tokenId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("question", "Give me name of any city")
            });

            var response = await _httpClient.PostAsync(uri, formContent, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LiarResponse>(ct);

                return result!.answer!; 
            }

            throw new Exception();
        }

        public async Task<InpromptFilteredData> GetContentForInpromptTaskAsync(string tokenId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<InpromptResponse>(ct);
                var name = GetName(result!.question);
                var filteredInput = result.input.Where((x) => x.Contains(name)).ToArray();

                var filteredData = new InpromptFilteredData(result.question, filteredInput);

                return filteredData!; 
            }

            throw new Exception();
        }

        public async Task<ScraperData> GetScraperTaskAsync(string tokenId, CancellationToken ct = default)
        {
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ScraperResponse>(ct);
                var externalData =  await GetExternalDataAsync(result.input, ct);
                var scraperData = new ScraperData(result.msg, externalData, result.question);

                return scraperData;
            }

            throw new Exception();
        }

        public async Task<List<UnknowNewsDataItem>> GetUnknowNewsDataAsync(CancellationToken ct = default)
        {
            var result = new List<UnknowNewsDataItem>();
             try
                {
                    var client = new HttpClient();
                    result = await client.GetFromJsonAsync<List<UnknowNewsDataItem>>("https://unknow.news/archiwum_aidevs.json", ct);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"error: {ex.Message}");
                }

            return result;
        }

        public async Task<string> GetSearchQuestionAsync(string tokenId, CancellationToken ct = default)
        {
            var result = "";
            Uri uri = new Uri($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");

            var response = await _httpClient.PostAsync(uri, null, ct);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<ScraperResponse>(ct);
                result = content!.question!;
            }

            return result;
        }

        public async Task<List<PeopleDataItem>> GetPeopleDataAsync(CancellationToken ct = default)
        {
            var result = new List<PeopleDataItem>();
             try
                {
                    var client = new HttpClient();
                    result = await client.GetFromJsonAsync<List<PeopleDataItem>>("https://tasks.aidevs.pl/data/people.json", ct);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"error: {ex.Message}");
                }

            return result;
        }

        public async Task<string> GetPeopleQuestionAsync(string tokenId, CancellationToken ct = default)
        {
            var response = await _httpClient.GetFromJsonAsync<PeopleResponse>($"{_aiDevsConfig.BaseAddress}/task/{tokenId}");
            if (response != null)
            {
                return response.question;
            }

            return "";
        }

        private async Task<string> GetExternalDataAsync(string dataUri, CancellationToken ct = default)
        {
            var result = "";
            var uri = dataUri.Replace(@"\","");
            do 
            {
                try
                {
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
                    using var ict = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    using Stream stream = await client.GetStreamAsync(uri, ict.Token);
                    using StreamReader reader = new StreamReader(stream);
                    result = await reader.ReadToEndAsync();
                    Console.WriteLine("GOT IT!");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"error: {ex.Message}");
                }
            }
            while (string.IsNullOrEmpty(result));

            return result;
        }

        private static string GetName(string sentence)
        {
            Match match = Regex.Match(sentence, @"\b[A-Z][a-z]*\b");
            
            return match.Success ? match.Value : "";            
        }

        public record TokenRequest(string apikey);
        public record TokenResponse(int code, string msg, string token);
        public record LiarResponse(int code, string msg, string answer);
        public record InpromptResponse(int code, string msg, string[] input, string question);
        public record InpromptFilteredData(string? question, string[]? input);

        public record HelloApiResponse(int code, string msg, string cookie);
        public record ModerationResponse(int code, string msg, string[] input);
       
        public record UniversalResponse(int code, string msg, string? hint, string? hint1, string? hint2, string? hint3);
        public record AnswerResponse(int code, string msg, string note, string? reply, string? Additional_papers);

        public record ScraperResponse(int code, string msg, string input, string question);
        public record ScraperData(string msg, string? data, string question);

        public record SearchResponse(int code, string msg, string question);

        public record AnswerRequest<T>(T answer);
        public record UnknowNewsDataItem(Guid id, string title, string url, string info, DateTime date){
            public UnknowNewsDataItem() : this(Guid.NewGuid(), "", "", "", DateTime.Now) { }
        };

        public record PeopleResponse(int code, string msg, string data, string question, string hint1, string hint2);

        public record PeopleDataItem(Guid id, string imie, string nazwisko, int wiek, string o_mnie, string ulubiona_postac_z_kapitana_bomby, string ulubiony_serial, string ulubiony_film, string ulubiony_kolor){
            public PeopleDataItem() : this(Guid.NewGuid(), "", "", -1, "", "", "", "", "") { }
        };
    }
}