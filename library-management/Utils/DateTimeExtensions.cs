using System;

namespace library_management.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime SpecifyKindUtc(this DateTime dt)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
    }
} 