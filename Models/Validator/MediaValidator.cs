using Cloud.Models.DTO;

namespace Cloud.Models.Validator
{
	/// <summary>
	/// Interface for Media validation strategies
	/// </summary>
	public interface IMediaValidationStrategy
	{
		void Validate(CreateMediaDto mediaDto, Dictionary<string, List<string>> errors);
	}

	/// <summary>
	/// Extension methods for IMediaValidationStrategy
	/// </summary>
	public static class MediaValidationStrategyExtensions
	{
		/// <summary>
		/// Adds an error to the errors dictionary
		/// </summary>
		/// <param name="strategy">The validation strategy</param>
		/// <param name="errors">The errors dictionary</param>
		/// <param name="key">The key for the error (usually property name)</param>
		/// <param name="error">The error message</param>
		public static void AddError(this IMediaValidationStrategy strategy, Dictionary<string, List<string>> errors, string key, string error)
		{
			if (!errors.ContainsKey(key))
			{
				errors[key] = new List<string>();
			}
			errors[key].Add(error);
		}
	}

	/// <summary>
	/// Validator for CreateMediaDto using strategy pattern
	/// </summary>
	public class CreateMediaDtoValidator
	{
		private readonly List<IMediaValidationStrategy> _validationStrategies = new List<IMediaValidationStrategy>();

		/// <summary>
		/// Initializes a new instance of the CreateMediaDtoValidator class
		/// </summary>
		public CreateMediaDtoValidator()
		{
			AddStrategy(new FileRequiredValidationStrategy());
			AddStrategy(new FileSizeValidationStrategy());
			AddStrategy(new FileTypeValidationStrategy());
			AddStrategy(new CustomFileNameValidationStrategy());
		}

		/// <summary>
		/// Adds a new validation strategy
		/// </summary>
		/// <param name="strategy">The strategy to add</param>
		public void AddStrategy(IMediaValidationStrategy strategy)
		{
			_validationStrategies.Add(strategy);
		}

		/// <summary>
		/// Validates the CreateMediaDto
		/// </summary>
		/// <param name="mediaDto">The DTO to validate</param>
		public void ValidateMedia(CreateMediaDto mediaDto)
		{
			var errors = new Dictionary<string, List<string>>();

			foreach (var strategy in _validationStrategies)
			{
				strategy.Validate(mediaDto, errors);
			}

			if (errors.Any())
			{
				throw new ValidationException(errors);
			}
		}
	}

	/// <summary>
	/// Validates that the file is present
	/// </summary>
	public class FileRequiredValidationStrategy : IMediaValidationStrategy
	{
		public void Validate(CreateMediaDto mediaDto, Dictionary<string, List<string>> errors)
		{
			if (mediaDto.File == null)
			{
				this.AddError(errors, "File", "File is required");
			}
		}
	}

	/// <summary>
	/// Validates the file size
	/// </summary>
	public class FileSizeValidationStrategy : IMediaValidationStrategy
	{
		private const int MaxFileSizeInBytes = 10 * 1024 * 1024; // 10MB

		public void Validate(CreateMediaDto mediaDto, Dictionary<string, List<string>> errors)
		{
			if (mediaDto.File != null && mediaDto.File.Length > MaxFileSizeInBytes)
			{
				this.AddError(errors, "File", "File size must not exceed 10MB");
			}
		}
	}

	/// <summary>
	/// Validates the file type
	/// </summary>
	public class FileTypeValidationStrategy : IMediaValidationStrategy
	{
		private readonly HashSet<string> _allowedTypes = new HashSet<string>
		{
			"image/jpeg", "image/png", "image/gif", "application/pdf"
		};

		public void Validate(CreateMediaDto mediaDto, Dictionary<string, List<string>> errors)
		{
			if (mediaDto.File != null && !_allowedTypes.Contains(mediaDto.File.ContentType.ToLower()))
			{
				this.AddError(errors, "File", "Only image files (JPEG, PNG, GIF) and PDFs are allowed");
			}
		}
	}

	/// <summary>
	/// Validates the custom file name if provided
	/// </summary>
	public class CustomFileNameValidationStrategy : IMediaValidationStrategy
	{
		public void Validate(CreateMediaDto mediaDto, Dictionary<string, List<string>> errors)
		{
			if (!string.IsNullOrEmpty(mediaDto.CustomFileName))
			{
				if (mediaDto.CustomFileName.Length > 255)
				{
					this.AddError(errors, "CustomFileName", "Custom file name must not exceed 255 characters");
				}

				if (!System.Text.RegularExpressions.Regex.IsMatch(mediaDto.CustomFileName, @"^[a-zA-Z0-9\-_\.]+$"))
				{
					this.AddError(errors, "CustomFileName", "Custom file name can only contain letters, numbers, hyphens, underscores, and periods");
				}
			}
		}
	}
}