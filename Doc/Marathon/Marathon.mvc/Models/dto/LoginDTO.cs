using System.ComponentModel.DataAnnotations;

namespace Marathon.mvc.Models.dto
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public LoginUserDto User { get; set; } = new();
    }

    public class LoginUserDto
    {
        public int UserAccountId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

   
        public class SignUpDTO
        {
            [Required, StringLength(50, MinimumLength = 3)]
            public string Username { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Phone]
            public string? PhoneNumber { get; set; }

            [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
            public string Password { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;   // client-only, never sent
        }


    }