 // Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features; // ✅ Needed for request size config
using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Hubs;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Implementations;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.SignalR;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES CONFIGURATION ====================

// ✅ Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ✅ ASP.NET Identity setup (with roles, email confirmation)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Require email confirmation
})
.AddRoles<IdentityRole>() // Enable role support
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// ✅ MVC and SignalR
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // ✅ Register IHttpClientFactory
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// ✅ DI: Application Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationSender, NotificationSender>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.Configure<PayPalSettings>(
    builder.Configuration.GetSection("PayPal"));
builder.Services.AddScoped<PayPalService>();

// ✅ Email settings configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ✅ Configure max request body size (important for image upload)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// ==================== BUILD APP ====================
var app = builder.Build();

// ==================== MIDDLEWARE ====================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage(); // ✅ Enables detailed error page
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Required for Identity
app.UseAuthorization();

// ==================== ROUTING ====================

// ✅ Area support route — this must come BEFORE 
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// ✅ Default route for non-area controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ✅ Razor Pages (e.g., Identity UI)
app.MapRazorPages()
   .WithStaticAssets();

// ✅ SignalR notification hub
app.MapHub<NotificationHub>("/notificationHub");

// ✅ Seed Roles and Admin at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedDefaultData(services);
}

// ✅ Run the app
app.Run();
