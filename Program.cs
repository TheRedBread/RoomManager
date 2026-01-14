using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomManagerApp.Data;
using RoomManagerApp.Models;
using RoomManagerApp.Services;


var builder = WebApplication.CreateBuilder(args);

// Context
builder.Services.AddDbContext<RoomManagerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RoomManagerDb"))
);




// Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<RoomManagerDbContext>()
    .AddDefaultTokenProviders();



// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen();


var app = builder.Build();

await SeedService.SeedDatabase(app.Services);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}


app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
