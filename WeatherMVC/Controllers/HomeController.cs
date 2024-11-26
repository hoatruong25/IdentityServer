using System.Diagnostics;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WeatherMVC.Models;
using WeatherMVC.Services;

namespace WeatherMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ITokenService _tokenService;

    public HomeController(ITokenService tokenService,ILogger<HomeController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    [Authorize]
    public async Task<IActionResult> Weather()
    {
        using var client = new HttpClient();

        // var token = await _tokenService.GetToken("CoffeeAPI.read");
        // if (token.AccessToken != null) client.SetBearerToken(token.AccessToken);

        var token = await HttpContext.GetTokenAsync("access_token");
        if (token != null) client.SetBearerToken(token);

        var result = await client.GetAsync("https://localhost:7295/WeatherForecast");

        if (result.IsSuccessStatusCode)
        {
            var model = await result.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

            return View(data);
        }

        throw new Exception("Unalble to get content");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}