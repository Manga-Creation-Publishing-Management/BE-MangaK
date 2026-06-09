namespace Manga.Service.Category;

public class Response
{
    public class GetCategoryResponse
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}