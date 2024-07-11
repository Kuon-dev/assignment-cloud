using Amazon.S3.Model;
using Bogus;
using Cloud.Models;
using Cloud.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Cloud.Factories
{
	/// <summary>
	/// Factory class for creating listing models with validations.
	/// </summary>
	public class ListingFactory
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly Faker<ListingModel> _listingFaker;
		private readonly ListingValidator _listingValidator;

		/// <summary>
		/// Initializes a new instance of the ListingFactory class.
		/// </summary>
		/// <param name="dbContext">The database context for entity operations.</param>
		/// <param name="listingValidator">The validator for listing models.</param>
		public ListingFactory(ApplicationDbContext dbContext, ListingValidator listingValidator)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_listingValidator = listingValidator ?? throw new ArgumentNullException(nameof(listingValidator));

			// Initialize Bogus for generating fake listing data
			_listingFaker = new Faker<ListingModel>()
				.RuleFor(l => l.Title, f => f.Lorem.Sentence(3, 3))
				.RuleFor(l => l.Description, f => f.Lorem.Paragraph())
				.RuleFor(l => l.Price, f => f.Random.Decimal(500, 5000))
				.RuleFor(l => l.IsActive, f => f.Random.Bool())
				.RuleFor(l => l.StartDate, f => f.Date.Past(1, DateTime.UtcNow).ToUniversalTime())
				.RuleFor(l => l.EndDate, (f, l) => l.StartDate.AddMonths(f.Random.Int(6, 24)).ToUniversalTime())
				.RuleFor(l => l.Views, f => f.Random.Int(0, 1000));
		}

		/// <summary>
		/// Creates a fake listing with random data.
		/// </summary>
		/// <returns>The created ListingModel.</returns>
		public async Task<ListingModel> CreateFakeListingAsync(Guid propertyId)
		{
			if (_dbContext.Listings == null)
			{
				throw new InvalidOperationException("Listing DbSet is not initialized.");
			}

			var listing = _listingFaker.Clone().RuleFor(l => l.PropertyId, _ => propertyId).Generate();
			await ValidateAndSaveListingAsync(listing);
			return listing;
		}

		/// <summary>
		/// Creates a listing with specified details.
		/// </summary>
		/// <param name="createListingDto">The DTO containing listing details.</param>
		/// <param name="userId">The ID of the user creating the listing.</param>
		/// <returns>The created ListingModel.</returns>
		public async Task<ListingModel> CreateListingAsync(CreateListingDto createListingDto, string userId)
		{
			if (_dbContext.Listings == null || _dbContext.Properties == null)
			{
				throw new InvalidOperationException("DbSet is not initialized.");
			}

			var property = await _dbContext.Properties.FindAsync(createListingDto.PropertyId);
			if (property == null || property.OwnerId.ToString() != userId)
			{
				throw new InvalidOperationException("Invalid property or user does not own the property.");
			}

			var listing = new ListingModel
			{
				Title = createListingDto.Title,
				Description = createListingDto.Description,
				Price = createListingDto.Price,
				PropertyId = createListingDto.PropertyId,
				IsActive = createListingDto.IsActive,
				Views = 0,
				StartDate = createListingDto.StartDate,
				EndDate = createListingDto.EndDate,
			};

			listing.UpdateCreationProperties(DateTime.UtcNow);
			listing.UpdateModifiedProperties(DateTime.UtcNow);

			await ValidateAndSaveListingAsync(listing);
			return listing;
		}

		/// <summary>
		/// Seeds the database with a specified number of fake listings.
		/// </summary>
		/// <param name="count">The number of listings to create.</param>
		public async Task SeedListingsAsync(int count)
		{
			if (_dbContext.Listings == null || _dbContext.Properties == null)
			{
				throw new InvalidOperationException("DbSet is not initialized.");
			}

			var propertyIds = await _dbContext.Properties.Select(p => p.Id).ToListAsync();

			if (propertyIds.Count == 0)
			{
				throw new InvalidOperationException("No properties found to associate with listings.");
			}

			var listings = new List<ListingModel>(count);
			var random = new Random();

			for (int i = 0; i < count; i++)
			{
				var propertyId = propertyIds[random.Next(propertyIds.Count)];
				var listing = _listingFaker.Clone().RuleFor(l => l.PropertyId, _ => propertyId).Generate();
				listing.UpdateCreationProperties(DateTime.UtcNow);
				listing.UpdateModifiedProperties(DateTime.UtcNow);
				_listingValidator.ValidateListing(listing);
				listings.Add(listing);
			}

			await _dbContext.Listings.AddRangeAsync(listings);
			await _dbContext.SaveChangesAsync();
		}

		private async Task ValidateAndSaveListingAsync(ListingModel listing)
		{
			_listingValidator.ValidateListing(listing);
			_dbContext.Listings.Add(listing);
			await _dbContext.SaveChangesAsync();
		}
	}
}