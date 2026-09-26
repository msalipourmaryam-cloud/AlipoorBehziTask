using Microsoft.AspNetCore.Mvc;

namespace AlipoorBehTask.Api.Controllers;

public sealed class DashboardController : Controller
{
    public IActionResult Index() => View();
}