using AIDAppApi.Configurations;
using AIDAppApi.Endpoints;
using AIDAppApi.Services;
using AIDAppApi.Services.Qdrant;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<OpenAiConfig>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<AiDevsConfig>(builder.Configuration.GetSection("AIDevs"));
builder.Services.Configure<QdrantConfig>(builder.Configuration.GetSection("Qdrant"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOpenAiService, OpenAiService>();
builder.Services.AddScoped<IAiDevsService, AiDevsService>();
builder.Services.AddScoped<IQdrantService, QdrantService>();

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
