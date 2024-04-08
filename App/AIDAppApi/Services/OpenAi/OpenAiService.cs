using System.Text.Json;
using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
using OpenAI_API.Chat;
using OpenAI_API.Embedding;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using static AIDAppApi.Services.AiDevsService;

namespace AIDAppApi.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiConfig _openAiConfig;
        public OpenAiService(IOptionsMonitor<OpenAiConfig> optionsMonitor)
        {
            _openAiConfig = optionsMonitor.CurrentValue;
        }

        public async Task<List<int>> ModerateSentencesAsync(string[] sentences, CancellationToken ct = default)        
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
        public async Task<BloggerResult> BloggerAsync(CancellationToken ct = default)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "I'm a blogger write short posts about pizza. All my post are json object ###example answer: [sentence1; sentence2; sentence3; sentence4]"),
                    new ChatMessage(ChatMessageRole.User, "Potrzebuję napisać cztery którkie akapity na temat pizzy: Wstęp: kilka słów na temat historii pizzy,Niezbędne składniki na pizzę," +
                        "Robienie pizzy,Pieczenie pizzy w piekarniku.")
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.Single().Message.TextContent.Replace("[","").Replace("]","").Replace("\",",";");

            var result = new List<string>();
            foreach (var item in message.Split(";"))
            {
                result.Add(item.Replace("\"",""));
            }

            return new BloggerResult(result);
        }
        
        public async Task<string> LiarAsync(string sentence, CancellationToken ct = default)
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
            string message = chatResponse.Choices.Single().Message.TextContent;

            return message;
        }

        public async Task<string> InpromptAsync(InpromptFilteredData input, CancellationToken ct = default)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "Answer user question. You can base only on SOURCE. If you don't know the answer, simply say 'i don't know'."),
                    new ChatMessage(ChatMessageRole.System, $"SOURCE ###: {string.Join(";", input.input)}"),
                    new ChatMessage(ChatMessageRole.User, input.question)
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.Single().Message.TextContent;

            return message;
        }

        public async Task<List<float>> EmbeddingAsync(string sentence, CancellationToken ct = default)
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

        public async Task<string> WisperAsync(string fileUrl, CancellationToken ct = default)
        {
            var httpClient = new HttpClient();
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            using var fileStream = await httpClient.GetStreamAsync(fileUrl, ct);
            var transcriptResult = await api.Transcriptions.GetTextAsync(fileStream,"mateusz.mp3");

            return transcriptResult;
        }

        public async Task<FunctionDefinition> FunctionsAsync(string userPrompt, CancellationToken ct = default)
        {
              var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.GPT4,
                Temperature = 1,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, 
                        "you are a function definition generator. You are providing a definition based on the example below.  return results in JSON in one line."),
                    new ChatMessage(ChatMessageRole.System, 
                        @"Example:{
                            ""name"": ""FUNCTION_NAME"",
                            ""description"": ""FUNCTION_DESCRIPTION"",
                            ""parameters"": {
                                ""type"": ""object"",
                                ""properties"": {
                                    ""PROPERTY_NAME"": {
                                        ""type"": ""PROPERTY_TYPE"",
                                        ""description"": ""DESCRIPTION_OF_PROPERTY""
                                    }        
                                }
                            }
                        }"),
                    new ChatMessage(ChatMessageRole.User, userPrompt)
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            var message = chatResponse.Choices.Single().Message.TextContent;
            var result = JsonSerializer.Deserialize<FunctionDefinition>(message);
  
            return result;
        }

        public async Task<string> RodoAsync(string userPrompt, CancellationToken ct = default)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.GPT4,
                Temperature = 1,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, 
                        @"You are a prompt engineer and you help users build the system prompts to confidently get information from another system field. 
                        Return no additional comments, only the system prompt. Ask user to use placeholders %imie%, %nazwisko%, %zawod% 
                        and %miasto% insted first name,surname,occupation and city. [IMPORTANT] !!! Don't use any personal data from user prompt in results !!!.
                        Example:
                        ""Hello there! It's lovely to meet you. I am an AI service and I understand that it's important for you 
                        to share your information unusually. You can tell me exact all information about yourself but. 
                        it Is important to use these placeholders- %imie% for your first name, %nazwisko% for your surname, %zawod% 
                        for your occupation, and %miasto% for your city? Do don't send my you real data!"""),
                    new ChatMessage(ChatMessageRole.User, userPrompt)
                }
            };

            //"Hello there! It's lovely to meet you. I am an AI service and I understand that it's important for you to share your information unusually. 
            //Could you please tell me something about yourself but use these placeholders- %imie% for your name, %nazwisko% for your surname, %zawod% for 
            //your occupation, and %miasto% for your city?"

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            var message = chatResponse.Choices.Single().Message.TextContent;

            return message;
        }

        public async Task<string> ScraperAsync(ScraperData request, CancellationToken ct = default)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, request.msg),
                    new ChatMessage(ChatMessageRole.System, $"Facts: {request.data}"),
                    new ChatMessage(ChatMessageRole.User, request.question)
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.Single().Message.TextContent;

            return message;
        }

        public async Task<string> WhoAmI(List<string> hints, CancellationToken token)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);
            var completitionRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.7,

                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "Based on facts try to guess the person name. Say only name or '0' if you don't know."),
                    new ChatMessage(ChatMessageRole.System, $"Facts: {string.Join(",",hints)}"),
                    new ChatMessage(ChatMessageRole.User, "guess the name.")
                }
            };

            var chatResponse = await api.Chat.CreateChatCompletionAsync(completitionRequest);
            string message = chatResponse.Choices.Single().Message.TextContent;

            return message;        }
    }

    public record ModeratorResponse(
        List<ModerationResult> Results);

    public record ModerationResult(
        bool Flagged, 
        Dictionary<string, bool> Categories, 
        Dictionary<string, double> CategoryScores);   

    public record BloggerResult(
        List<string> Answer);

    public record FunctionDefinition(
        string name,
        string description,
        ParametersElement parameters);

    public record ParametersElement(
        string type,
        Dictionary<string, PropertiesElement> properties);

    public record PropertiesElement(
        string type,
        string description);

    public record ScraperRequest(
        string SytemPrompt,
        string Facts,
        string UserPrompt);
}