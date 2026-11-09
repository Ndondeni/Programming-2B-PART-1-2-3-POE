using Microsoft.AspNetCore.Mvc;
namespace Marathon.MVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
