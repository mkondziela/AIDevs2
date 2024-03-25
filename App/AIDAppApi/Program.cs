using AIDAppApi.Configurations;
using AIDAppApi.Services;
using static AIDAppApi.Services.AiDevsService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<OpenAiConfig>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<AiDevsConfig>(builder.Configuration.GetSection("AIDevs"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IAiDevsService, AiDevsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/gettask", async (string taskId, IAiDevsService aiDevsService) =>
{
    var result = "";
    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
    {
        result = await aiDevsService.GetTaskAsync(taskId, ct.Token);
    };

    return result;
});

app.MapPost("/helloapi", async (IAiDevsService aiDevsService) =>
{
    var result = "";
    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
    {
        result = await aiDevsService.SolveHelloApi(ct.Token);
    };

    return result;
});

app.MapPost("/moderation", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
{
    var result = "";
    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
    {
        var taskId = "moderation";
        var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
        var taskContent = await aiDevsService.GetModerationTaskAsync(tokenId, ct.Token);
        if (taskContent!.input!.Length>0)
        {
            var openAIResponse = await openAiService.ModerateSentencesAsync(taskContent.input!);
            result = await aiDevsService.SendAnswerAsync<AnswerRequest<List<int>>>(tokenId, new AnswerRequest<List<int>>(openAIResponse));

            return result;
        }

        throw new Exception();
    };
});

app.MapPost("/blogger", async (IAiDevsService aiDevsService, IOpenAiService openAiService) =>
{
    var result = "";
    using (var ct = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
    {
        var taskId = "blogger";
        var tokenId = await aiDevsService.GetTokenForTaskAsync(taskId, ct.Token);
        var openAIResponse = await openAiService.BloggerAsync();

        result = await aiDevsService.SendAnswerAsync<AnswerRequest<List<string>>>(tokenId, new AnswerRequest<List<string>>(openAIResponse.Answer));

        return result;
    };
});


app.Run();
