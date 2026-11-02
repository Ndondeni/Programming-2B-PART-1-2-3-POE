using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Marathon.Services;
using System.Security.Claims;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbcontext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<UserAccount> _hasher;

        public AuthController(
            ApplicationDbcontext db,
            IConfiguration config,
            IPasswordHasher<UserAccount> hasher)
        {
            _db = db;
            _config = config;
            _hasher = hasher;
        }

        [AllowAnonymous] // 🆕 Allows users to reach this endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register(Add_New_UserDTO addDTO)
        {
            if (await _db.accounts.AnyAsync(a => a.Email == addDTO.Email))
                return BadRequest("Email already exists");

            var userAccount = new UserAccount
            {
                Email = addDTO.Email,
                Role = "User",
                PasswordHash = string.Empty,
                UserAccountName = addDTO.FirstName + " " + addDTO.LastName
            };

            userAccount.PasswordHash = _hasher.HashPassword(userAccount, addDTO.Password);

            _db.accounts.Add(userAccount);
            await _db.SaveChangesAsync();

            var user = new User
            {
                UserId = userAccount.UserAccountId,
                Username = addDTO.FirstName.ToLower() + "." + addDTO.LastName.ToLower(),
                FirstName = addDTO.FirstName,
                LastName = addDTO.LastName,
                DateOfBirth = DateOnly.FromDateTime(addDTO.DateOfBirth),
                PhoneNumber = addDTO.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true,
            };

            _db.users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("User was added successfully");
        }

        [AllowAnonymous] // 🆕 Allows users to reach this endpoint
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginDTO logDto)
        {
            var user = await _db.accounts.FirstOrDefaultAsync(u => u.Email == logDto.Email);
            if (user == null) return Unauthorized("Invalid email or password");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, logDto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            var token = new JwtServives(_config).GenerateToken(user);

            return Ok(new
            {
                token,
                user = new { user.UserAccountId, user.Email, user.Role }
            });
        }

      
        [Authorize(Roles = "User")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _db.accounts.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound("User profile not found");

           
            var profile = new
            {
                user.UserAccountId,
                user.Email,
                user.Role
            };

            return Ok(profile);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllRegistration()
        {
           
            var users = await _db.accounts
                .Select(r => new
                {
                    r.UserAccountId,
                    r.Email,
                    r.Role,
                    r.UserAccountName
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}