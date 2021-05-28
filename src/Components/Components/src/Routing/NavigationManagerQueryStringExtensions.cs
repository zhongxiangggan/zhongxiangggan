using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// 
    /// </summary>
    public static class NavigationManagerQueryStringExtensions
    {
        private delegate bool TryParseDelegate<T>(string input, out T result);
        private delegate bool TryParseUntypedDelegate(string input, out object? result);
        private delegate string FormatDelegate<T>(T value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="navigationManager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? GetQueryParameter<T>(this NavigationManager navigationManager, string name)
        {
            var parsedQueryString = GetCurrentParsedQueryString(navigationManager);
            return parsedQueryString.TryGetValue(name, out var valueString)
                && TryParseQueryParameter<T>(valueString, out var result)
                ? result
                : default(T);
        }

        internal static bool TryParseQueryParameter<T>(string valueString, out T result)
        {
            if (_typedParsers.TryGetValue(typeof(T), out var tryParser))
            {
                return ((TryParseDelegate<T>)tryParser)(valueString, out result);
            }
            else
            {
                throw new NotSupportedException($"Cannot parse querystring values as type '{typeof(T)}'");
            }
        }

        internal static bool TryParseQueryParameter(string valueString, Type valueType, out object? result)
        {
            if (_untypedParsers.TryGetValue(valueType, out var tryParser))
            {
                return tryParser(valueString, out result);
            }
            else
            {
                throw new NotSupportedException($"Cannot parse querystring values as type '{valueType}'");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="navigationManager"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="replace"></param>
        public static void SetQueryParameter<T>(this NavigationManager navigationManager, string name, T? value, bool replace = false)
        {
            // For nullable types, setting to null is equivalent to removing. For non-nullable types,
            // you would have to call RemoveQueryParameter explicitly.
            if (value is null)
            {
                RemoveQueryParameter(navigationManager, name, replace);
                return;
            }

            var formattedValue = _formatters.TryGetValue(typeof(T), out var formatter)
                ? ((FormatDelegate<T>)formatter)(value)
                : throw new NotSupportedException($"Cannot format value of type '{typeof(T)}' for querystring.");

            // TODO: This whole approach of round-tripping through a dictionary isn't great, as it will lose
            // the ordering and is allocatey. We should consider having some parser that steps through the
            // tokens and can replace or remove a single parameter without affecting the result.
            var queryDict = GetCurrentParsedQueryString(navigationManager);
            if (queryDict.TryGetValue(name, out var existingFormattedValues))
            {
                if (!existingFormattedValues.Equals(formattedValue, StringComparison.Ordinal))
                {
                    // Existing, but value needs to be changed
                    queryDict[name] = formattedValue;
                    SetParsedQueryString(navigationManager, queryDict, replace);
                }
            }
            else
            {
                // No existing value
                queryDict[name] = formattedValue;
                SetParsedQueryString(navigationManager, queryDict, replace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="navigationManager"></param>
        /// <param name="name"></param>
        /// <param name="replace"></param>
        public static void RemoveQueryParameter(this NavigationManager navigationManager, string name, bool replace = false)
        {
            var queryDict = GetCurrentParsedQueryString(navigationManager);
            if (queryDict.Remove(name))
            {
                SetParsedQueryString(navigationManager, queryDict, replace);
            }
        }

        private static void SetParsedQueryString(NavigationManager navigationManager, Dictionary<string, string> queryDict, bool replace)
        {
            var oldUri = new Uri(navigationManager.CurrentOrPendingUri);
            var newUri = QueryHelpers.AddQueryString(
                oldUri.GetLeftPart(UriPartial.Path),
                queryDict);

            // TODO: Support 'replace' param
            navigationManager.NavigateTo(newUri);
        }

        private static Dictionary<string, string> GetCurrentParsedQueryString(NavigationManager navigationManager)
        {
            // TODO: Cache it in between URL changes. Presumably need to do that inside NavigationManager.
            // TODO: Handle decoding the URL-encoded values.
            var url = new Uri(navigationManager.CurrentOrPendingUri);
            return QueryHelpers.ParseQuery(url.Query);
        }

        // These are all equivalent to the parsers in RouteConstraint.cs
        private static readonly Dictionary<Type, Delegate> _typedParsers = new Dictionary<Type, Delegate>
        {
            { typeof(string), (TryParseDelegate<string>)TryParse },
            { typeof(bool), (TryParseDelegate<bool>)bool.TryParse },
            { typeof(DateTime), (TryParseDelegate<DateTime>)TryParse },
            { typeof(decimal), (TryParseDelegate<decimal>)TryParse },
            { typeof(double), (TryParseDelegate<double>)TryParse },
            { typeof(float), (TryParseDelegate<float>)TryParse },
            { typeof(Guid), (TryParseDelegate<Guid>)Guid.TryParse },
            { typeof(int), (TryParseDelegate<int>)TryParse },
            { typeof(long), (TryParseDelegate<long>)TryParse },
        };

        private static readonly Dictionary<Type, TryParseUntypedDelegate> _untypedParsers = new Dictionary<Type, TryParseUntypedDelegate>
        {
            { typeof(string), TryParseUntypedString },
            { typeof(bool), TryParseUntypedBool },
            { typeof(DateTime), TryParseUntypedDateTime },
            { typeof(decimal), TryParseUntypedDecimal },
            { typeof(double), TryParseUntypedDouble },
            { typeof(float), TryParseUntypedFloat },
            { typeof(Guid), TryParseUntypedGuid },
            { typeof(int), TryParseUntypedInt },
            { typeof(long), TryParseUntypedLong },
        };

        private static readonly Dictionary<Type, Delegate> _formatters = new Dictionary<Type, Delegate>
        {
            { typeof(string), (FormatDelegate<string>)(value => value) },
            { typeof(bool), (FormatDelegate<bool>)(value => value.ToString()) },
            { typeof(DateTime), (FormatDelegate<DateTime>)(value => value.ToString(CultureInfo.InvariantCulture)) },
            { typeof(decimal), (FormatDelegate<decimal>)(value => value.ToString(CultureInfo.InvariantCulture)) },
            { typeof(double), (FormatDelegate<double>)(value => value.ToString(CultureInfo.InvariantCulture)) },
            { typeof(float), (FormatDelegate<float>)(value => value.ToString(CultureInfo.InvariantCulture)) },
            { typeof(Guid), (FormatDelegate<Guid>)(value => value.ToString()) },
            { typeof(int), (FormatDelegate<int>)(value => value.ToString(CultureInfo.InvariantCulture)) },
            { typeof(long), (FormatDelegate<long>)(value => value.ToString(CultureInfo.InvariantCulture)) },
        };

        private static bool TryParse(string str, out string result)
        {
            result = str;
            return true;
        }

        private static bool TryParse(string str, out DateTime result)
            => DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);

        private static bool TryParse(string str, out decimal result)
            => decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

        private static bool TryParse(string str, out double result)
            => double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

        private static bool TryParse(string str, out float result)
            => float.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

        private static bool TryParse(string str, out int result)
            => int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

        private static bool TryParse(string str, out long result)
            => long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

        private static bool TryParseUntypedBool(string str, out object? result)
        {
            if (bool.TryParse(str, out var typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedString(string str, out object? result)
        {
            result = str;
            return true;
        }

        private static bool TryParseUntypedDateTime(string str, out object? result)
        {
            if (TryParse(str, out DateTime typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedDecimal(string str, out object? result)
        {
            if (TryParse(str, out decimal typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedDouble(string str, out object? result)
        {
            if (TryParse(str, out double typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedFloat(string str, out object? result)
        {
            if (TryParse(str, out float typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedGuid(string str, out object? result)
        {
            if (Guid.TryParse(str, out Guid typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedInt(string str, out object? result)
        {
            if (TryParse(str, out int typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseUntypedLong(string str, out object? result)
        {
            if (TryParse(str, out long typedResult))
            {
                result = typedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
