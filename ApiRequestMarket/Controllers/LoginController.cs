using System.Security.Claims;
using ApiRequestMarket.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiRequestMarket;

public class LoginController : Controller
{
    private MainControllerUsers users;
    public LoginController(MainControllerUsers users)
    {
        this.users = users;
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (await Database.verifyUserData(email, password))
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, email) };
            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);
            await users.addUserToList(email);
            return RedirectToAction("Index", "Home"); // Перенаправление после успешного входа
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }
    }
    public async Task<IActionResult> Logout()
    {
        await users.deleteUserFromList(User.Identity.Name);
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await users.getUserInfo(User.Identity.Name);
        if (user == null)
        {
            return NotFound();
        }
        var model = new UserViewModel
        {
            Email = user.email,
        };
        return View(model);
    }
}