using API.DTOs;
using API.Extensions;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(SignInManager<AppUser> signInManager) : BaseApiController
    {
        public AppUser? _user;

        [HttpPost("mylogin")]
        public async Task<ActionResult> Login([FromQuery] bool useCookies, [FromBody] LoginDto loginDto)
        {
            if(loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            _user = await signInManager.UserManager.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if(_user == null){
                return Unauthorized("Invalid email or password");
            }else{
                if(useCookies)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddDays(1),
                        Domain = "localhost"
                    };

                    Response.Cookies.Append("AuthCookie", "MyToken", cookieOptions);
                    return Ok(new {Message = "Login successful, Cookie set"});
                }else{
                    return Ok(new { Message = "Login successful, but no cookie was set.", Token = "YourGeneratedToken" });
                }
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.Email
            };

            var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded)
            {
                foreach (var error in result.Errors){
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem();
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            return NoContent();
        }

        [HttpGet("user-info")]
        public async Task<ActionResult> GetUserInfo()
        {
            if (User.Identity?.IsAuthenticated == false) return NoContent();

            var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);

            if(user == null) return Unauthorized();

            return Ok(new {
                user.FirstName,
                user.LastName,
                user.Email, 
                Address = user.Address?.ToDto()
            });
        }

        [HttpGet("auth-status")]
        public ActionResult GetAuthState()
        {
            return Ok(new {IsAuthenticated = User.Identity?.IsAuthenticated ?? false});
        }

        [Authorize]
        [HttpPost("address")]
        public async Task<ActionResult<Address>> CreateOrUpdateAddress(AddressDto addressDto)
        {
            var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);

            if(user.Address == null)
            {
                user.Address = addressDto.ToEntity();
            }else
            {
                user.Address.UpdateFromDto(addressDto);
            }

            var result = await signInManager.UserManager.UpdateAsync(user);

            if(!result.Succeeded) return BadRequest("Problem updating user address");

            return Ok(user.Address.ToDto());
        }

    }
}
