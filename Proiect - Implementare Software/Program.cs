using Proiect_Implementare_Software.Data;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Services;
using Proiect_Implementare_Software.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Proiect___Implementare_Software.Repositories;
using Proiect___Implementare_Software.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailRepository, EmailRepository>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurăm Identity pentru a folosi User-ul și rolurile din baza de date
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Adăugăm serviciile personalizate
builder.Services.AddScoped<IRideService, RideService>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  


app.MapGet("/", async context =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Home/Index");
    }
    else
    {
        context.Response.Redirect("/Identity/Account/Login");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
