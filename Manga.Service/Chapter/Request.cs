using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.Chapter;

public class Request
{
    public class CreateChapterRequest
    {
        public required string Title { get; set; }
        public string? Summary { get; set; }
        public IFormFile? ManuscriptFileUrl { get; set; }
        public IFormFile? ChapterFileUrl { get; set; }
    }

}