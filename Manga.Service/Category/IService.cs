namespace Manga.Service.Category;

public interface IService
{
    Task<List<Response.GetCategoryResponse>> GetCategories();
}