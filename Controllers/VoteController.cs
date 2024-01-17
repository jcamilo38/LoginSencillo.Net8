using Microsoft.AspNetCore.Mvc;

namespace Login.Controllers
{
    public class VoteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
