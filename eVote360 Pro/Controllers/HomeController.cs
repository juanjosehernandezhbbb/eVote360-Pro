using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using eVote360_Pro.Models; 
namespace eVote360_Pro.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
                        if (User.Identity.IsAuthenticated)
            {
                ViewBag.MensajeBienvenida = $"Bienvenido al sistema eVote360 Pro, {User.Identity.Name}.";
            }
            else
            {
                ViewBag.MensajeBienvenida = "Bienvenido a eVote360 Pro. Por favor, inicie sesión para participar o gestionar el proceso electoral.";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}