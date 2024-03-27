using AIDAppApi.Configurations;
using AIDAppApi.Endpoints;
using AIDAppApi.Services;

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
app.RegisterUserEndpoints();

app.Run();
