using InvestNaijaAuth.Data;
using InvestNaijaAuth.Servicies;
using Microsoft.AspNetCore.Mvc;

namespace InvestNaijaAuth.Controllers
{
    
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
    }
}
