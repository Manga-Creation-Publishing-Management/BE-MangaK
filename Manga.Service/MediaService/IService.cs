using Microsoft.AspNetCore.Http;

namespace Manga.Service.MediaService;

public interface IService
{
    public Task<string> UploadImageAsync(IFormFile file);
    Task<(string FileUrl, string PublicId)> UploadFileAsync(IFormFile file); 
    Task DeleteFileAsync(string publicId);                                   
}