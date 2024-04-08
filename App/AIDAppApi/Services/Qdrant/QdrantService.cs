using AIDAppApi.Configurations;
using Microsoft.Extensions.Options;
using OpenAI_API.Moderation;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AIDAppApi.Services.Qdrant
{
    public class QdrantService : IQdrantService
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantConfig _config;
        private readonly QdrantClient _client;

        public QdrantService(IOptionsMonitor<QdrantConfig> optionsMonitor)
        {
            _config = optionsMonitor.CurrentValue;
            _httpClient = new HttpClient();
            var channel = QdrantChannel.ForAddress(_config.BaseAddress,
                new ClientConfiguration
                {
                    ApiKey= _config.Key,
                    CertificateThumbprint = "345183414e68f20b86299687ecb117126f8ff259827c6f23302b1fc9a9662fc9",
                }
            );
            var _grpcClient = new QdrantGrpcClient(channel);
            _client = new QdrantClient(_grpcClient);            
        }

        public async Task<string> CreateCollection(string collectionName, ulong collectionSize)
        {
            var result = "";
			//Create a collection named DB_1 with a size of 768 dimensions and Cosine distance
			await _client.CreateCollectionAsync(collectionName, new VectorParams { Size = collectionSize, Distance = Distance.Cosine });

            return result;
        }

        public async Task AddDataToCollectionAsync(string collectionName, Dictionary<Guid, List<float>> qdrantInput, CancellationToken ct)
        {
            var pointStruct = new List<PointStruct>();
            ulong id = 0;
            var points = qdrantInput.Select(x => new PointStruct
            {
                Id = ++id,
                Vectors = x.Value.ToArray(), 
                Payload = {
                    ["guid"] = x.Key.ToString(),
                }
            }).ToList();

            var operationInfo = await _client.UpsertAsync(
                collectionName: collectionName,
                points: points
            );
        }

        public async Task<Guid> SearchAsync(string collectionName, List<float> questionVectors, CancellationToken token)
        {
            var points = await _client.SearchAsync(
                collectionName,
                questionVectors.ToArray(),
                limit: 1);

            var point = points.Single();
            var x = point.Payload["guid"].StringValue;

            return Guid.Parse(x);
        }
    }
}