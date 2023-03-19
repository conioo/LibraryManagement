using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Application.Extensions
{
    public static class EntityEntryExtension
    {
        public static string? GetPrimaryKey<T>(this EntityEntry<T> entityEntry) where T : class
        {
            var keyName = entityEntry.Metadata.FindPrimaryKey()?.Properties.First().Name;

            if(keyName is null)
            {
                return null;
            }

            return entityEntry.Property(keyName).CurrentValue?.ToString();
        }


    }
}
