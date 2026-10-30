using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using Marathon.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Runtime.CompilerServices;

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


        [HttpPost]
        public IActionResult AddNewUser(Add_New_UserDTO addDTO)
        {
            var user = new User
            {
                Username = addDTO.Username,
                FirstName = addDTO.FirstName,
                LastName = addDTO.LastName,
                DateOfBirth = addDTO.DateOfBirth,
                PhoneNumber = addDTO.PhoneNumber,

                // server-controlled:
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true,
            };
            var userAccount = new UserAccount
            {
                Email = addDTO.Username, 
                PasswordHash = _hasher.HashPassword(null, "defaultPassword"), 
                Role = "User", 
                UserAccountName = addDTO.FirstName + " " + addDTO.LastName
            };

            _db.users.Add(user);
            _db.accounts.Add(userAccount);
            _db.SaveChanges();
            return Ok("User was added successfully");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO regDto)
        {
            if (await _db.accounts.AnyAsync(u => u.Email == regDto.Email))
                return BadRequest("Email already exists");

            var regUser = new UserAccount
            {
                Email = regDto.Email,
                PasswordHash = null, // will be set below
                Role = "User",   // ignore regDto.Role — prevent self-promotion 
                UserAccountName = regDto.UserAccountName
                //Roles — Paticipant, Event Manager or Admin
            };

            regUser.PasswordHash = _hasher.HashPassword(regUser, regDto.Password);

            _db.accounts.Add(regUser);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful",
                userId = regUser.UserAccountId,
                email = regUser.Email
            });
        }

        [HttpPost("loginUser")]
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
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO logDto)
        {
            var user = await _db.accounts.FirstOrDefaultAsync(u => u.Email == logDto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = _hasher.VerifyHashedPassword(
                user, user.PasswordHash, logDto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            var token = new JwtServives(_config).GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserAccountId,
                    user.Email,
                    user.Role
                }
            });
        }

       
        // ------------------------------------------------------------
        // GET: api/auth/me   (requires JWT)
        // ------------------------------------------------------------
        [Authorize(Roles = "User/Participant")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile() 
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _db.accounts.FirstOrDefaultAsync(u => u.Email == email);


            if (user == null)
                return NotFound("Student profile not found");


            var profile = await _db.accounts
            .Where(r => r.UserAccountId == user.UserAccountId)
               .Include(r => r.Role)
               .Select(r => new
               {
                r.UserAccountId,
                r.Email,
                r.Role
            }).FirstOrDefaultAsync();

            return Ok(profile);
        }
        [Authorize(Roles = "Admin,Lecturer")]
        [HttpGet]
        public async Task<IActionResult> GetAllRegistration()
        {
            var users = await _db.accounts
            .Include(r => r.UserAccountId)
            .Include(r => r.Email)
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