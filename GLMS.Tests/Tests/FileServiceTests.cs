using GLMS.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace GLMS.Tests
{
    public class FileServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;

        public FileServiceTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        }

        [Fact]
        public void IsValidPdfFile_WithValidPdfFile_ReturnsTrue()
        {
            var service = new FileService(_mockEnvironment.Object);
            var mockFile = CreateMockPdfFile("contract.pdf", "application/pdf");
            bool result = service.IsValidPdfFile(mockFile.Object);
            Assert.True(result);
        }

        [Fact]
        public void IsValidPdfFile_WithExeFile_ReturnsFalse()
        {
            var service = new FileService(_mockEnvironment.Object);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("malware.exe");
            mockFile.Setup(f => f.ContentType).Returns("application/x-msdownload");
            mockFile.Setup(f => f.Length).Returns(1024);

            bool result = service.IsValidPdfFile(mockFile.Object);
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdfFile_WithEmptyFile_ReturnsFalse()
        {
            var service = new FileService(_mockEnvironment.Object);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            bool result = service.IsValidPdfFile(mockFile.Object);
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdfFile_WithWrongContentType_ReturnsFalse()
        {
            var service = new FileService(_mockEnvironment.Object);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.pdf");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.Length).Returns(1024);

            bool result = service.IsValidPdfFile(mockFile.Object);
            Assert.False(result);
        }

        [Fact]
        public void IsValidPdfFile_WithNullFile_ReturnsFalse()
        {
            var service = new FileService(_mockEnvironment.Object);
            bool result = service.IsValidPdfFile(null);
            Assert.False(result);
        }

        private Mock<IFormFile> CreateMockPdfFile(string fileName, string contentType)
        {
            var mockFile = new Mock<IFormFile>();
            byte[] pdfHeader = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4");
            var stream = new MemoryStream(pdfHeader);

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(pdfHeader.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

            return mockFile;
        }
    }
}