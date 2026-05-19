using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GLMS.Web.Services
{
   

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "contracts");

            // Create directory if not exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> SavePdfFileAsync(IFormFile file, string contractNumber)
        {
            // Validate file
            if (!IsValidPdfFile(file))
            {
                throw new InvalidOperationException("Only PDF files are allowed.");
            }

            // Generate unique filename
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{contractNumber}_{timestamp}.pdf";
            // Remove invalid characters from filename
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            string filePath = Path.Combine(_uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return $"/uploads/contracts/{fileName}";
        }

        public bool IsValidPdfFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check extension
            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf")
                return false;

            // Check content type
            if (file.ContentType != "application/pdf")
                return false;

            return true;
        }

        public byte[] GetFileBytes(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            // Convert relative path to absolute
            string fullPath = filePath;
            if (filePath.StartsWith("/"))
            {
                fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            }

            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            return null;
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string fullPath = filePath;
            if (filePath.StartsWith("/"))
            {
                fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}