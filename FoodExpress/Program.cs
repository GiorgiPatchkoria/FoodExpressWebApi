using FoodExpress.Services.Services.Implementations;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FoodExpress.Core.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using FoodExpress.Core.Interfaces;
using FoodExpress.Infrastructure;
using FoodExpress.Services.Services.Interfaces;
using FoodExpress.Infrastructure.Repositories;
using FoodExpress.Middleware;
using FoodExpress.Validators;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;
using FoodExpress.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FoodExpress.Infrastructure.FileStorage;

namespace FoodExpress
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers and validation responses.
            builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var response = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation failed."
                    };
                    return new BadRequestObjectResult(response);
                };
            });
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenApi();

            // Database and repositories.
            builder.Services.AddDbContext<FoodDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            var webRootPath = builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(webRootPath);
            builder.Services.AddSingleton<IFileStorageService>(new LocalFileStorageService(webRootPath));
            builder.Services.AddScoped<ICatalogImageService, CatalogImageService>();

            // Services.
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRestaurantService, RestaurantService>();
            builder.Services.AddScoped<IMenuItemService, MenuItemService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            // Input validation
            builder.Services.AddScoped<IRequestValidator, RequestValidator>();

            // Swagger.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "FoodExpress API",
                    Version = "v1"
                });
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the accessToken returned by /api/auth/login."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            // AutoMapper.
            builder.Services.AddAutoMapper(cfg => { },
                typeof(Program).Assembly,
                typeof(RestaurantService).Assembly);

            // Identity manages users; the existing RoleId relationship supplies JWT role claims.
            builder.Services.AddDataProtection().SetApplicationName("FoodExpress");
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<FoodDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(30);
            });

            // JWT authentication.
            var jwtKey = builder.Configuration["JwtTokenSettings:SecretKey"]!;
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtTokenSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtTokenSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                await Results.Problem(statusCode: response.StatusCode, title: ReasonPhrases.GetReasonPhrase(response.StatusCode)).ExecuteAsync(context.HttpContext);
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
