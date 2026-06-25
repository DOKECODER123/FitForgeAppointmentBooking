using Microsoft.AspNetCore.Mvc;

namespace AppointmentBooking.Controllers;

public class AuthController : Controller
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var validUser = _config["AdminCredentials:Username"];
        var validPass = _config["AdminCredentials:Password"];

        if (username == validUser && password == validPass)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToAction("Index", "Admin");
        }

        ViewBag.Error = "Invalid username or password.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}