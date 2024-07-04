// Program.cs
using DotNetEnv;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework Core with PostgreSQL
var appEnv = Environment.GetEnvironmentVariable("APP_ENV");
var connectionString = "";

if (string.Equals(appEnv, "development", StringComparison.OrdinalIgnoreCase)) {
  connectionString = Environment.GetEnvironmentVariable("DATABASE_LOCAL_URL");
  if (string.IsNullOrEmpty(connectionString)) {
	throw new InvalidOperationException("Database connection string 'DATABASE_LOCAL_URL' not found.");
  }
}
else {
  connectionString = Environment.GetEnvironmentVariable("DATABASE_REMOTE_NEON");
  if (string.IsNullOrEmpty(connectionString)) {
	throw new InvalidOperationException("Database connection string 'DATABASE_REMOTE_NEON' not found.");
  }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString));

// Add JWT configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience)) {
  throw new InvalidOperationException("JWT configuration not found.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options => {
	  options.TokenValidationParameters = new TokenValidationParameters {
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtIssuer,
		ValidAudience = jwtAudience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
	  };
	});

// Register EmailService


builder.Services.AddIdentity<UserModel, IdentityRole>(options => {
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

// add logger
builder.Services.AddLogging();

builder.Services.AddSingleton<S3Service>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<ApiExceptionFilter>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IRentalApplicationService, RentalApplicationService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILeaseService, LeaseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMaintenanceTaskService, MaintenanceTaskService>();
builder.Services.AddScoped<IOwnerPaymentService, OwnerPaymentService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

builder.Services.AddScoped<PaymentIntentService>();

StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? throw new InvalidOperationException("Stripe secret key not found.");

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment()) {*/
app.UseSwagger();
app.UseSwaggerUI();
/*}*/

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();