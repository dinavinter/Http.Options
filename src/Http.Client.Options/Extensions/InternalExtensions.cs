using System;

namespace Http.Options
{
    internal static class InternalExtensions
    {
        internal static TResult NullOr<T, TResult>(this T value, Func<T, TResult> selector) where TResult : class
        {
            return value == null ? null : selector(value);
        }

        internal static TResult? NullableOr<T, TResult>(this T? value, Func<T, TResult> selector)
            where TResult : struct where T : struct
        {
            return value.HasValue ? selector(value.Value) : (TResult?) null;
        }
    }
}