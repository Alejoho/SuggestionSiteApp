namespace SuggestionAppLibrary.DataAccess;

public interface IStatusData
{
   Task CreateStatus(StatusModel status);
   ValueTask<List<StatusModel>> GetAllStatusesAsync();
}