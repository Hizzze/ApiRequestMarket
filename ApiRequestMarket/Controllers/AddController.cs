using ApiRequestMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiRequestMarket.Controllers;

public class AddController : Controller
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var model = new AddViewModel()
        {
            categories = await Database.GetCategories()
        };
        return View(model);
    }
}