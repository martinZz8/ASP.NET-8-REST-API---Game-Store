using Microsoft.EntityFrameworkCore;
using GameStore.Web.DbContexts;
using GameStore.Web.Helpers.AppsettingsLoader;
using GameStore.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace GameStore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Run custom loader "AppsettingsLoader" of all required "appsettings.json" variables (they will be available throught properties of static class "AppsettingsLoader")
            AppsettingsLoader.LoadAllEnvVariables();

            var builder = WebApplication.CreateBuilder(args);

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

            // Add services to the container.
            builder.Services.AddControllers();
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

            // Create host based on connection string taken from "appsettings.json" config file
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
            );
            // ... and register services to be injected into controllers
            builder.Services.AddTransient<IGameSerive, GameService>(); //it => new GameService(it.GetService<ApplicationDbContext>())
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IGameUserCopyService, GameUserCopyService>();
            builder.Services.AddTransient<IGameGenreService, GameGenreService>();
            builder.Services.AddTransient<IUserRoleService, UserRoleService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();            
            app.MapControllers();

            // Add static files that are placed in folder "wwwroot"
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }
    }
}
