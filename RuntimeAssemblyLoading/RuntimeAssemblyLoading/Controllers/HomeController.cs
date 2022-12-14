using Microsoft.AspNetCore.Mvc;

namespace RuntimeAssemblyLoading.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : Controller
{
    [HttpGet(Name = "GetList")]
    public IEnumerable<string> Get()
    {
        return Enumerable.Range(1, 5).Select(index => index.ToString())
        .ToArray();
    }
}