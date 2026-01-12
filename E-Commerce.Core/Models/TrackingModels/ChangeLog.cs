namespace E_Commerce.Core.Models.TrackingModels;
    public class ChangeLog : BaseEntity
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? ActionType { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }

