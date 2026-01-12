namespace E_Commerce.Core.Shared.Utilties.Enums;
public enum Status
{
    Pending,
    Paid,
    Canceled,
    Refunded,
    Sent
}

/* 
    // Smart Enum can provide a Parse method used to map a string value to an enum value
    public abstract record SmartEnum<TEnum, TKey>(TKey Key)
        where TEnum : SmartEnum<TEnum, TKey>
        where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        public static Dictionary<TKey, TEnum> GetAll()
        {
            var enumType = typeof(TEnum);
            var fieldTypes = enumType.GetFields(BindingFlags.Public | BindingFlags.Static 
                | BindingFlags.FlattenHierarchy)
                .Where(fieldInfo => enumType.IsAssignableFrom(fieldInfo.FieldType))
                .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

            return fieldTypes.ToDictionary(x => x.Key);
        }

        public static TEnum? GetByKey(TKey key)
        => GetAll().TryGetValue(key, out TEnum? val) ? val : null;

        public static IEnumerable<TKey> GetKeys()
            => GetAll().Select(x => x.Key);
        public static string CommaSeperatedkeys()
        {
            return $"{string.Join(",", GetKeys())}";
        }

        public static implicit operator TKey(SmartEnum<TEnum, TKey> e)
            => e.Key;

        public static implicit operator SmartEnum<TEnum, TKey>(TKey key)
            => GetAll()[key];

        public static implicit operator string(SmartEnum<TEnum, TKey> e)
            => e.Key.ToString()!;

        public bool IsValid()
            => GetByKey(Key) != null;

        public override string ToString() => Key.ToString();
    }
}
*/