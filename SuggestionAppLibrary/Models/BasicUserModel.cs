namespace SuggestionAppLibrary.Models;

public class BasicUserModel
{
   [BsonRepresentation(BsonType.ObjectId)]
   public string Id { get; set; } = string.Empty;
   public string DisplayName { get; set; } = string.Empty;

   public BasicUserModel()
   {
   }

   public BasicUserModel(UserModel user)
   {
      Id = user.Id;
      DisplayName = user.DisplayName;
   }
}
