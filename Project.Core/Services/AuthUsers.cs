using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class AuthUsers : IAuthUsers
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IImageService _imageService;   // التعامل مع صور Azure
        private readonly IValidator<RegisterDTO> _registerDtoValidator;



        public AuthUsers(UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IImageService imageService,
            IValidator<RegisterDTO> registerDtoValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _registerDtoValidator = registerDtoValidator;
        }
        public async Task<User> Register(RegisterDTO registerDTO, IFormFile? image)

        {
            //  Validate DTO
            var valResult = await _registerDtoValidator.ValidateAsync(registerDTO);
            if (!valResult.IsValid)
            {
                // ValidationException
                var errors = string.Join(", ", valResult.Errors.Select(e => e.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //  Upload Image
            string? profileImageUrl = null;
            if (image != null && image.Length > 0)
            {
                profileImageUrl = await _imageService.UploadImageAsync(image);
            }

            // Map DTO to Entity
            User user = new User()
            {
                FullName = registerDTO.FullName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email, 
                ProfileImage = profileImageUrl,
                City = registerDTO.City,
                CreatedAt = DateTime.UtcNow, 
                UserType = UserType.User.ToString(),
                IsVerified = false 
            };

            // Add User
            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Registration Failed: {errors}");
            }

            //  Handle Roles
            string roleName = UserType.User.ToString();

            // check role
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                ApplicationRole applicationRole = new ApplicationRole()
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper() 
                };
                // create role
                await _roleManager.CreateAsync(applicationRole);
            }
            // add role
            await _userManager.AddToRoleAsync(user, roleName);

            // sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            return user;
        }
        




        public async Task<User> Login(LoginDTO loginDTO)
        {
            //  Check if user exists
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);

            if (user == null)
            {
                throw new ArgumentException("Invalid Email or Password");
            }

            //  Check Password
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new ArgumentException("Invalid Email or Password");
            }


            return user;
        }
    }


    }

