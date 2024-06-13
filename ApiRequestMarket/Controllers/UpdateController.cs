using ApiRequestMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiRequestMarket.Controllers;

public class UpdateController : Controller 
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        Item item = await Database.GetItemById(id);
        var model = new UpdateViewModel()
        {
            id = item.id,
            name = item.name,
            price = item.price,
            count = item.count,
            path = item.pathImage,
            description = item.description,
            categoryId = item.categoryId,
            categories = await Database.GetCategories()
        };
        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Update(UpdateViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await Database.UpdateItem(model.id, model.name, model.price,model.count, model.path, model.description,
                    model.categoryId))
            {
                await MainControllerUsers.sendReloadToApi();
                return RedirectToAction("Index","Home");
            }
        }
        else
        {
            return View(model);
        }

        return View(model);
    }
}