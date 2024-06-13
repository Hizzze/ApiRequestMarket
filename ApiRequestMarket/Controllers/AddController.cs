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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(AddViewModel model)
    {
        if (ModelState.IsValid)
        {
            string path = "../images/" + model.path;
            if (await Database.AddNewItem(model.name, model.price, model.count, path, model.description,
                    model.categoryId))
            {
                await MainControllerUsers.sendReloadToApi();
            }
        }
        return RedirectToAction("Add", "Add");
    }
}