﻿using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Response.Archive;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sieve.Services;

namespace Application.Services
{
    internal class CopyService : CommonService<Copy, CopyRequest, CopyResponse>, ICopyService
    {
        public CopyService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor, ILogger<CopyService> logger, IUserResolverService userResolverService) : base(unitOfWork, mapper, sieveProcessor, logger, userResolverService)
        { }

        public override async Task<CopyResponse> AddAsync(CopyRequest dto)
        {
            var itemExist = await _unitOfWork.Set<Item>().AnyAsync(item => item.Id == dto.ItemId);
            if (itemExist is false)
            {
                throw new NotFoundException("item not found");
            }

            var library = await _unitOfWork.Set<Library>().AnyAsync(library => library.Id == dto.LibraryId);
            if (library is false)
            {
                throw new NotFoundException("library not found");
            }

            var copies = new List<Copy>();

            for (int i = 0; i < dto.Count; ++i)
            {
                copies.Add(new Copy { ItemId = dto.ItemId, LibraryId = dto.LibraryId, CopyHistory = new CopyHistory() });
            }

            await _unitOfWork.Set<Copy>().AddRangeAsync(copies);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"{_userResolverService.GetUserName} added {dto.Count} the copies item: {dto.ItemId} to library {dto.LibraryId}");

            return null;
        }

        public async Task<RentalResponse?> GetCurrentRentalAsync(string inventoryNumber)
        {
            var currentRental = await _unitOfWork.Set<Copy>()
                .AsNoTracking()
                .Where(copy => copy.InventoryNumber == inventoryNumber)
                .Select(copy => copy.CurrentRental)
                .ProjectTo<RentalResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (currentRental is null)
            {
                throw new NotFoundException();
            }

            if (currentRental.CopyInventoryNumber is null)
            {
                throw new NoContentException();
            }

            return currentRental;
        }

        public async Task<ReservationResponse?> GetCurrentReservationAsync(string inventoryNumber)
        {
            var currentReservation = await _unitOfWork.Set<Copy>()
                .AsNoTracking()
                .Where(copy => copy.InventoryNumber == inventoryNumber)
                .Select(copy => copy.CurrentReservation)
                .ProjectTo<ReservationResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (currentReservation is null)
            {
                throw new NotFoundException();
            }

            if (currentReservation.CopyInventoryNumber is null)
            {
                throw new NoContentException();
            }

            return currentReservation;
        }

        public async Task<CopyHistoryResponse> GetCopyHistoryAsync(string inventoryNumber)
        {
            var response = await _unitOfWork.Set<Copy>()
                .AsNoTracking()
                .Where(copy => copy.InventoryNumber == inventoryNumber)
                .Select(copy => copy.CopyHistory)
                .ProjectTo<CopyHistoryResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException();
            }

            return response;
        }

        public async Task<bool> IsAvailableAsync(string inventoryNumber)
        {
            var response = await _unitOfWork.Set<Copy>().Where(copy => copy.InventoryNumber == inventoryNumber).Select(copy => new { copy.IsAvailable }).FirstOrDefaultAsync();

            if(response is null)
            {
                throw new NotFoundException();
            }

            return response.IsAvailable;
        }
    }
}