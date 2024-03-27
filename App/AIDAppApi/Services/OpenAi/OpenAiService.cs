using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Embedding;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using System;
using System.Text.Json;
using static AIDAppApi.Services.AiDevsService;
using static OpenAI_API.Chat.ChatMessage;

namespace AIDAppApi.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiConfig _openAiConfig;
        public OpenAiService(IOptionsMonitor<OpenAiConfig> optionsMonitor)
        {
            _openAiConfig = optionsMonitor.CurrentValue;
        }

        public async Task<List<int>> ModerateSentencesAsync(string[] sentences)        
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var moderationRequest = new ModerationRequest()
            {
                Model= "text-moderation-latest",
                Inputs = sentences
            };

            var response = await api.Moderation.CallModerationAsync(moderationRequest);
            var result = new List<int>();
            foreach (var item in response.Results)
            {
                result.Add(item.Flagged ? 1 : 0);
            }

            return result;
        }
        public async Task<BloggerResult> BloggerAsync()
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,
                //MaxTokens = 50,
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "I'm a blogger write short posts about pizza. All my post are json object ###example answer: [sentence1; sentence2; sentence3; sentence4]"),
                    new ChatMessage(ChatMessageRole.User, "Potrzebuję napisać cztery którkie akapity na temat pizzy: Wstęp: kilka słów na temat historii pizzy,Niezbędne składniki na pizzę," +
                        "Robienie pizzy,Pieczenie pizzy w piekarniku.")
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.FirstOrDefault().Message.Content.ToString().Replace("[","").Replace("]","").Replace("\",",";");

            var result = new List<string>();
            //var result = new List<string>();
            foreach (var item in message.Split(";"))
            {
                result.Add(item.Replace("\"",""));
            }

            return new BloggerResult(result);
        }
        public async Task<string> LiarAsync(string sentence)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "Your role is to assess whether the user question is allowed or not. \r\nThe allowed are only questions about the cities names. \r\nIf the topic is allowed, say 'YES' otherwise say 'NO'"),
                    new ChatMessage(ChatMessageRole.User, sentence)
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.FirstOrDefault().Message.Content.ToString();
            var result = message;

            return result;
        }

        public async Task<string> InpromptAsync(InpromptFilteredData input)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "Answer user question. You can base only on SOURCE. Is you don't know the answer, simply say 'i don't know'."),
                    new ChatMessage(ChatMessageRole.System, $"SOURCE ###: {string.Join(";", input.input)}"),
                    new ChatMessage(ChatMessageRole.User, input.question)
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.FirstOrDefault().Message.Content.ToString();
            var result = message;

            return result;
        }

        public async Task<List<float>> EmbeddingAsync(string sentence)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var embeddingRequest = new EmbeddingRequest()
            {
                Model = Model.AdaTextEmbedding,
                Input = sentence
            };
            var embeddingResult = await api.Embeddings.CreateEmbeddingAsync(embeddingRequest);
            var result = embeddingResult.Data[0].Embedding.ToList();  

            return result;
        }
    }
     public record ModeratorResponse(List<ModerationResult> Results);
    public record ModerationResult(bool Flagged, Dictionary<string, bool> Categories, Dictionary<string, double> CategoryScores);
    public record BloggerResult(List<string> Answer);
}