using System;
using System.Linq;

namespace CodeFex.NetCore.Json.Serialization
{
    public static class TypeChecker
    {
        public static bool IsAssignableToInterface(this Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null) return false;

#if NET5
            return sourceType.IsAssignableFrom(targetType);
#else
            return sourceType.GetInterfaces().Any(i => i.IsAssignableFrom(targetType));
#endif
        }
    }
}
