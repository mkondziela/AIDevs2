using AIDAppApi.Services;
using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Endpoints
{
    public static class UsersEndpoints
    {
        public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var endpoints = routes.MapGroup("/api/v1");

            endpoints.MapPost("/gettask", async (string taskId, IAiDevsService aiDevsService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    result = await aiDevsService.GetTaskAsync(taskId, ct.Token);
                };

                return result;
            });

            endpoints.MapPost("/helloapi", async (IAiDevsService aiDevsService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    result = await aiDevsService.SolveHelloApi(ct.Token);
                };

                return result;
            });

            endpoints.MapPost("/moderation", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var taskId = "moderation";
                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
                    var taskContent = await aiDevsService.GetModerationTaskAsync(tokenId, ct.Token);
                    if (taskContent!.input!.Length > 0)
                    {
                        var openAIResponse = await openAiService.ModerateSentencesAsync(taskContent.input!);
                        result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<int>>(openAIResponse));

                        return result;
                    }

                    throw new Exception();
                };
            });

            endpoints.MapPost("/blogger", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var taskId = "blogger";
                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
                    var openAIResponse = await openAiService.BloggerAsync();

                    result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<string>>(openAIResponse.Answer));

                    return result;
                };
            });

            endpoints.MapPost("/liar", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var taskId = "liar";
                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
                    var liarSaid = await aiDevsService.GetContentForLiarTaskAsync(tokenId, ct.Token);
                    var openAIResponse = await openAiService.LiarAsync(liarSaid);

                    result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse));

                    return result;
                };
            });

            endpoints.MapPost("/inprompt", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var taskId = "inprompt";
                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
                    var inpromtInput = await aiDevsService.GetContentForInpromptTaskAsync(tokenId, ct.Token);
                    
                    var openAIResponse = await openAiService.InpromptAsync(inpromtInput);

                    result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse));

                    return result;
                };
            });

            endpoints.MapPost("/embedding", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
            {
                var result = "";
                using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
                {
                    var taskId = "embedding";
                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
                    
                    var openAIResponse = await openAiService.EmbeddingAsync("Hawaiian pizza");

                    result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<float>>(openAIResponse));

                    return result;
                };
            });            
        }
    }
}
