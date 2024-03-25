using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Moderation;

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
                    new ChatMessage(ChatMessageRole.User, "Potrzebujê napisaæ cztery którkie akapity na temat pizzy: Wstêp: kilka s³ów na temat historii pizzy,Niezbêdne sk³adniki na pizzê," +
                        "Robienie pizzy,Pieczenie pizzy w piekarniku.")
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);

            string message = chatResponse.Choices.FirstOrDefault().Message.Content.ToString().Replace("[","").Replace("]","").Replace("\",",";");


            var result = new List<string>();
            foreach (var item in message.Split(";"))
            {
                result.Add(item.Replace("\"",""));
            }

            //BloggerResult result = JsonSerializer.Deserialize<BloggerResult>(message!);


            return new BloggerResult(result);
        }
    }

    public record ModeratorResponse(List<ModerationResult> Results);
    public record ModerationResult(bool Flagged, Dictionary<string, bool> Categories, Dictionary<string, double> CategoryScores);
    public record BloggerResult(List<string> Answer);
}