using System.Net.Http.Json;
using Marathon.mvc.Models.dto;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _http;
    public AccountController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Token")))
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginDTO model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _http.CreateClient("ApiClient");

        var response = await client.PostAsJsonAsync("api/Account/loginUser",
            new { Email = model.Email, Password = model.Password });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (login is null || string.IsNullOrWhiteSpace(login.Token))
        {
            ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
            return View(model);
        }

        HttpContext.Session.SetString("Token", login.Token);
        HttpContext.Session.SetString("Role", login.User?.Role ?? "User");
        HttpContext.Session.SetString("Email", login.User?.Email ?? string.Empty);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public IActionResult SignUp()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Token")))
            return RedirectToAction("Index", "Dashboard");

        return View(new SignUpDTO());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpDTO model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _http.CreateClient("ApiClient");

        // Only send what the API's Add_New_UserDTO expects — ConfirmPassword stays client-side
        var payload = new
        {
            model.Username,
            model.FirstName,
            model.LastName,
            model.Email,
            model.DateOfBirth,
            model.PhoneNumber,
            model.Password
        };

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("api/Account/AddNewUser", payload);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Registration service unavailable. Try again.");
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                ModelState.AddModelError(string.Empty,
                    string.IsNullOrWhiteSpace(error)
                        ? "An account with this email already exists."
                        : error);
            else
                ModelState.AddModelError(string.Empty,
                    string.IsNullOrWhiteSpace(error)
                        ? "Registration failed. Please try again."
                        : error);

            return View(model);
        }

        TempData["SignUpSuccess"] = "Account created successfully. Please sign in.";
        return RedirectToAction(nameof(Index));
    }

}