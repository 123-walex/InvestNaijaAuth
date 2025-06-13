using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using InvestNaijaAuth.Servicies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : Controller
    {
        private readonly ILogger<WalletController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWalletService _walletService;
        private readonly InvestNaijaDBContext _context;

        public WalletController(
            ILogger<WalletController> logger,
                 IConfiguration configuration, 
                    IWalletService walletService,
                       InvestNaijaDBContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _walletService = walletService;
            _context = context;
        }
        [HttpPost("CreateWallet")]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletDTO wallet)
        {
            var result = await _walletService.CreateWalletAsync(wallet);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("Getwallet/{walletId}")]
        public async Task<IActionResult> GetWalletById(Guid walletId)
        {
            var result = await _walletService.GetWalletAsync(new GetWalletDTO { WalletId = walletId });
            return Ok(result);
        }

        [HttpPost("CreditWallet")]
        public async Task<IActionResult> FundWallet([FromBody] FundWalletDTO fund)
        {
            var result = await _walletService.FundWalletAsync(fund);
            return Ok(result);
        }
        [HttpPost("Debitwallet")]
        public async Task<IActionResult> DebitWallet([FromBody] DeFundWalletDTO debit)
        {
            var result = await _walletService.DebitWalletAsync(debit);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{walletId}/GetWalletTransactions")]
        public async Task<IActionResult> GetWalletTransactions(Guid walletId)
        {
            var transactions = await _walletService.GetWalletTransactionsAsync(walletId);
            return Ok(transactions);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{walletId}/UserExists")]
        public async Task<IActionResult> WalletExists(Guid walletId)
        {
            var exists = await _walletService.WalletExistsAsync(walletId);
            return Ok(new { exists });
        }
    }
}
