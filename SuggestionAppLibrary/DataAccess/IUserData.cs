namespace SuggestionAppLibrary.DataAccess;

public interface IUserData
{
   Task CreateUser(UserModel user);
   Task<List<UserModel>> GetAllUserAsync();
   Task<UserModel?> GetUserByIdAsync(string id);
   Task<UserModel?> GetUserByObjectId(string objectId);
   Task UpsertUser(UserModel user);
}