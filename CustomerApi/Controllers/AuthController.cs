using CustomerApi.Data;
using CustomerApi.DTOs;
using CustomerApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly CustomerDbContext _context;

        public AuthController(IConfiguration configuration, CustomerDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("loginuser")]
        public async Task<ActionResult<LoginResponseDto>> LoginUser(LoginRequestDto loginRequest)
        {
            //find user that matches username in loginRequest sent in
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            //user attempting log in was not in db
            if (user == null)
            {
                return Unauthorized();
            }

            var hasher = new PasswordHasher<User>();

            var passwordResult = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginRequest.Password);

            //password in db does not match password sent in
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, loginRequest.Username),
            new Claim(ClaimTypes.Role, "User")
            };

            //read key from configuration object
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            var response = new LoginResponseDto
            {
                Token = tokenString,
                Username = loginRequest.Username,
                Role = "User"
            };

            return Ok(response);
        }

        [HttpPost("loginadmin")]
        public async Task<ActionResult<LoginResponseDto>> LoginAdmin(LoginRequestDto loginRequest)
        {
            //find user that matches username in loginRequest sent in
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

            //user attempting log in was not in db
            if (user == null)
            {
                return Unauthorized();
            }

            var hasher = new PasswordHasher<User>();

            var passwordResult = hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginRequest.Password);

            //password in db does not match password sent in
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized();
            }

            /* Verify the user's actual role is admin before 
               granting admin access. Since user is authenticated, but does
               not have admin role return 403 Forbidden
            */
            if (!string.Equals(user.Role, "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            //send in the actual role of the user read from db 
            //not a hard coded Admin role
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //read key from configuration object
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            var response = new LoginResponseDto
            {
                Token = tokenString,
                Username = loginRequest.Username,
                Role = "Admin"
            };

            return Ok(response);
        }

    }
}
