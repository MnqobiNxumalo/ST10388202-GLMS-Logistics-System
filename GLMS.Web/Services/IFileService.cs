using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services
{
    public interface IFileService
    {
        Task<string> SavePdfFileAsync(IFormFile file, string contractNumber);
        bool IsValidPdfFile(IFormFile file);
        byte[] GetFileBytes(string filePath);
        void DeleteFile(string filePath);
    }
}