using Microsoft.AspNetCore.Mvc;

namespace InvestNaijaAuth.Controllers
{
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
