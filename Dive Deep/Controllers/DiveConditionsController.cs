using Dive_Deep.Services;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers;

public sealed class DiveConditionsController(IDiveConditionsService diveConditionsService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new DiveConditionsPageViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DiveConditionsPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await diveConditionsService.GetConditionsAsync(model.Location, cancellationToken);
        model.Report = result.Report;
        model.ErrorMessage = result.ErrorMessage;
        return View(model);
    }
}