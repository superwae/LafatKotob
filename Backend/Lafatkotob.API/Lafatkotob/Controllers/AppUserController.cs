using Lafatkotob.Entities;
using Lafatkotob.Services;
using Lafatkotob.Services.AppUserService;
using Lafatkotob.Services.EmailService;
using Lafatkotob.Services.UserPreferenceService;
using Lafatkotob.ViewModel;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Claims;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppUserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUserPreferenceService _userPreferenceService;

        public AppUserController(
            IUserService userService,
            IEmailService emailService,
            UserManager<AppUser> userManager,
            IUserPreferenceService userPreferenceService
        )
        {
            _userService = userService;
            _emailService = emailService;
            _userManager = userManager;
            _userPreferenceService = userPreferenceService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginResult = await _userService.LoginUser(model);

            if (!loginResult.Success)
            {
                return Unauthorized(loginResult.ErrorMessage);
            }

            return Ok(loginResult);
        }
[HttpPost("Contact")]
public async Task<IActionResult> Contact([FromBody] SendEmailModel model)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState); // Returns detailed error information
    }
            string email = "lafatkotobreports@gmail.com";
    string subject = $"Report from user: {model.Name}";
    string body = $@" {model.Email},<br><br> {model.Message}";

    try
    {
        await _emailService.SendEmailAsync(email, subject, body);
        return Ok(new { message = "Email sent successfully" });
    }
    catch (Exception ex)
    {
        // Log the exception details here for debugging purposes
        return StatusCode(500, "Failed to send email");
    }
}


        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUsers(int pageNumber, int pageSize)
        {
            var users = await _userService.GetAllUsers(pageNumber, pageSize);
            return Ok(users);
        }
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        [HttpGet("GetUserByUserName")]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            var user = await _userService.GetUserByUserName(userName);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var UserName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user.UserName != UserName && !User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                return BadRequest("you are not authorized to delete this user");
            }
            var result = await _userService.DeleteUser(userId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok("User deleted successfully.");
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("UpdateUser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser(string userId, [FromForm] UpdateUserModel model, IFormFile imageFile)
        {
            var optionalFields = new List<string> { "Name", "Email", "UserName", "CurrentPassword", "NewPassword", "ConfirmNewPassword", "ProfilePictureUrl", "About", "City" };
            foreach (var field in optionalFields)
            {
                ModelState.Remove(field);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }

            var UserName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user.UserName != UserName && !User.HasClaim(ClaimTypes.Role, "Admin"))
            {
                return BadRequest("you are not authorized to update this user");
            }
            var result = await _userService.UpdateUser(model, userId, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok("User updated successfully.");
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("updateProfilePicture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm]  string UserId, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }

            var result = await _userService.UpdateProfilePicture(UserId, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("updateBio")]
        public async Task<IActionResult> UpdateBio([FromBody] BioUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateBio(model.Id, model.About);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("updateCity")]
        public async Task<IActionResult> UpdateCity([FromBody] CityUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateCity(model.Id, model.City);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("UpdateUserSetting")]
        public async Task<IActionResult> UpdateUserSetting([FromBody] UpdateUserSettingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Update non-email related settings
            user.UserName = model.UserName ?? user.UserName;
            user.Name = model.UserName ?? user.Name;
            user.City = model.City ?? user.City;
            user.About = model.About ?? user.About;

            // Check and process email changes
            if (model.Email != null && model.Email != user.Email)
            {
                // Generate email change token and send confirmation link
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                            new { userId = user.Id, token = emailToken },
                                            protocol: Request.Scheme);

                string subject = "Confirm Your New Email";
                string body = $@"Hello {user.UserName},<br><br>
                        Please confirm your new email by clicking the link below:<br>
                        <a href='{confirmationLink}'>Confirm Your New Email</a><br><br>
                        Thank you.";

               await _emailService.SendEmailAsync(model.Email, subject, body);
           

                // Mark the email as unconfirmed until the new email is verified
                user.EmailConfirmed = false;
            }

            // Update user information in the database
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(model);
        }


        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromQuery] string role, [FromForm] RegisterModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }

            var result = await _userService.RegisterUser(model, role, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(result.Data);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                    new { userId = result.Data.Id, token = token },
                                    protocol: Request.Scheme);

            string subject = "Confirm Your Account";
            string body = $@"Hello {result.Data.UserName},<br><br>
                                Please confirm your account by clicking the link below:<br>
                                <a href='{confirmationLink}'>Confirm Your Account</a><br><br>
                                Thank you.";

            await _emailService.SendEmailAsync(result.Data.Email, subject, body);


            return Ok(result.Data);
        }


        [HttpPost("resendemail")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> resendemail([FromQuery]string userId, string email)
        {
            var user = _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return BadRequest();
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user.Result);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                    new { userId = user.Result.Id, token = token },
                                    protocol: Request.Scheme);

            string subject = "Confirm Your Account";
            string body = $@"Hello {user.Result.UserName},<br><br>
                                Please confirm your account by clicking the link below:<br>
                                <a href='{confirmationLink}'>Confirm Your Account</a><br><br>
                                Thank you.";

            await _emailService.SendEmailAsync(user.Result.Email, subject, body);


            return Ok(user.Result);
        }



        [HttpGet("validateToken")]
        public IActionResult ValidateToken()
        {
            var isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                return Unauthorized("Token is invalid or expired.");
            }
            var userName = HttpContext.User.Identity.Name;
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { UserName = userName, UserId = userIdClaim });
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            //var user = await _userManager.FindByEmailAsync(model.Email); use this when the email in unique
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);


                var frontendResetLink = $"http://localhost:4200/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";
                await _emailService.SendPasswordResetEmailAsync(model.Email, frontendResetLink, user.UserName);

            }
            return Ok(new { Message = "Please check your email to reset your password." });
        }

        [HttpPut]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            //var user = await _userManager.FindByEmailAsync(model.Email); use this when the email in unique
            if (user == null)
            {

                return BadRequest("Invalid request");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password has been reset successfully." });
            }
            return BadRequest(result.Errors);
        }

        [HttpPut]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            //var user = await _userManager.FindByEmailAsync(model.Email); use this when the email in unique
            if (user == null)
            {

                return BadRequest("Invalid request");
            }
            var passWordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if(!passWordCheck)
            {
                return BadRequest("incorrect old password");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Password has been changed successfully." });
            }
            return BadRequest(result.Errors);
        }

        [HttpPut]
        [Route("set-historyId")]
        public async Task<IActionResult> setHistoryId([FromBody] SetHistoryIdModel model)
        {
            if (model.UserId == null)
            {
                return BadRequest("User Id is required");
            }
            var result = await _userService.SetHistoryId(model.UserId, model.HistoryId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }


        [HttpPost("RegisterWithPreferences")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterWithPreferences([FromQuery] string role, [FromForm] RegisterModel model, [FromForm(Name = "genreIds")] List<int> genreIds, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }

            var userResult = await _userService.RegisterUser(model, role, imageFile);
            if (!userResult.Success)
            {
                return BadRequest(userResult.Message);
            }

            // Step 2: Save User Preferences
            var preferenceResult = await _userPreferenceService.SaveUserPreferences(userResult.Data.Id, genreIds);
            if (!preferenceResult.Success)
            {
                return BadRequest(preferenceResult.Message);
            }

            // Step 3: Send Email Confirmation 
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(userResult.Data);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                                        new { userId = userResult.Data.Id, token = token },
                                        protocol: Request.Scheme);

            string subject = "Confirm Your Account";
            string body = $@"Hello {userResult.Data.UserName},<br><br>
                    Please confirm your account by clicking the link below:<br>
                    <a href='{confirmationLink}'>Confirm Your Account</a><br><br>
                    Thank you.";

            await _emailService.SendEmailAsync(userResult.Data.Email, subject, body);

            return Ok();
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("ToggleDelete")]
        public async Task<IActionResult> ToggleDelete(string userId)
        {
            // Skip role check assuming the user is an admin

            var result = await _userService.ToggleDelete(userId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("updateUserRole")]
        public async Task<IActionResult> UpdateUserRole([FromQuery] string userId, [FromQuery] string role) // Use [FromQuery] for both parameters
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var result = await _userService.UpdateUserRole(userId, role);

            if (result.Success)
            {
                return Ok(new { Role = role });  // Ensure JSON structure
            }
            else
            {
                return BadRequest(new { Message = result.Message, Errors = result.Errors });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> UserSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required.");
            }

            var users = await _userService.UserSearch(query);


            return Ok(users.Data);
        }


    }
}
