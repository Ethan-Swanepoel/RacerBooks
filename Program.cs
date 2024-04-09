using Google.Apis.Auth.OAuth2;
using System.IO;
using Microsoft.EntityFrameworkCore;
using EBBooks.Services;
using RacerBooks.Interfaces;
using RacerBooks.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Add database context
builder.Services.AddDbContext<RacerbooksContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the singleton logger service
var logFilePath = builder.Configuration.GetValue<string>("Logging:LogFilePath");
builder.Services.AddSingleton<IUnsuccessfulLoginLogger>(new UnsuccessfulLoginLogger(logFilePath));

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; });


var app = builder.Build();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<RacerbooksContext>();
    dbContext.Database.Migrate(); // Apply migrations
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();