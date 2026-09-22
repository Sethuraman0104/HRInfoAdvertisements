using System.Security.Claims;

using HRInfoAdvertisements.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/advertisements/{advertisementId:long}/media")]
public class AdvertisementMediaController : ControllerBase
{
    private readonly IAdvertisementMediaService _mediaService;

    public AdvertisementMediaController(
        IAdvertisementMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    // ============================================================
    // IMAGES
    // ============================================================

    [HttpPost("images")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        long advertisementId,
        IFormFile file,
        [FromForm] bool isPrimary = false)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Please select an image."
            });
        }

        try
        {
            await using var stream =
                file.OpenReadStream();

            var result =
                await _mediaService.UploadImageAsync(
                    userId,
                    advertisementId,
                    stream,
                    file.FileName,
                    file.ContentType,
                    isPrimary);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("images")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImages(
        long advertisementId)
    {
        try
        {
            long? userId = null;

            if (User.Identity?.IsAuthenticated == true &&
                TryGetUserId(out var authenticatedUserId))
            {
                userId = authenticatedUserId;
            }

            var result =
                await _mediaService.GetImagesAsync(
                    advertisementId,
                    userId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("images/{imageId:long}/primary")]
    [Authorize]
    public async Task<IActionResult> SetPrimaryImage(
        long advertisementId,
        long imageId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result =
                await _mediaService.SetPrimaryImageAsync(
                    userId,
                    advertisementId,
                    imageId);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Success = true,
                Message = "Primary image updated successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("images/{imageId:long}/order")]
    [Authorize]
    public async Task<IActionResult> UpdateImageOrder(
        long advertisementId,
        long imageId,
        [FromQuery] int displayOrder)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result =
                await _mediaService.UpdateImageOrderAsync(
                    userId,
                    advertisementId,
                    imageId,
                    displayOrder);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Success = true,
                Message = "Image order updated successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("images/{imageId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteImage(
        long advertisementId,
        long imageId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result =
                await _mediaService.DeleteImageAsync(
                    userId,
                    advertisementId,
                    imageId);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Success = true,
                Message = "Image deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ============================================================
    // VIDEOS
    // ============================================================

    [HttpPost("videos")]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> UploadVideo(
        long advertisementId,
        IFormFile file)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Please select a video."
            });
        }

        try
        {
            await using var stream =
                file.OpenReadStream();

            var result =
                await _mediaService.UploadVideoAsync(
                    userId,
                    advertisementId,
                    stream,
                    file.FileName,
                    file.ContentType);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("videos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVideos(
        long advertisementId)
    {
        try
        {
            long? userId = null;

            if (User.Identity?.IsAuthenticated == true &&
                TryGetUserId(out var authenticatedUserId))
            {
                userId = authenticatedUserId;
            }

            var result =
                await _mediaService.GetVideosAsync(
                    advertisementId,
                    userId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("videos/{videoId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteVideo(
        long advertisementId,
        long videoId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result =
                await _mediaService.DeleteVideoAsync(
                    userId,
                    advertisementId,
                    videoId);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Success = true,
                Message = "Video deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ============================================================
    // DOCUMENTS
    // ============================================================

    [HttpPost("documents")]
    [Authorize]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(
        long advertisementId,
        IFormFile file)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Please select a document."
            });
        }

        try
        {
            await using var stream =
                file.OpenReadStream();

            var result =
                await _mediaService.UploadDocumentAsync(
                    userId,
                    advertisementId,
                    stream,
                    file.FileName,
                    file.ContentType);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet("documents")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocuments(
        long advertisementId)
    {
        try
        {
            long? userId = null;

            if (User.Identity?.IsAuthenticated == true &&
                TryGetUserId(out var authenticatedUserId))
            {
                userId = authenticatedUserId;
            }

            var result =
                await _mediaService.GetDocumentsAsync(
                    advertisementId,
                    userId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("documents/{documentId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteDocument(
        long advertisementId,
        long documentId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result =
                await _mediaService.DeleteDocumentAsync(
                    userId,
                    advertisementId,
                    documentId);

            if (!result)
                return NotFound();

            return Ok(new
            {
                Success = true,
                Message = "Document deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    private bool TryGetUserId(out long userId)
    {
        userId = 0;

        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        return long.TryParse(
            userIdClaim,
            out userId);
    }
}