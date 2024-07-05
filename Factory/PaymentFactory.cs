using Bogus;
using Cloud.Models;
using Cloud.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cloud.Factories {
  public class RentPaymentFactory {
	private readonly ApplicationDbContext _dbContext;
	private readonly Faker<RentPaymentModel> _paymentFaker;
	private readonly PaymentValidator _paymentValidator;

	public RentPaymentFactory(ApplicationDbContext dbContext, PaymentValidator paymentValidator) {
	  _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	  _paymentValidator = paymentValidator ?? throw new ArgumentNullException(nameof(paymentValidator));

	  // Initialize Bogus for generating fake rent payment data
	  _paymentFaker = new Faker<RentPaymentModel>()
		  .RuleFor(p => p.TenantId, f => f.Random.Guid())
		  .RuleFor(p => p.Amount, f => f.Finance.Amount(1000, 500000, 2))
		  .RuleFor(p => p.Currency, f => f.Finance.Currency().Code)
		  .RuleFor(p => p.PaymentIntentId, f => f.Random.AlphaNumeric(10))
		  .RuleFor(p => p.PaymentMethodId, f => f.Random.AlphaNumeric(10))
		  .RuleFor(p => p.Status, f => f.PickRandom<PaymentStatus>());
	}

	public async Task<RentPaymentModel> CreateFakePaymentAsync() {
	  if (_dbContext.RentPayments == null) {
		throw new InvalidOperationException("RentPayments DbSet is not initialized.");
	  }

	  var payment = _paymentFaker.Generate();
	  _paymentValidator.ValidatePayment(payment);
	  _dbContext.RentPayments.Add(payment);
	  await _dbContext.SaveChangesAsync();
	  return payment;
	}

	public async Task<RentPaymentModel> CreatePaymentAsync(CreateRentPaymentDto dto) {
	  if (_dbContext.RentPayments == null) {
		throw new InvalidOperationException("RentPayments DbSet is not initialized.");
	  }

	  var payment = new RentPaymentModel {
		TenantId = dto.TenantId,
		Amount = dto.Amount,
		Currency = dto.Currency,
		PaymentIntentId = dto.PaymentIntentId,
		PaymentMethodId = dto.PaymentMethodId,
		Status = dto.Status
	  };

	  _paymentValidator.ValidatePayment(payment);
	  _dbContext.RentPayments.Add(payment);
	  await _dbContext.SaveChangesAsync();
	  return payment;
	}

	public async Task SeedPaymentsAsync(int count) {
	  if (_dbContext.RentPayments == null) {
		throw new InvalidOperationException("RentPayments DbSet is not initialized.");
	  }

	  var payments = new List<RentPaymentModel>(count);

	  for (int i = 0; i < count; i++) {
		var payment = _paymentFaker.Generate();
		_paymentValidator.ValidatePayment(payment);
		payments.Add(payment);
	  }

	  await _dbContext.RentPayments.AddRangeAsync(payments);
	  await _dbContext.SaveChangesAsync();
	}
  }
}
