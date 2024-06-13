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
                Items = Database.getItemsList()
            };
        }
        else
        {
            model = new HomeViewModel();
        }
        return View(model);
    }

    public IActionResult Add()
    {
        return RedirectToAction("Add", "Add");
    }

    public async Task<IActionResult> Delete(int id)
    {
        Database.DeleteItemFromDatabase(id);
        MainControllerUsers.sendReloadToApi();
        HomeViewModel model = new HomeViewModel();
        model.Items = Database.getItemsList() ?? new List<Item>();
        return View("Index", model);
    }
    public IActionResult Update()
    {
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