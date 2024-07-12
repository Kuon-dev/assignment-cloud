using Microsoft.EntityFrameworkCore;
using Stripe;
using Cloud.Models;
using Cloud.Models.DTO;

namespace Cloud.Services
{
	public interface IStripeService
	{
		Task<StripeCustomerModel> CreateConnectedAccountAsync(string userId);
		Task<string> CreateOnboardingLinkAsync(string stripeCustomerId);
		Task<StripeAccountStatusDto> GetAccountStatusAsync(string stripeCustomerId);
		Task<bool> IsAccountFullyOnboardedAsync(string stripeCustomerId);
	}

	public class StripeService : IStripeService
	{
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;

		public StripeService(IConfiguration configuration, ApplicationDbContext context)
		{
			_configuration = configuration;
			_context = context;
			StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
		}

		public async Task<StripeCustomerModel> CreateConnectedAccountAsync(string userId)
		{
			var user = await _context.Users.FindAsync(userId)
				?? throw new InvalidOperationException("User not found");

			var options = new AccountCreateOptions
			{
				Type = "express",
				Country = "US",
				Email = user.Email,
				Capabilities = new AccountCapabilitiesOptions
				{
					CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
					Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
				},
				BusinessType = "individual",
				BusinessProfile = new AccountBusinessProfileOptions
				{
					ProductDescription = "Property rental services"
				},
				Metadata = new Dictionary<string, string>
				{
					{ "UserId", userId }
				}
			};

			var service = new AccountService();
			var account = await service.CreateAsync(options);

			var stripeCustomer = new StripeCustomerModel
			{
				UserId = userId,
				StripeCustomerId = (account.Id),
				IsVerified = false
			};

			_context.StripeCustomers.Add(stripeCustomer);
			await _context.SaveChangesAsync();

			return stripeCustomer;
		}

		public async Task<string> CreateOnboardingLinkAsync(string stripeCustomerId)
		{
			var options = new AccountLinkCreateOptions
			{
				Account = stripeCustomerId,
				RefreshUrl = _configuration["Stripe:OnboardingRefreshUrl"],
				ReturnUrl = _configuration["Stripe:OnboardingReturnUrl"],
				Type = "account_onboarding"
			};

			var service = new AccountLinkService();
			var accountLink = await service.CreateAsync(options);

			return accountLink.Url;
		}

		public async Task<StripeAccountStatusDto> GetAccountStatusAsync(string stripeCustomerId)
		{
			var service = new AccountService();
			var account = await service.GetAsync(stripeCustomerId);

			return new StripeAccountStatusDto
			{
				DetailsSubmitted = account.DetailsSubmitted,
				PayoutsEnabled = account.PayoutsEnabled,
				RequiredVerification = GetRequiredVerification(account)
			};
		}

		public async Task<bool> IsAccountFullyOnboardedAsync(string stripeCustomerId)
		{
			var service = new AccountService();
			var account = await service.GetAsync(stripeCustomerId);

			var isFullyOnboarded = account.DetailsSubmitted &&
								   account.PayoutsEnabled &&
								   account.Capabilities.CardPayments == "active" &&
								   account.Capabilities.Transfers == "active";

			if (isFullyOnboarded)
			{
				var stripeCustomer = await _context.StripeCustomers
					.FirstOrDefaultAsync(sc => sc.StripeCustomerId.ToString() == stripeCustomerId);
				if (stripeCustomer != null)
				{
					stripeCustomer.IsVerified = true;
					await _context.SaveChangesAsync();
				}
			}

			return isFullyOnboarded;
		}

		private List<string> GetRequiredVerification(Account account)
		{
			var requiredVerification = new List<string>();

			if (!account.DetailsSubmitted)
			{
				requiredVerification.Add("Complete account details");
			}

			if (!account.PayoutsEnabled)
			{
				requiredVerification.Add("Set up payout method");
			}

			if (account.Capabilities.CardPayments != "active")
			{
				requiredVerification.Add("Enable card payments");
			}

			if (account.Capabilities.Transfers != "active")
			{
				requiredVerification.Add("Enable transfers");
			}

			return requiredVerification;
		}
	}
}