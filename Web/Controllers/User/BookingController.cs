using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.User
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
