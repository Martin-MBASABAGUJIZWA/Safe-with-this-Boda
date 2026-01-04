// Top-level statements must be the first thing in the file
using Microsoft.EntityFrameworkCore;
using SafeBoda.Application;
using SafeBoda.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure services
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline
startup.Configure(app, app.Environment);

// Initialize the database
await ProgramInitializer.Initialize(app.Services, app.Environment);

app.Run();

// Program class for testability
public partial class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

// Startup class for configuration
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        // Database
        var connectionString = Configuration.GetConnectionString("SafeBodaDb");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (env == "Development")
        {
            services.AddDbContext<SafeBodaDbContext>(options =>
                options.UseSqlite(connectionString));
        }
        else
        {
            services.AddDbContext<SafeBodaDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        }

        // Controllers and API
        services.AddControllers();
        services.AddMemoryCache();
        services.AddEndpointsApiExplorer();

        // Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SafeBoda API", Version = "v1" });

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "Put your JWT token here (no quotes).",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });
        });

        // Repositories
        services.AddScoped<ITripRepository, EfTripRepository>();
        services.AddScoped<IRiderRepository, EfRiderRepository>();
        services.AddScoped<IDriverRepository, EfDriverRepository>();

        // Identity & Authentication
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SafeBodaDbContext>()
            .AddSignInManager();

        var jwtSection = Configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"];
        var jwtIssuer = jwtSection["Issuer"];
        var jwtAudience = jwtSection["Audience"];

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("DevCors");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

// ProgramInitializer for database seeding
public class ProgramInitializer
{
    public static async Task Initialize(IServiceProvider services, IWebHostEnvironment env)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        try
        {
            var context = serviceProvider.GetRequiredService<SafeBodaDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            // Ensure database is created and apply migrations
            if (env.IsDevelopment())
            {
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                await context.Database.MigrateAsync();
            }

            // Seed roles
            string[] roles = { "Rider", "Driver", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create test users
            var testRiderEmail = "rider@example.com";
            var testDriverEmail = "driver@example.com";
            var password = "Test@123";

            // Create test rider user
            if (await userManager.FindByEmailAsync(testRiderEmail) == null)
            {
                var user = new ApplicationUser
                {
                    Id = "test-rider-user-id",
                    UserName = testRiderEmail,
                    Email = testRiderEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Rider");
                }
            }

            // Create test driver user
            if (await userManager.FindByEmailAsync(testDriverEmail) == null)
            {
                var user = new ApplicationUser
                {
                    Id = "test-driver-user-id",
                    UserName = testDriverEmail,
                    Email = testDriverEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Driver");
                }
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw; // Re-throw to fail the application startup
        }
    }
}