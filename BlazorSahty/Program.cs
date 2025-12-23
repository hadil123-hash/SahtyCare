using Microsoft.AspNetCore.Authentication.Cookies;
using Sahty.Web.Models;
using Sahty.Web.Services.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.Admin));
    options.AddPolicy("DoctorOnly", p => p.RequireRole(AppRoles.Doctor));
    options.AddPolicy("PharmacyOnly", p => p.RequireRole(AppRoles.Pharmacy));
    options.AddPolicy("ClientOnly", p => p.RequireRole(AppRoles.Client));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Dashboard");

    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Doctor", "/", "DoctorOnly");
    options.Conventions.AuthorizeAreaFolder("Pharmacy", "/", "PharmacyOnly");
    options.Conventions.AuthorizeAreaFolder("Client", "/", "ClientOnly");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiTokenStore, HttpContextTokenStore>();
builder.Services.AddHttpClient<RestClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Missing configuration: Api:BaseUrl");
    client.BaseAddress = new Uri(baseUrl);
})
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        if (builder.Environment.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        return handler;
    });
builder.Services.AddScoped<AuthClient>();
builder.Services.AddScoped<AdminClient>();
builder.Services.AddScoped<AppointmentsClient>();
builder.Services.AddScoped<DoctorsClient>();
builder.Services.AddScoped<PatientsClient>();
builder.Services.AddScoped<PharmacistsClient>();
builder.Services.AddScoped<MedicationsClient>();
builder.Services.AddScoped<PrescriptionsClient>();
builder.Services.AddScoped<NotificationsClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
