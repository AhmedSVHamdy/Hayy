using Microsoft.AspNetCore.Http;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IAuthUsers
    {
        public Task<User> Register(RegisterDTO registerDTO, IFormFile? image);
        public Task<User> Login(LoginDTO loginDTO);
    }
}
