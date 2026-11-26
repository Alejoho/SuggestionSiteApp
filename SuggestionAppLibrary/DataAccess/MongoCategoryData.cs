using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class MongoCategoryData : ICategoryData
{
   private const string CacheName = "CategoryData";
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<CategoryModel> _categoryCollection;


   public MongoCategoryData(IDbConnection dbConnection, IMemoryCache cache)
   {
      _cache = cache;
      _categoryCollection = dbConnection.CategoryCollection;
   }

   public async ValueTask<List<CategoryModel>> GetAllCategoriesAsync()
   {
      var output = _cache.Get<List<CategoryModel>>(CacheName);

      if (output is null)
      {
         var results = await _categoryCollection.FindAsync(_ => true);
         output = results.ToList();

         _cache.Set(CacheName, output, TimeSpan.FromDays(1));
      }

      return output;
   }

   public Task CreateCategory(CategoryModel category)
   {
      // Categories won't change very often, so it's fine to have this order.
      _cache.Remove(CacheName);
      return _categoryCollection.InsertOneAsync(category);
   }
}
