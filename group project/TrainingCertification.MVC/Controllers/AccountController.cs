using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrainingCertification.API.Models;
using TrainingCertification.API.Services;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    public AccountController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users) { _signIn = signIn; _users = users; }

    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
            var user = await _users.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _users.IsInRoleAsync(user, Roles.TrainingCoordinator)) return RedirectToAction("Dashboard", "Coordinator");
                if (await _users.IsInRoleAsync(user, Roles.Instructor)) return RedirectToAction("Dashboard", "Instructor");
                if (await _users.IsInRoleAsync(user, Roles.Trainee)) return RedirectToAction("Dashboard", "Trainee");
            }
            return RedirectToAction("Index", "Dashboard");
        }
        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            TraineeNumber = await TraineeNumberGenerator.GenerateUniqueAsync(_users)
        };
        var result = await _users.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, Roles.Trainee);
            await _signIn.SignInAsync(user, false);
            return RedirectToAction("Dashboard", "Trainee");
        }
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction("Index", "Home"); }

    public IActionResult AccessDenied() => View();
}
