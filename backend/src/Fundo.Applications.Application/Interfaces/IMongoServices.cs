using MongoDB.Driver;

namespace Fundo.Applications.Apllication.Interfaces;

public interface IMongoServices
{
    IMongoClient Client { get; }
    IMongoDatabase Database { get; }
    public IMongoCollection<T> GetCollection<T>(string collectionName,string? databaseName = null);
    Task InsertOneAsync<T>(string collectionName, T document, CancellationToken ct = default);
    Task InsertManyAsync<T>(string collectionName, IEnumerable<T> documents, CancellationToken ct = default);
    
}