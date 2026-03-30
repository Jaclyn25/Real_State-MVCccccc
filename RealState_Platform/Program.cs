namespace RealState_Platform
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Add Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Disable email confirmation for testing
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Configure cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            var app = builder.Build();

            // Seed functions
            async Task SeedRolesAsync(IServiceProvider serviceProvider)
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Admin", "Agent", "Customer" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            async Task SeedAdminAsync(IServiceProvider serviceProvider)
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminEmail = "jacquelineSaad@gmail.com";
                var adminPassword = "Admin@123";

                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "Jacqueline",
                        LastName = "Saad",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
                else
                {
                    // Ensure password is correct for the existing user
                    var passwordValid = await userManager.CheckPasswordAsync(existingAdmin, adminPassword);
                    if (!passwordValid)
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                        await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);
                    }
                }
            }

            // Configure pipeline
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

            // Seed data logic
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    if (await context.Database.CanConnectAsync())
                    {
                        await context.Database.EnsureCreatedAsync();
                        await SeedRolesAsync(services);
                        await SeedAdminAsync(services);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Seeding error: {ex.Message}");
                }
            }

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<InquiryChatHub>("/inquiryChatHub");
            app.MapHub<CommentHub>("/commentHub");

            await app.RunAsync();
        }
    }
}