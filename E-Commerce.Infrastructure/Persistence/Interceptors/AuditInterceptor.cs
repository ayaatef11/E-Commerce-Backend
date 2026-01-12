using Microsoft.EntityFrameworkCore.Diagnostics;
namespace E_Commerce.Infrastructure.Persistence.Interceptors;
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly List<AuditEntry> _auditEntries = new();

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
                {
                    var now = DateTime.UtcNow;
                    var user = GetCurrentUser();

                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = user;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = user;
                    }
                }
                var auditEntry = new AuditEntry
                {
                    StartTimeUtc = DateTime.UtcNow,
                    Metadata = GenerateMetadata(context)
                };

                _auditEntries.Add(auditEntry);

                if (context.Set<AuditEntry>() != null)
                {
                    context.Set<AuditEntry>().Add(auditEntry);
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                foreach (var entry in _auditEntries)
                {
                    entry.EndTimeUtc = DateTime.UtcNow;
                    entry.Succeeded = true;
                }
                context.SaveChanges();
            }

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            var context = eventData.Context;
            if (context != null)
            {
                foreach (var entry in _auditEntries)
                {
                    entry.EndTimeUtc = DateTime.UtcNow;
                    entry.Succeeded = false;
                    entry.ErrorMessage = eventData.Exception.Message[..255];
                }

                try
                {
                    using var newContext = new AuditContext();
                    newContext.AddRange(_auditEntries);
                    newContext.SaveChanges();
                }
                catch {  }
            }

            base.SaveChangesFailed(eventData);
        }

        private string GenerateMetadata(DbContext context)
        {
            var changes = context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .Select(entry => new
                {
                    Entity = entry.Entity.GetType().Name,
                    Id = GetEntityId(entry.Entity),
                    State = entry.State.ToString(),
                    Properties = entry.Properties.ToDictionary(
                        p => p.Metadata.Name,
                        p => p.CurrentValue)
                });

            return JsonSerializer.Serialize(changes);
        }

        private static string GetEntityId(object entity)
        {
            var type = entity.GetType();
            var idProperty = type.GetProperty("Id") ?? type.GetProperty($"{type.Name}Id");
            return idProperty?.GetValue(entity)?.ToString() ?? "Unknown";
        }

        private static string GetCurrentUser()
        {
            return Environment.UserName;
        }
    }
   
