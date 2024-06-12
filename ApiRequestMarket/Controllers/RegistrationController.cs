using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiRequestMarket.Models;
namespace ApiRequestMarket.Controllers;

public class RegistrationController : Controller
{
    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Registration(RegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await Database.IsUserRegistered(model.Email))
        {
            ModelState.AddModelError(string.Empty, "Użytkownik o podanym adresie email już istnieje.");
            return View(model);
            
        }
        else
        {
            User user = new User(model.Email, model.Password);
            user.registerUser();
            return View(model);
        }
    }

   
}