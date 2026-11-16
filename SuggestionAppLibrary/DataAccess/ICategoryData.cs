namespace SuggestionAppLibrary.DataAccess;

public interface ICategoryData
{
   Task CreateCategory(CategoryModel category);
   ValueTask<List<CategoryModel>> GetAllCategoriesAsync();
}