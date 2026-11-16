using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class DbConnection : IDbConnection
{
   private readonly IConfiguration _config;
   private readonly IMongoDatabase _db;
   private readonly string _connectionId = "MongoDB";

   public string DatabaseName { get; private set; }
   public string CategoryCollectionName { get; private set; } = "categories";
   public string StatusCollectionName { get; private set; } = "statuses";
   public string UserCollectionName { get; private set; } = "users";
   public string SuggestionCollectionName { get; private set; } = "suggestions";

   public IMongoClient Client { get; private set; }
   public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }
   public IMongoCollection<StatusModel> StatusCollection { get; private set; }
   public IMongoCollection<UserModel> UserCollection { get; private set; }
   public IMongoCollection<SuggestionModel> SuggestionCollection { get; private set; }

   public DbConnection(IConfiguration config)
   {
      _config = config;
      Client = new MongoClient(_config.GetConnectionString(_connectionId));
      DatabaseName = _config["DatabaseName"];
      _db = Client.GetDatabase(DatabaseName);

      CategoryCollection = _db.GetCollection<CategoryModel>(CategoryCollectionName);
      StatusCollection = _db.GetCollection<StatusModel>(StatusCollectionName);
      UserCollection = _db.GetCollection<UserModel>(UserCollectionName);
      SuggestionCollection = _db.GetCollection<SuggestionModel>(SuggestionCollectionName);
   }
}
