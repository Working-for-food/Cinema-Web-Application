using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;

namespace Application.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}