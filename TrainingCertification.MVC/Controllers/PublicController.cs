using Microsoft.AspNetCore.Mvc;
using TrainingCertification.MVC.Services;
using TrainingCertification.MVC.ViewModels;

namespace TrainingCertification.MVC.Controllers;

public class PublicController : Controller
{
    private readonly PublicCertificateClient _client;
    public PublicController(PublicCertificateClient client) => _client = client;
    public IActionResult CertificationLookup() => RedirectToAction(nameof(CertificateLookup));
    public IActionResult CertificateLookup() => View(new PublicLookupViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CertificateLookup(PublicLookupViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        model.SearchCompleted = true;
        try
        {
            model.Result = await _client.VerifyAsync(model.TraineeId, model.ReferenceNumber);
            if (model.Result == null) model.Error = "No certification record found for the provided details";
        }
        catch (HttpRequestException)
        {
            model.Error = "Unable to retrieve information at this time. Please try again later.";
        }
        return View(model);
    }
}
