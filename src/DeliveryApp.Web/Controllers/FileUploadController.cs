using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliveryApp.Web.Controllers
{
    [Route("api/app/file-upload")]
    [Authorize(Roles = "admin,manager,restaurant_owner")]
    public class FileUploadController : AbpController
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 30 * 1024 * 1024; // 5MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public FileUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit" });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only JPG, PNG, GIF, and WebP files are allowed." });
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/images/{fileName}";
                
                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    fileUrl = fileUrl,
                    fileName = fileName,
                    originalName = file.FileName,
                    size = file.Length
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        [HttpPost("category-image")]
        public async Task<IActionResult> UploadCategoryImage(IFormFile file)
        {
            return await UploadImage(file);
        }

        [HttpPost("restaurant-image")]
        public async Task<IActionResult> UploadRestaurantImage(IFormFile file)
        {
            return await UploadImage(file);
        }

        [HttpPost("menu-item-image")]
        public async Task<IActionResult> UploadMenuItemImage(IFormFile file)
        {
            return await UploadImage(file);
        }

        [HttpDelete("image/{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "images", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { success = true, message = "File deleted successfully" });
                }
                
                return NotFound(new { message = "File not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
            }
        }

        [HttpGet("image/{fileName}")]
        [AllowAnonymous]
        public IActionResult GetImage(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "images", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    var contentType = GetContentType(fileName);
                    return File(fileBytes, contentType);
                }
                
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
