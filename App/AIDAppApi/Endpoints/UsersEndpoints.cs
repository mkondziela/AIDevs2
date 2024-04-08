using AIDAppApi.Configurations;
using AIDAppApi.Services;
using AIDAppApi.Services.Qdrant;
using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Endpoints
{
    public static class UsersEndpoints
    {
        private static TimeSpan _ctTimeOut = TimeSpan.FromSeconds(6000);

        public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var endpoints = routes.MapGroup("/api/v1");

            endpoints.MapPost("/gettask", GetTask());
            endpoints.MapPost("/helloapi", HelloApi());
            endpoints.MapPost("/moderation", Moderation());
            endpoints.MapPost("/blogger", Blogger());
            endpoints.MapPost("/liar", Liar());
            endpoints.MapPost("/inprompt", Inprompt());
            endpoints.MapPost("/embedding", Embedding());
            endpoints.MapPost("/whisper", Whisper());
            endpoints.MapPost("/functions", Functions());
            endpoints.MapPost("/rodo", Rodo());
            endpoints.MapPost("/scraper", Scraper());
            endpoints.MapPost("/whoami", WhoAmI());
            endpoints.MapPost("/search", CreateSearchEndpoint());
            endpoints.MapPost("/people",CreatePeopleEndpoint());
        }

        private static Func<string, IAiDevsService, CancellationToken, Task<IResult>> GetTask()       
        {
            return async (string taskId, IAiDevsService aiDevsService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var result = await aiDevsService.GetTaskAsync(taskId, cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, CancellationToken, Task<IResult>> HelloApi()
        {
            return async (IAiDevsService aiDevsService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var result = await aiDevsService.SolveHelloApi(cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Moderation()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "moderation";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var taskContent = await aiDevsService.GetModerationTaskAsync(tokenId, cts.Token);
                if (taskContent!.input!.Length > 0)
                {
                    var openAIResponse = await openAiService.ModerateSentencesAsync(taskContent.input!, cts.Token);
                    var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<int>>(openAIResponse));

                    return Results.Ok(result);
                }

                throw new Exception();
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Blogger()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "blogger";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var openAIResponse = await openAiService.BloggerAsync(cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<string>>(openAIResponse.Answer), cts.Token);

                return Results.Ok(result);
            };
        }
        
        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Liar()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "liar";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var liarSaid = await aiDevsService.GetContentForLiarTaskAsync(tokenId, cts.Token);
                var openAIResponse = await openAiService.LiarAsync(liarSaid, cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse), cts.Token);

                return Results.Ok(result);
            };
        }
       
        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Inprompt()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "inprompt";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var inpromtInput = await aiDevsService.GetContentForInpromptTaskAsync(tokenId, cts.Token);
                var openAIResponse = await openAiService.InpromptAsync(inpromtInput, cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse));

                return Results.Ok(result);
            };
        }
        
        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Embedding()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "embedding";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var openAIResponse = await openAiService.EmbeddingAsync("Hawaiian pizza", cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<List<float>>(openAIResponse), cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Whisper()       
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {

                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var tokenId = await aiDevsService.GetTokenForTaskAsync("whisper", cts.Token);
                var openAIResponse = await openAiService.WisperAsync("https://tasks.aidevs.pl/data/mateusz.mp3", cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse), cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Functions()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {

                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var tokenId = await aiDevsService.GetTokenForTaskAsync("functions", cts.Token);
                var taskResponse = await aiDevsService.GetUniversalTaskAsync(tokenId, cts.Token);

                var openAIResponse = await openAiService.FunctionsAsync(taskResponse.msg, cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<FunctionDefinition>(openAIResponse), cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Rodo()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var tokenId = await aiDevsService.GetTokenForTaskAsync("rodo", cts.Token);
                var taskResponse = await aiDevsService.GetUniversalTaskAsync(tokenId, cts.Token);

                var openAIResponse = await openAiService.RodoAsync(taskResponse.msg, cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse), cts.Token);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> Scraper()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "scraper";
                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var inpromtInput = await aiDevsService.GetScraperTaskAsync(tokenId, cts.Token);
                var openAIResponse = await openAiService.ScraperAsync(inpromtInput, cts.Token);

                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse));

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, CancellationToken, Task<IResult>> WhoAmI()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, CancellationToken ct) =>
            {
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var taskId = "whoami";               
                var hints  = new List<string>();
                var hasCorrectAnswer = false;
                AnswerResponse result;
                do
                {
                    var theName = "0";
                    do
                    {
                        var taskResponse = await aiDevsService.GetTaskAsync(taskId, cts.Token);
                        var hint = taskResponse.hint;
                        if (hint != null && !hints.Any(x => x==hint)) 
                            hints.Add(hint);
                        if (hints.Count >= 1)
                            theName = await openAiService.WhoAmI(hints, cts.Token);
                        Thread.Sleep(2000);
                    }
                    while (theName == "0");

                    var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                    result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(theName));
                    Console.WriteLine($"{theName} : {result.msg}");
                    hasCorrectAnswer = result.code >= 0;
                    Thread.Sleep(2000);
                }
                while (!hasCorrectAnswer);

                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, IQdrantService,  CancellationToken, Task<IResult>> CreateSearchEndpoint()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, IQdrantService qdrantService, CancellationToken ct) =>
            {
                var taskId = "search";
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var collectionName = $"{taskId}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";
                var collectionResult = await qdrantService.CreateCollection(collectionName, 1536);
                var unknowNewsData  = await aiDevsService.GetUnknowNewsDataAsync(cts.Token);
                var qdrantInput = new Dictionary<Guid, List<float>>();

                foreach (var item in unknowNewsData)
                {                  
                    var openAIResponse = await openAiService.EmbeddingAsync(item.title, cts.Token);
                    qdrantInput.Add(item.id, openAIResponse);                    
                }

                await qdrantService.AddDataToCollectionAsync(collectionName, qdrantInput, cts.Token);

                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var taskResponse = await aiDevsService.GetSearchQuestionAsync(tokenId, cts.Token);
                var questionVectors = await openAiService.EmbeddingAsync(taskResponse, cts.Token);

                var qdrantPayLoad = await qdrantService.SearchAsync(collectionName, questionVectors, cts.Token);
                var url  = unknowNewsData.Where(x => qdrantPayLoad.Equals(x.id)).Select(x => x.url).FirstOrDefault();
              
                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(url!), cts.Token);


                return Results.Ok(result);
            };
        }

        private static Func<IAiDevsService, IOpenAiService, IQdrantService,  CancellationToken, Task<IResult>> CreatePeopleEndpoint()
        {
            return async (IAiDevsService aiDevsService, IOpenAiService openAiService, IQdrantService qdrantService, CancellationToken ct) =>
            {
                var taskId = "people";
                using var cts = new CancellationTokenSource(_ctTimeOut);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

                var collectionName = $"{taskId}_{DateTime.Now.ToString("yyyyMMddhhmmss")}";
                var collectionResult = await qdrantService.CreateCollection(collectionName, 1536);
                var peopleDb  = await aiDevsService.GetPeopleDataAsync(cts.Token);
                var qdrantInput = new Dictionary<Guid, List<float>>();

                foreach (var item in peopleDb)
                {                  
                    var embedings = await openAiService.EmbeddingAsync($"{item.imie} {item.nazwisko}", cts.Token);
                    qdrantInput.Add(item.id, embedings);                    
                }

                await qdrantService.AddDataToCollectionAsync(collectionName, qdrantInput, cts.Token);

                var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, cts.Token);
                var taskQuestion = await aiDevsService.GetPeopleQuestionAsync(tokenId, cts.Token);
                var questionVectors = await openAiService.EmbeddingAsync(taskQuestion, cts.Token);

                var qdrantPayLoad = await qdrantService.SearchAsync(collectionName, questionVectors, cts.Token);
                var person  = peopleDb.Where(x => qdrantPayLoad.Equals(x.id)).Single();

                var inpromtInput = new InpromptFilteredData(taskQuestion, new string[] 
                    {$"nazywam się: {person.imie} {person.nazwisko}",
                    $"wiek: {person.wiek}",
                    $"o mnie: {person.o_mnie}",
                    $"ulubiona postać z kapitana bomby: {person.ulubiona_postac_z_kapitana_bomby}",
                    $"ulubiony serial: {person.ulubiony_serial}",
                    $"ulubiony film: {person.ulubiony_film}",
                    $"ulubiony kolor: {person.ulubiony_kolor}"
                    });
                var openAIResponse = await openAiService.InpromptAsync(inpromtInput, cts.Token);
              
                var result = await aiDevsService.SendAnswerAsync(tokenId, new AnswerRequest<string>(openAIResponse), cts.Token);


                return Results.Ok(result);
            };
        }        
    }  
}
