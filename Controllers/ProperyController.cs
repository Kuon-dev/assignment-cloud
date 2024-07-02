using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Cloud.Models;
using Cloud.Services;
using Cloud.Filters;
using System.ComponentModel.DataAnnotations;

namespace Cloud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class PropertiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPropertyService _propertyService;

        public PropertiesController(ApplicationDbContext context, IPropertyService propertyService)
        {
            _context = context;
            _propertyService = propertyService;
        }

        /// <summary>
        /// Get all properties with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Allows unauthenticated access
        public async Task<ActionResult<IEnumerable<PropertyModel>>> GetProperties([FromQuery] PaginationParams paginationParams)
        {
            var properties = await _context.Properties
                .Skip((paginationParams.Page - 1) * paginationParams.Size)
                .Take(paginationParams.Size)
                .ToListAsync();

            return Ok(properties);
        }

        /// <summary>
        /// Get a specific property by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PropertyModel>> GetProperty(Guid id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            return property;
        }

        /// <summary>
        /// Create a new property
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,PropertyManager")] // Only allows Admin or PropertyManager roles
        [ServiceFilter(typeof(ValidationFilter))] // Custom filter for model validation
        public async Task<ActionResult<PropertyModel>> CreateProperty(CreatePropertyModel model)
        {
            var property = new PropertyModel
            {
                OwnerId = model.OwnerId,
                Address = model.Address,
                City = model.City,
                State = model.State,
                ZipCode = model.ZipCode,
                PropertyType = model.PropertyType,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                SquareFootage = model.SquareFootage,
                RentAmount = model.RentAmount,
                Description = model.Description,
                Amenities = model.Amenities,
                IsAvailable = model.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProperty), new { id = property.Id }, property);
        }

        /// <summary>
        /// Update an existing property
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,PropertyManager")]
        [ServiceFilter(typeof(ValidationFilter))]
        public async Task<IActionResult> UpdateProperty(Guid id, UpdatePropertyModel model)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            // Update properties
            property.Address = model.Address ?? property.Address;
            property.City = model.City ?? property.City;
            property.State = model.State ?? property.State;
            property.ZipCode = model.ZipCode ?? property.ZipCode;
            property.PropertyType = model.PropertyType ?? property.PropertyType;
            property.Bedrooms = model.Bedrooms ?? property.Bedrooms;
            property.Bathrooms = model.Bathrooms ?? property.Bathrooms;
            property.SquareFootage = model.SquareFootage ?? property.SquareFootage;
            property.RentAmount = model.RentAmount ?? property.RentAmount;
            property.Description = model.Description ?? property.Description;
            property.Amenities = model.Amenities ?? property.Amenities;
            property.IsAvailable = model.IsAvailable ?? property.IsAvailable;
            property.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PropertyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Soft delete a property
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            property.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Search for properties with filters
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PropertyModel>>> SearchProperties([FromQuery] SearchPropertyParams searchParams)
        {
            var searchResults = await _propertyService.SearchPropertiesAsync(
                searchParams.Location,
                searchParams.PriceRange,
                searchParams.Bedrooms,
                searchParams.Amenities);
            return Ok(searchResults);
        }

/// <summary>
/// Get all tenants currently renting a specific property with pagination
/// </summary>
[HttpGet("{id}/tenants")]
[Authorize(Roles = "Admin,PropertyManager")]
public async Task<ActionResult<IEnumerable<TenantModel>>> GetPropertyTenants(Guid id, [FromQuery] PaginationParams paginationParams)
{
    var property = await _context.Properties.FindAsync(id);
    if (property == null)
    {
        return NotFound("Property not found");
    }

    var tenants = await _propertyService.GetPropertyTenantsAsync(id, paginationParams.Page, paginationParams.Size);
    return Ok(tenants);
}

        /// <summary>
        /// Get all units for a specific property
        /// </summary>
        [HttpGet("{id}/units")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UnitModel>>> GetPropertyUnits(Guid id)
        {
            var units = await _context.Units
                .Where(u => u.PropertyId == id)
                .ToListAsync();

            return Ok(units);
        }

        /// <summary>
        /// Get all listings for a specific property
        /// </summary>
        [HttpGet("{id}/listings")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ListingModel>>> GetPropertyListings(Guid id)
        {
            var listings = await _context.Listings
                .Where(l => l.PropertyId == id)
                .ToListAsync();

            return Ok(listings);
        }

        /// <summary>
        /// Get all maintenance requests for a specific property
        /// </summary>
        [HttpGet("{id}/maintenance-requests")]
        [Authorize(Roles = "Admin,PropertyManager,Tenant")]
        public async Task<ActionResult<IEnumerable<MaintenanceRequestModel>>> GetPropertyMaintenanceRequests(Guid id)
        {
            var maintenanceRequests = await _context.MaintenanceRequests
                .Where(m => m.PropertyId == id)
                .ToListAsync();

            return Ok(maintenanceRequests);
        }

        private bool PropertyExists(Guid id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
    }

    public class PaginationParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int Size { get; set; } = 10;
    }

    public class SearchPropertyParams
    {
        public string? Location { get; set; }
        public string? PriceRange { get; set; }
        public int? Bedrooms { get; set; }
        public List<string>? Amenities { get; set; }
    }

    public class CreatePropertyModel
    {
        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public PropertyType PropertyType { get; set; }

        [Required]
        [Range(0, 20)]
        public int Bedrooms { get; set; }

        [Required]
        [Range(0, 10)]
        public int Bathrooms { get; set; }

        [Required]
        [Range(0, 10000)]
        public int SquareFootage { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal RentAmount { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public List<string>? Amenities { get; set; }

        [Required]
        public bool IsAvailable { get; set; }
    }

    public class UpdatePropertyModel
    {
        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        public PropertyType? PropertyType { get; set; }

        [Range(0, 20)]
        public int? Bedrooms { get; set; }

        [Range(0, 10)]
        public int? Bathrooms { get; set; }

        [Range(0, 10000)]
        public int? SquareFootage { get; set; }

        [Range(0, 1000000)]
        public decimal? RentAmount { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public List<string>? Amenities { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
