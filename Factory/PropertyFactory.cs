using Cloud.Models;
using Bogus;

public class PropertyFactory
{
	private readonly ApplicationDbContext _context;
	private readonly Faker<PropertyModel> _propertyFaker;
	private readonly Randomizer _randomizer;

	public PropertyFactory(ApplicationDbContext context)
	{
		_context = context;

		// Initialize Bogus for generating fake property data
		_propertyFaker = new Faker<PropertyModel>()
		  .RuleFor(p => p.Id, f => Guid.NewGuid())
		  .RuleFor(p => p.OwnerId, f => Guid.NewGuid()) // Assume you'll replace this with an actual owner ID from your database
		  .RuleFor(p => p.Address, f => f.Address.StreetAddress())
		  .RuleFor(p => p.City, f => f.Address.City())
		  .RuleFor(p => p.State, f => f.Address.StateAbbr())
		  .RuleFor(p => p.ZipCode, f => f.Address.ZipCode())
		  .RuleFor(p => p.PropertyType, f => f.PickRandom<PropertyType>())
		  .RuleFor(p => p.Bedrooms, f => f.Random.Int(1, 5))
		  .RuleFor(p => p.Bathrooms, f => f.Random.Int(1, 4))
		  .RuleFor(p => p.RentAmount, f => f.Finance.Amount(500, 5000))
		  .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
		  .RuleFor(p => p.Amenities, f => f.Make(3, () => f.Commerce.Product()))
		  .RuleFor(p => p.IsAvailable, f => f.Random.Bool())
		  .RuleFor(p => p.RoomType, f => f.PickRandom<RoomType>());

		_randomizer = new Randomizer();
	}

	// Method to create fake property data for seeding
	public async Task<PropertyModel> CreateFakePropertyAsync(Guid ownerId)
	{
		var property = _propertyFaker.Generate();
		property.UpdateCreationProperties(DateTime.UtcNow);
		property.UpdateModifiedProperties(DateTime.UtcNow);
		property.OwnerId = ownerId; // Set the actual owner ID

		await _context.Properties.AddAsync(property);
		await _context.SaveChangesAsync();

		return property;
	}

	// Method to create actual property data from user input
	public async Task<PropertyModel> CreatePropertyAsync(Guid ownerId, string address, string city, string state, string zipCode, PropertyType propertyType, int bedrooms, int bathrooms, decimal rentAmount, string? description, List<string>? amenities, bool isAvailable, RoomType roomType)
	{
		var property = new PropertyModel
		{
			OwnerId = ownerId,
			Address = address,
			City = city,
			State = state,
			ZipCode = zipCode,
			PropertyType = propertyType,
			Bedrooms = bedrooms,
			Bathrooms = bathrooms,
			RentAmount = rentAmount,
			Description = description,
			Amenities = amenities,
			IsAvailable = isAvailable,
			RoomType = roomType
		};

		property.UpdateCreationProperties(DateTime.UtcNow);

		await _context.Properties.AddAsync(property);
		await _context.SaveChangesAsync();

		return property;
	}
}