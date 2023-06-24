using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Helpers
{
    public static class EntityHelper
    {
        private static readonly Dictionary<Type, string> keyNames = new Dictionary<Type, string>()
        {
            {typeof(Library), "Id" },
            {typeof(Item), "Id" },
            {typeof(Copy), "InventoryNumber" },
        };
        public static string GetKeyName<TEntity>() where TEntity : class
        {
            return keyNames[typeof(TEntity)];
        }

        public static string? GetKeyValue<TEntity>(TEntity entity) where TEntity : class
        {
            var keyPropertyInfo = typeof(TEntity).GetProperty(GetKeyName<TEntity>());

            return (string?)keyPropertyInfo?.GetValue(entity);
        }

        public static void SetKeyValue<TEntity>(TEntity entity, string id) where TEntity : class
        {
            var keyPropertyInfo = typeof(TEntity).GetProperty(GetKeyName<TEntity>());

            if (keyPropertyInfo is null)
            {
                throw new NullReferenceException();
            }

            keyPropertyInfo.SetValue(entity, id);
        }

        public static TEntity GetEntityWithKey<TEntity>(string keyValue) where TEntity : class
        {
            var instance = Activator.CreateInstance(typeof(TEntity));

            if(instance is null)
            {
                throw new NullReferenceException();
            }
            var keyPropertyInfo = typeof(TEntity).GetProperty(GetKeyName<TEntity>());

            if(keyPropertyInfo is null)
            {
                throw new NullReferenceException();
            }

            keyPropertyInfo.SetValue(instance, keyValue);

            return (TEntity)instance;
        }
    }
}
