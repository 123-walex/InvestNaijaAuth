using AutoMapper;
using System;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Enums;
using System.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace InvestNaijaAuth.Servicies
{
    public interface IWalletService
    {
        Task<CreateWalletDTO> CreateWalletAsync(CreateWalletDTO wallet);
        Task<GetWalletDTO> GetWalletAsync(GetWalletDTO get);
        Task<FundWalletDTO>FundWalletAsync(FundWalletDTO fund);
        
       
    }
    public class WalletService : IWalletService
    {
        private readonly InvestNaijaDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<WalletService> _logger;

        public WalletService(InvestNaijaDBContext context, IMapper mapper , ILogger<WalletService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
       
        public async Task<CreateWalletDTO> CreateWalletAsync(CreateWalletDTO wallet)
        {
            var newwallet = new Wallet
            {
                WalletId= Guid.NewGuid(),
                UserId = wallet.UserId,
                Balance = 30000,
                CreatedAt = DateTime.UtcNow
            };

            _context.Wallet.Add(newwallet);
            await _context.SaveChangesAsync();

            return _mapper.Map<CreateWalletDTO>(newwallet);
        }

        public async Task<GetWalletDTO> GetWalletAsync(GetWalletDTO get)
        {
            var getwallet = await _context.Wallet.FindAsync(get.WalletId);

            if (getwallet == null)
                throw new Exception("Wallet not found");

            return _mapper.Map<GetWalletDTO>(getwallet);
        }
        public async Task<FundWalletDTO> FundWalletAsync(FundWalletDTO fund)
        {
            var fundwallet = await _context.Wallet.FindAsync(fund.WalletId);

            if (fundwallet == null)
                throw new Exception("Wallet not found");

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = fund.WalletId,
                Amount = fund.Amount,
                Type = TransactionType.Credit,
                Description = fund.Description ?? "Wallet funded",
                PerformedAt = DateTime.UtcNow
            };

            fundwallet.Balance += fund.Amount;

            _context.WalletTransaction.Add(transaction);
            _context.Wallet.Update(fundwallet);
            await _context.SaveChangesAsync();

            return fund;
        }
    }
}
