using AutoMapper;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly IUserLogRepository _repository;
        private readonly IMapper _mapper; // ضيفنا المابر هنا

        public UserLogService(IUserLogRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task LogActivityAsync(CreateUserLogDto dto)
        {
            // 👇 بص النظافة! سطر واحد بيعمل كل حاجة
            var log = _mapper.Map<UserLog>(dto);

            await _repository.AddLogAsync(log);
        }
    }
}
