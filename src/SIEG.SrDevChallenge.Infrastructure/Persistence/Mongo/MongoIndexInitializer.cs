using System;
using MongoDB.Driver;
using SIEG.SrDevChallenge.Domain.Entities;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;


public class MongoIndexInitializer(IMongoDatabase database)
{
    private readonly IMongoDatabase _database = database;

    public async Task InitializeAsync()
    {
        var collection = _database
            .GetCollection<DocumentoFiscal>("documentosFiscais");

        var indexes = new List<CreateIndexModel<DocumentoFiscal>>
        {
            new(
                Builders<DocumentoFiscal>.IndexKeys.Ascending(x => x.HashXml),
                new CreateIndexOptions { Unique = true }),

            new(
                Builders<DocumentoFiscal>.IndexKeys.Ascending(x => x.ChaveAcesso)),

            new(
                Builders<DocumentoFiscal>.IndexKeys.Ascending(x => x.CriadoEm)),

            new(
                Builders<DocumentoFiscal>.IndexKeys.Ascending(x => x.Data))
        };

        await collection.Indexes.CreateManyAsync(indexes);
    }
}