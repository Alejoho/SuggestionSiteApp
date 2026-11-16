using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class MongoUserData : IUserData
{
   private IMongoCollection<UserModel> _userCollection;

   public MongoUserData(IDbConnection dbConnection)
   {
      _userCollection = dbConnection.UserCollection;
   }

   public async Task<List<UserModel>> GetAllUserAsync()
   {
      var output = await _userCollection.FindAsync(_ => true);

      return output.ToList();
   }

   public async Task<UserModel?> GetUserByIdAsync(string id)
   {
      var output = await _userCollection.FindAsync(u => u.Id == id);
      return output.FirstOrDefault();
   }

   public async Task<UserModel?> GetUserByObjectId(string objectId)
   {
      var output = await _userCollection.FindAsync(u => u.ObjectIdentifier == objectId);
      return output.FirstOrDefault();
   }

   public Task CreateUser(UserModel user) => _userCollection.InsertOneAsync(user);

   public Task UpsertUser(UserModel user)
   {
      var filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
      return _userCollection.ReplaceOneAsync(
         filter,
         user,
         new ReplaceOptions() { IsUpsert = true });
   }
}
