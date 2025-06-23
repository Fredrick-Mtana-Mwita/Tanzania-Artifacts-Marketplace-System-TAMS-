using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Hubs;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Implementations;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.SignalR;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity using AddDefaultIdentity to ensure all required services (including SignInManager) are registered
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{     // Require email confirmation
    options.SignIn.RequireConfirmedAccount = true; 
})
// ✅ Support for roles like Admin, Seller, etc
.AddRoles<IdentityRole>() 
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Enable MVC support
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
// Then register IUserIdProvider as singleton
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();


// Register Repository with DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository,CartRepository>();   
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationSender, NotificationSender>();




// Register UnitOfWork with DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<NotificationHub>("/notificationHub");

//  Seed default roles and admin user at app startup

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedDefaultData(services);
}

app.Run();
