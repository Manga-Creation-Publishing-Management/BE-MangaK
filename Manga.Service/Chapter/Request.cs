using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;

namespace Manga.Service.Chapter;

public class Request
{
    public class CreateChapterRequest
    {
        public required string Title { get; set; }
        public string? Summary { get; set; }
        // public DateTimeOffset Deadline { get; set; }
        public IFormFile? ManuscriptFileUrl { get; set; }
        public IFormFile? ChapterFileUrl { get; set; }
    }


    public class UpdateChapterRequest
    {
        public ChapterStatus? Status { get; set; }
        public IFormFile? ChapterFileUrl { get; set; }
        public string? Feedback { get; set; }
    }

}