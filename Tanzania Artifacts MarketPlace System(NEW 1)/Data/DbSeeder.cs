using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Constants;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userMgr = service.GetService<UserManager<ApplicationUser>>();
            var roleMgr = service.GetService<RoleManager<IdentityRole>>();
            var dbContext = service.GetService<ApplicationDbContext>()
                ?? throw new Exception("ApplicationDbContext is not registered in the DI container.");

            if (userMgr == null || roleMgr == null)
                throw new Exception("UserManager or RoleManager is not registered in the DI container.");

            // 1. Define Roles
            var roles = new[] { Roles.Admin.ToString(), Roles.User.ToString(), Roles.Seller.ToString() };

            foreach (var role in roles)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                {
                    var roleResult = await roleMgr.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {role}. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // 2. Create Users
            await CreateUserIfNotExists(userMgr, "admin@gmail.com", "Admin@123", "System Administrator", Roles.Admin.ToString());
            await CreateUserIfNotExists(userMgr, "seller@gmail.com", "Seller@123", "Artifact Seller", Roles.Seller.ToString());
            await CreateUserIfNotExists(userMgr, "user@gmail.com", "User@123", "Regular Buyer", Roles.User.ToString());

            // 3. Get the seller user
            var sellerUser = await userMgr.FindByEmailAsync("seller@gmail.com");
            if (sellerUser == null)
                throw new Exception("Default seller user not found.");

            // 4. Seed Products
            if (!await dbContext.Products.AnyAsync())
            {
                dbContext.Products.AddRange(
                    new Product
                    {
                        Name = "Maasai Tribal Art",
                        Description = "Massive collection of Maasai Mahogany carved wood sculptures.",
                        ProductHistory = "Lorem ipsum dolor sit amet, consectetur adipiscing elit," +
                        " sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
                        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        Price = 20000,
                        ProductImage = "Tribal Art.jpg", // Make sure this image exists in wwwroot/uploads
                        InStock = true,
                        StockQuantity = 10,
                        SellerId = sellerUser.Id
                    },
                    new Product
                    {
                        Name = "Makonde Male",
                        Description = "The picture that depicts the male sex of Makonde tribe.",
                        ProductHistory = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                        "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                        " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. " +
                        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        Price = 100000,
                        ProductImage = "MAKONDE .jpg",
                        InStock = true,
                        StockQuantity = 5,
                        SellerId = sellerUser.Id
                    },
                    new Product
                    {
                        Name = "Makonde Tree of Life",
                        Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                        "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, " +
                        "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                        " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
                        " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum",
                        ProductHistory = "Hand-carved from ebony wood by local artists.",
                        Price = 500000,
                        ProductImage = "Makonde-tree-of-life.jpg",
                        InStock = true,
                        StockQuantity = 3,
                        SellerId = sellerUser.Id
                    }
                );

                await dbContext.SaveChangesAsync();
            }
        }

        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userMgr, string email, string password, string firstName, string role)
        {
            var existingUser = await userMgr.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    EmailConfirmed = true,
                    ProfileImage = "default-profile.png",
                    Role = Enum.Parse<Roles>(role) // 👈 this is the fix
                };

                var createResult = await userMgr.CreateAsync(newUser, password);
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to create user {email}. Errors: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var addToRoleResult = await userMgr.AddToRoleAsync(newUser, role);
                if (!addToRoleResult.Succeeded)
                {
                    throw new Exception($"Failed to assign role {role} to {email}. Errors: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
