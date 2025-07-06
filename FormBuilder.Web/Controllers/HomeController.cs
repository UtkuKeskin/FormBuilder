using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FormBuilder.Web.Models;
using FormBuilder.Core.Interfaces;

namespace FormBuilder.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ITagService _tagService;

    public HomeController(
        ILogger<HomeController> logger,
        ITagService tagService)
    {
        _logger = logger;
        _tagService = tagService;
    }

    public async Task<IActionResult> Index()
    {
        //  Tag cloud data add
        var tagCloudData = await _tagService.GetTagCloudDataAsync(15);
        ViewBag.TagCloudData = tagCloudData;
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var model = new ErrorViewModel { RequestId = requestId };

        // Log the error details
        Serilog.Log.Error("Error page visited. RequestId: {RequestId}", requestId);

        return View(model);
    }
    
    [Route("404")]
    public IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }
}