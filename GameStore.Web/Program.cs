using Microsoft.EntityFrameworkCore;
using GameStore.Web.DbContexts;
using GameStore.Web.Helpers.AppsettingsLoader;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using GameStore.Web.Helpers;

namespace GameStore.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Run custom loader "AppsettingsLoader" of all required "appsettings.<env>.json" variables (they will be available throught properties of static class "AppsettingsLoader")
            AppsettingsLoader.LoadAllEnvVariables(builder.Environment.EnvironmentName);

            // Add authorization and authentication based on JWT
            // from: https://www.telerik.com/blogs/asp-net-core-basics-authentication-authorization-jwt
            builder.Services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppsettingsLoader.JWTSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false
                };
            });
            builder.Services.AddAuthorization();

            // Add services to the container
            // Add controllers (together with xml formatter, from: https://stackoverflow.com/a/77560928/17168242)
            builder.Services.AddControllers().AddXmlSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger to use authentication and authorization
            // from: https://www.telerik.com/blogs/asp-net-core-basics-authentication-authorization-jwt
            builder.Services.AddSwaggerGen(c =>
            {
                var securityScheme = new OpenApiSecurityScheme()
                {
                    Name = "JWT Authentication",
                    Description = "Enter your JWT token in this field",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement()
                {
                    // Note: Code below runs "securityRequirement.Add()" with two given arguments (key and value)
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                };

                c.AddSecurityRequirement(securityRequirement);
            });

            // Add CORS for all origins
            const string corsPolicyName = "CorsPolicy";
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy(corsPolicyName, policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Create host based on connection string taken from "appsettings.json" config file (this is also scoped service)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
            );
            // ... and register services to be injected into controllers
            // Note: There are multiple of services lifetimes:
            // - builder.Services.AddTransient<T>: Transient lifetime services are created each time they are requested from the service container (IServiceProvider).
            // - builder.Services.AddScoped<T>: Scoped lifetime services are created once per HTTP request and reused within that request.
            // - builder.Services.AddSingleton<T>: Singleton lifetime services are created the first time they are requested and reused across the application lifetime.
            builder.Services.AddTransient<IGameService, GameService>(); //it => new GameService(it.GetRequiredService<ApplicationDbContext>())
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IGameUserCopyService, GameUserCopyService>();
            builder.Services.AddTransient<IGameGenreService, GameGenreService>();
            builder.Services.AddTransient<IUserRoleService, UserRoleService>();
            builder.Services.AddTransient<IGameFileDescriptionService, GameFileDescriptionService>();
            builder.Services.AddTransient<IXmlProcessorService, XmlProcessorService>();

            var app = builder.Build();

            // Note: We can acquire registered services using one of the following methods (user better "GetRequiredService" over "GerService", because first method throws an exception when service is not found, and the second in that case returns null)
            // based on: https://andrewlock.net/the-difference-between-getservice-and-getrquiredservice-in-asp-net-core/
            //var dbContext = app.Services.GetService<ApplicationDbContext>();
            //var dbContext2 = app.Services.GetRequiredService<ApplicationDbContext>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();            
            app.MapControllers();
            app.UseCors(corsPolicyName);

            // Add static files that are placed in folder "wwwroot"
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            // Note: If we want to execute migrations on startup, change the "ApplyMigrationsAtStart" in "appsettings.json" to true
            if (bool.Parse(app.Configuration.GetSection("ApplyMigrationsAtStart").Value))
                await app.MigrateToNewestDbAsync();

            // Note: If we want to execute data insertions on startup, change the "ApplyDataInsertionsAtStart" in "appsettings.json" to true
            if (bool.Parse(app.Configuration.GetSection("ApplyDataInsertionsAtStart").Value))
                await app.ApplyDataInsertionsAsync();

            app.Run();
        }
    }
}
