using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace SuggestionAppLibrary.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
   private const string CacheName = "SuggestionData";
   private readonly IDbConnection _db;
   private readonly IMemoryCache _cache;
   private readonly IMongoCollection<SuggestionModel> _suggestionCollection;
   private readonly IMongoCollection<UserModel> _userCollection;

   public MongoSuggestionData(IDbConnection db, IMemoryCache cache)
   {
      _db = db;
      _cache = cache;
      _suggestionCollection = _db.SuggestionCollection;
      _userCollection = _db.UserCollection;
   }

   public async ValueTask<List<SuggestionModel>> GetAllSuggestions()
   {
      var output = _cache.Get<List<SuggestionModel>>(CacheName);

      if (output is null)
      {
         var results = await _suggestionCollection.FindAsync(s => s.Archived == false);
         output = results.ToList();

         _cache.Set(CacheName, output);
      }

      return output;
   }

   public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()
      => (await GetAllSuggestions())
         .Where(s => s.ApprovedForRelease == true)
         .ToList();

   public async Task<SuggestionModel> GetSuggestion(string id)
      => (await GetAllSuggestions())
         .First(s => s.Id == id);

   public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
      => (await GetAllSuggestions())
         .Where(s => s.ApprovedForRelease == false && s.Rejected == false)
         .ToList();

   public async Task UpdateSuggestion(SuggestionModel suggestion)
   {
      var result = await _suggestionCollection.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);

      _cache.Remove(CacheName);
   }

   public async Task UpvoteSuggestion(string suggestionId, string userId)
   {
      var client = _db.Client;
      using var session = await client.StartSessionAsync();
      session.StartTransaction();

      try
      {
         // Get the suggestion
         var suggestion = await GetSuggestion(suggestionId);

         // Add or remove the user id
         bool isUpvote = suggestion.UserVotes.Add(userId);

         if (isUpvote is false)
         {
            suggestion.UserVotes.Remove(userId);
         }

         await _suggestionCollection.ReplaceOneAsync(
            session, s => s.Id == suggestion.Id, suggestion);

         // Get the user
         var user = (await _userCollection.FindAsync(u => u.Id == userId)).First();

         // Add or remove the suggestion
         if (isUpvote)
         {
            user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
         }
         else
         {
            var votedOnSuggestion = user.VotedOnSuggestions.First(s => s.Id == suggestion.Id);

            user.VotedOnSuggestions.Remove(votedOnSuggestion);
         }

         await _userCollection.ReplaceOneAsync(
            session, u => u.Id == userId, user);

         await session.CommitTransactionAsync();

         _cache.Remove(CacheName);
      }
      catch
      {
         await session.AbortTransactionAsync();
         throw;
      }
   }

   public async Task CreateSuggestion(SuggestionModel suggestion)
   {
      var client = _db.Client;
      using var session = await client.StartSessionAsync();

      await session.WithTransactionAsync(async (session, ct) =>
      {
         // Insert the suggestion
         var db = client.GetDatabase(_db.DatabaseName);
         var suggestionCollection = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);

         await suggestionCollection.InsertOneAsync(session, suggestion, cancellationToken: ct);

         // Add the suggestion to the user
         var userCollection = db.GetCollection<UserModel>(_db.UserCollectionName);

         var user = (await userCollection.FindAsync(
            u => u.Id == suggestion.Author.Id,
            cancellationToken: ct))
               .First(cancellationToken: ct);

         user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));

         await userCollection.ReplaceOneAsync(
            session,
            u => u.Id == suggestion.Author.Id,
            user,
            cancellationToken: ct);

         return true;
      });

      _cache.Remove(CacheName);
   }
}
