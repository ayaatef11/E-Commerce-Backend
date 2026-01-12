namespace E_Commerce.Infrastructure.Persistence.Interceptors;
public class Movie : IAuditable
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}