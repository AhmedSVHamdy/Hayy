using Microsoft.AspNetCore.Mvc;
using Project.Core.Enums;

namespace Project.Core.DTO
{
    public class RegisterDTO
    {
        public string? FullName { get; set; }

        [Remote(action: "IsEmailAlreadyRegisterd", controller: "Account", ErrorMessage = "Email is already in use")]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        public string? City { get; set; }

    }
}




