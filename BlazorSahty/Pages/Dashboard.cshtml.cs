using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models;

namespace Sahty.Web.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.IsInRole(AppRoles.Admin)) return RedirectToPage("/Index", new { area = "Admin" });
        if (User.IsInRole(AppRoles.Doctor)) return RedirectToPage("/Index", new { area = "Doctor" });
        if (User.IsInRole(AppRoles.Pharmacy)) return RedirectToPage("/Index", new { area = "Pharmacy" });
        if (User.IsInRole(AppRoles.Client)) return RedirectToPage("/Index", new { area = "Client" });

        return RedirectToPage("/Index");
    }
}
