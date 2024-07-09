using DotNetEnv;
using Cloud.Models;
using Cloud.Models.DTO;
using Cloud.Services;
using Cloud.Filters;
using Cloud.Middlewares;
using Cloud.Factories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Stripe;
using Cloud.Models.Validator;
using Microsoft.AspNetCore.HttpOverrides;
/*using System.Text.Json;*/
/*using System.Text.Json.Serialization;*/
/*using Microsoft.Extensions.Logging;*/

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

// Add services to the container.
builder.Services.AddControllers();
/*	.AddJsonOptions(options =>*/
/*{*/
/*	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;*/
/*	options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;*/
/*});*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework Core with PostgreSQL
var appEnv = Environment.GetEnvironmentVariable("APP_ENV");
var connectionString = "";

if (string.Equals(appEnv, "development", StringComparison.OrdinalIgnoreCase))
{
	connectionString = Environment.GetEnvironmentVariable("DATABASE_LOCAL_URL");
	if (string.IsNullOrEmpty(connectionString))
	{
		throw new InvalidOperationException("Database connection string 'DATABASE_LOCAL_URL' not found.");
	}
}
else
{
	connectionString = Environment.GetEnvironmentVariable("DATABASE_REMOTE_NEON");
	if (string.IsNullOrEmpty(connectionString))
	{
		throw new InvalidOperationException("Database connection string 'DATABASE_REMOTE_NEON' not found.");
	}
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(connectionString));

// Add JWT configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
	throw new InvalidOperationException("JWT configuration not found.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
		  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
	  };
  });

builder.Services.AddIdentity<UserModel, IdentityRole>(options =>
{
	// Configure identity options here if needed
	options.SignIn.RequireConfirmedAccount = false;
	options.SignIn.RequireConfirmedPhoneNumber = false;

	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend",
		builder => builder
			.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials());
});

// Add logging
builder.Services.AddLogging();

/*builder.Services.AddSingleton<S3Service>();*/
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<ApiExceptionFilter>();
builder.Services.AddScoped<PaymentIntentService>();

builder.Services.AddScoped<PropertyFactory>();
builder.Services.AddScoped<LeaseFactory>();
builder.Services.AddScoped<UserFactory>();
builder.Services.AddScoped<ListingFactory>();
builder.Services.AddScoped<RentPaymentFactory>();
builder.Services.AddScoped<RentalApplicationFactory>();
builder.Services.AddScoped<MaintenanceFactory>();
builder.Services.AddScoped<OwnerPaymentFactory>();

builder.Services.AddScoped<ListingValidator>();
builder.Services.AddScoped<LeaseValidator>();
builder.Services.AddScoped<RentPaymentValidator>();
builder.Services.AddScoped<RentalApplicationValidator>();
builder.Services.AddScoped<MaintenanceTaskValidator>();
builder.Services.AddScoped<MaintenanceRequestValidator>();
builder.Services.AddScoped<OwnerPaymentValidator>();
builder.Services.AddScoped<StripeCustomerValidator>();
builder.Services.AddScoped<CreateMediaDtoValidator>();
builder.Services.AddScoped<UserValidator>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IRentalApplicationService, RentalApplicationService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOwnerPaymentService, OwnerPaymentService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IStripeCustomerService, StripeCustomerService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IAdminService, AdminService>();

StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? throw new InvalidOperationException("Stripe secret key not found.");

// Add DataSeeder service
builder.Services.AddTransient<DataSeeder>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	using (var scope = app.Services.CreateScope())
	{
		var services = scope.ServiceProvider;
		try
		{
			var seeder = services.GetRequiredService<DataSeeder>();
			await seeder.SeedAsync();
		}
		catch (Exception ex)
		{
			var logger = services.GetRequiredService<ILogger<Program>>();
			logger.LogError(ex, "An error occurred seeding the DB.");
		}
	}
}

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment()) {*/
app.UseUserStatusMiddleware();
app.UseSwagger();
app.UseSwaggerUI();
/*}*/

app.UseRouting();
app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();