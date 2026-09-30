using HRInfoAdvertisements.Application.DTOs.Authentication;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ============================================================
    // REGISTER
    // ============================================================

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ============================================================
    // LOGIN
    // ============================================================

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    // ============================================================
    // VERIFY EMAIL OTP
    // ============================================================

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _authService.VerifyEmailOtpAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ============================================================
    // RESEND EMAIL VERIFICATION OTP
    // ============================================================

    [HttpPost("resend-email-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailVerification(
        [FromBody] ResendEmailVerificationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _authService.ResendEmailVerificationAsync(request);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message =
                    "A new verification code could not be sent yet. " +
                    "Please wait before requesting another code."
            });
        }

        return Ok(new
        {
            Success = true,
            Message =
                "If the account requires email verification, " +
                "a verification code has been sent."
        });
    }

    // ============================================================
    // REFRESH TOKEN
    // ============================================================

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result =
            await _authService.RefreshTokenAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    // ============================================================
    // LOGOUT
    // ============================================================

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request)
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result =
            await _authService.LogoutAsync(
                userId,
                request.RefreshToken);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Logout failed."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = "Logout successful."
        });
    }

    // ============================================================
    // FORGOT PASSWORD
    // ============================================================

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var result =
            await _authService.ForgotPasswordAsync(request);

        return Ok(new
        {
            Success = result,
            Message =
                "If the email address exists, password reset instructions will be sent."
        });
    }

    // ============================================================
    // RESET PASSWORD
    // ============================================================

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        var result =
            await _authService.ResetPasswordAsync(request);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Password reset failed."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = "Password has been reset successfully."
        });
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result =
            await _authService.ChangePasswordAsync(
                userId,
                request);

        if (!result)
        {
            return BadRequest(new
            {
                Success = false,
                Message =
                    "The current password is incorrect, " +
                    "the new password is invalid, " +
                    "or the password could not be changed."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = "Password changed successfully."
        });
    }

    // ============================================================
    // CURRENT USER
    // ============================================================

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(
                userIdClaim,
                out var userId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid user identity."
            });
        }

        var result =
            await _authService.GetCurrentUserAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}