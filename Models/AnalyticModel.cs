namespace Cloud.Models {
  public class PerformanceAnalytics {
	public int TotalListings { get; set; }
	public decimal AveragePrice { get; set; }
	public int TotalApplications { get; set; }
  }

  public class ListingAnalytics {
	public Guid ListingId { get; set; }
	public int Views { get; set; }
	public int Applications { get; set; }
	public DateTime LastUpdated { get; set; }
  }
}