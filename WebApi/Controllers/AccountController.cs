using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;

namespace WebApi.Controllers
{
    public class AccountController : CustomController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //public Task<ActionResult<User>> Register(RegisterDTO registerDTO)
        //{

        //    return
        //}
    }
}
