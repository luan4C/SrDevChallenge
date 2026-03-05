using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Mongo;

public static class MongoDbHelpers
{
    public static bool IsDuplicatedKeyException(MongoBulkWriteException ex)
    {
        return ex.WriteErrors.Any(e => e.Code == 11000);
    }
}
