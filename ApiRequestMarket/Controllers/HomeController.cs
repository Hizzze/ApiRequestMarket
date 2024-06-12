using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ApiRequestMarket.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiRequestMarket.Controllers;

public class HomeController : Controller
{
    private MainControllerUsers users;
    public HomeController(MainControllerUsers users)
    {
        this.users = users;
    }
    
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await users.getUserInfo(User.Identity.Name);
        HomeViewModel model;
        if (user.access_level > 0)
        {
            model = new HomeViewModel()
            {
                items = Database.getItemsList()
            };
        }
        else
        {
            model = new HomeViewModel();
        }
        return View(model);
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