using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class MongoStatusData : IStatusData
{
   private const string CacheName = "StatusData";
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<StatusModel> _statusCollection;

   public MongoStatusData(IDbConnection dbConnection, IMemoryCache cache)
   {
      _cache = cache;
      _statusCollection = dbConnection.StatusCollection;
   }

   public async ValueTask<List<StatusModel>> GetAllStatusesAsync()
   {
      var output = _cache.Get<List<StatusModel>>(CacheName);

      if (output is null)
      {
         var results = await _statusCollection.FindAsync(_ => true);
         output = results.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromDays(1));
      }

      return output;
   }

   public Task CreateStatus(StatusModel status)
   {
      // Statuses won't change very often, so it's fine to have this order.
      _cache.Remove(CacheName);
      return _statusCollection.InsertOneAsync(status);
   }
}
