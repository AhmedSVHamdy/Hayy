using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.DTO.Plans;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubscriptionPlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ===== GetAll - Active فقط =====
        public async Task<IEnumerable<SubscriptionPlanResponseDto>> GetAllAsync()
        {
            var plans = await _unitOfWork.SubscriptionPlans.GetAllAsync();

            return _mapper.Map<IEnumerable<SubscriptionPlanResponseDto>>(
                plans.Where(p => p.IsActive)
            );
        }

        // ===== GetById =====
        public async Task<SubscriptionPlanResponseDto?> GetByIdAsync(Guid id)
        {
            var plan = await _unitOfWork.SubscriptionPlans
                .GetAsync(p => p.Id == id && p.IsActive);

            if (plan is null)
                return null;

            return _mapper.Map<SubscriptionPlanResponseDto>(plan);
        }

        // ===== AddPlan =====
        public async Task<SubscriptionPlanResponseDto> AddPlanAsync(AddSubscriptionPlanRequestDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            // Map من الـ DTO للـ Entity
            var plan = _mapper.Map<SubscriptionPlan>(dto);

            // الحاجات اللي بنعملها يدوياً
            plan.Id = Guid.NewGuid();   // ✅ دايماً NewGuid
            plan.IsActive = true;

            await _unitOfWork.SubscriptionPlans.AddAsync(plan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubscriptionPlanResponseDto>(plan);
        }

        // ===== UpdatePlan - PUT =====
        public async Task<SubscriptionPlanResponseDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequestDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            var plan = await _unitOfWork.SubscriptionPlans
                .GetAsync(p => p.Id == id && p.IsActive);

            if (plan is null)
                return null;

            // بنعمل Map من الـ DTO على الـ Entity الموجودة مباشرةً ✅
            _mapper.Map(dto, plan);

            _unitOfWork.SubscriptionPlans.Update(plan);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SubscriptionPlanResponseDto>(plan);
        }

        // ===== DeletePlan - Soft Delete =====
        public async Task<bool> DeletePlanAsync(Guid id)
        {
            var plan = await _unitOfWork.SubscriptionPlans
                .GetAsync(p => p.Id == id && p.IsActive);

            if (plan is null)
                return false;

            _unitOfWork.SubscriptionPlans.SoftDelete(plan);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
