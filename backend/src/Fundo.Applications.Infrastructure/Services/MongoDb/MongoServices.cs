using Fundo.Applications.Apllication.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Fundo.Applications.Infrastructure.Services.MongoDb;

public class MongoServices:IMongoServices
{
     private readonly IMongoClient _client;
     private readonly IMongoDatabase _defaultMongoDataBase;

     public MongoServices(IConfiguration configuration)
     {
          var section = configuration.GetSection("MongoDbSettings");
          var connectionString = section.GetValue<string>("ConnectionString")
                                 ?? throw new ArgumentNullException("MongoDbSettings:ConnectionString");
          var configDb = section.GetValue<string>("LoanPaymentsDb") ?? section.GetValue<string>("DatabaseName")
               ?? throw new ArgumentNullException("MongoDbSettings:DatabaseName");

          _client = new MongoClient(connectionString);
          _defaultMongoDataBase = _client.GetDatabase(configDb);
     }
     public IMongoClient Client => _client;
     public IMongoDatabase Database => _defaultMongoDataBase;


     public IMongoCollection<T> GetCollection<T>(string collectionName, string? databaseName = null)
     {
          var db=string.IsNullOrEmpty(databaseName) ? _defaultMongoDataBase : _client.GetDatabase(databaseName);
          return db.GetCollection<T>(collectionName);
     }

     public Task InsertOneAsync<T>(string collectionName, T document, CancellationToken ct = default)
     {
          return GetCollection<T>(collectionName).InsertOneAsync(document, null, ct);
     }

     public Task InsertManyAsync<T>(string collection, IEnumerable<T> documents, CancellationToken ct = default)
     {
          return GetCollection<T>(collection).InsertManyAsync(documents, null, ct);
     }

 
}