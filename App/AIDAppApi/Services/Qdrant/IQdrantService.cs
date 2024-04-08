
namespace AIDAppApi.Services.Qdrant
{
    public interface IQdrantService
    {
        Task<string> CreateCollection(string collectionName, ulong collectionSize);
        Task AddDataToCollectionAsync(string collectionName, Dictionary<Guid, List<float>> qdrantInput, CancellationToken token);
        Task <Guid> SearchAsync(string collectionName, List<float> questionVectors, CancellationToken token);
    }
}