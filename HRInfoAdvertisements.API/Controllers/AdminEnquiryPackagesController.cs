using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/admin/enquiry-packages")]
[Authorize(Policy = "ADVERTISEMENT_VIEW")]
public sealed class AdminEnquiryPackagesController : ControllerBase
{
    private readonly IEnquiryPackageAdminService _service;

    public AdminEnquiryPackagesController(IEnquiryPackageAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(cancellationToken);

        return Ok(new
        {
            Success = true,
            Data = result
        });
    }

    [HttpPut("{enquiryPackageId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> Update(
        int enquiryPackageId,
        [FromBody] UpdateEnquiryPackageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(
                enquiryPackageId,
                request,
                cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Enquiry package not found."
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Enquiry package updated.",
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                Success = false,
                Message = "The package update conflicts with existing data."
            });
        }
    }
}
