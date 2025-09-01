using AutoMapper;
using System;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.Entities;
using InvestNaijaAuth.Enums;
using System.Transactions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Servicies
{
    public interface IWalletService
    {
        Task<CreateWalletDTO> CreateWalletAsync(CreateWalletDTO wallet);
        Task<GetWalletDTO> GetWalletAsync(GetWalletDTO get);
        Task<FundWalletDTO>FundWalletAsync(FundWalletDTO fund);
        Task<DeFundWalletDTO>DebitWalletAsync(DeFundWalletDTO debit);
        Task<IEnumerable<CheckTransactionDTO>> GetWalletTransactionsAsync(Guid walletId);
        Task<bool> WalletExistsAsync(Guid walletId);
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
                AccountBalance = 30000,
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

            fundwallet.AccountBalance += fund.Amount;

            _context.WalletTransaction.Add(transaction);
            _context.Wallet.Update(fundwallet);
            await _context.SaveChangesAsync();

            return fund;
        }
        public async Task<DeFundWalletDTO>DebitWalletAsync(DeFundWalletDTO debit)
        {
            var defund = await _context.Wallet.FindAsync(debit.WalletId);

            if (defund == null)
                throw new Exception("Wallet not found");

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = defund.WalletId,
                Amount = debit.Amount,
                Type = TransactionType.Debit,
                Description = debit.Description ?? "Wallet Debited",
                PerformedAt = DateTime.UtcNow
            };

            if (defund.AccountBalance < debit.Amount)
                throw new Exception("Insufficient funds");

            defund.AccountBalance -= debit.Amount;

            _context.WalletTransaction.Add(transaction);
            _context.Wallet.Update(defund);
            await _context.SaveChangesAsync();

            return debit;

        }
        public async Task<IEnumerable<CheckTransactionDTO>> GetWalletTransactionsAsync(Guid walletId)
        {
            var transactions = await _context.WalletTransaction
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.PerformedAt)
                .ToListAsync();

            return transactions.Select(t => new CheckTransactionDTO
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                Type = t.Type,
                Description = t.Description,
                PerformedAt = t.PerformedAt
            });
        }
        public async Task<bool> WalletExistsAsync(Guid walletId)
        {
            return await _context.Wallet.AnyAsync(w => w.WalletId == walletId);
        }
    }
}
