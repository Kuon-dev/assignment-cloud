/// <summary>
/// Custom exception for validation errors
/// </summary>
public class ValidationException : Exception
{
	/// <summary>
	/// Collection of validation errors
	/// </summary>
	public IReadOnlyDictionary<string, List<string>> Errors { get; }

	/// <summary>
	/// Initializes a new instance of the ValidationException class with a single error
	/// </summary>
	/// <param name="propertyName">The name of the property that failed validation</param>
	/// <param name="errorMessage">The error message</param>
	public ValidationException(string propertyName, string errorMessage)
		: base("One or more validation errors occurred.")
	{
		Errors = new Dictionary<string, List<string>>
			{
				{ propertyName, new List<string> { errorMessage } }
			};
	}

	/// <summary>
	/// Initializes a new instance of the ValidationException class with multiple errors
	/// </summary>
	/// <param name="errors">A dictionary of property names and their corresponding error messages</param>
	public ValidationException(IDictionary<string, List<string>> errors)
		: base("One or more validation errors occurred.")
	{
		Errors = new Dictionary<string, List<string>>(errors);
	}

	/// <summary>
	/// Returns a string representation of the validation errors
	/// </summary>
	/// <returns>A formatted string of all validation errors</returns>
	public override string ToString()
	{
		return $"ValidationException: {base.Message}\n" +
			   string.Join("\n", Errors.SelectMany(kvp =>
				   kvp.Value.Select(error => $"{kvp.Key}: {error}")));
	}
}