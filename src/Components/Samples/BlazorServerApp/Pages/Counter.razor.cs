// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorServerApp.Pages
{
    public partial class Counter
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private string? SelfUrl(int incrementAmount)
        {
            return Nav.UrlWithQuery("inc", incrementAmount).WithQuery("another", "blah");
        }
    }

    public static class NavigationManagerQueryExtensions
    {
        public static QueryBuilder UrlWithQuery(this NavigationManager navigationManager, string name, string value)
            => new QueryBuilder(navigationManager.Uri).WithQuery(name, value);
        public static QueryBuilder UrlWithQuery(this NavigationManager navigationManager, string name, int? value)
            => new QueryBuilder(navigationManager.Uri).WithQuery(name, value);
    }

    public struct QueryBuilder
    {
        public static implicit operator string(QueryBuilder queryBuilder)
            => queryBuilder.ToString()!;

        private readonly string _baseUrl;

        private int _numPendingValues;
        private PendingValue? PendingValue1;
        private PendingValue? PendingValue2;
        private PendingValue? PendingValue3;
        // TODO: List if you need more

        public QueryBuilder(string baseUrl)
        {
            this = default;
            _baseUrl = baseUrl;
        }

        private QueryBuilder Append(PendingValue addPendingValue)
        {
            switch (++_numPendingValues)
            {
                case 1:
                    PendingValue1 = addPendingValue;
                    break;
                case 2:
                    PendingValue2 = addPendingValue;
                    break;
                case 3:
                    PendingValue3 = addPendingValue;
                    break;
                default:
                    throw new NotImplementedException("More than 3 pending values");
            }

            return this;
        }

        public override string ToString()
        {
            return string.Create(100, this, static (chars, state) =>
            {
                var queryStartPos = state._baseUrl.IndexOf('?');
                ReadOnlyMemory<char> preQuery;
                ReadOnlyMemory<char> query;
                if (queryStartPos < 0)
                {
                    preQuery = state._baseUrl.AsMemory();
                    query = default;
                }
                else
                {
                    preQuery = state._baseUrl.AsMemory(0, queryStartPos);
                    query = state._baseUrl.AsMemory(queryStartPos);
                }

                preQuery.Span.CopyTo(chars);
                var pos = preQuery.Length;
                var usedPendingValue1 = false;
                var usedPendingValue2 = false;
                var usedPendingValue3 = false;

                var enumerable = new QueryStringEnumerable(query);
                var first = true;
                foreach (var pair in enumerable)
                {
                    chars[pos++] = first ? '?' : '&';
                    first = false;

                    pair.EncodedName.Span.CopyTo(chars.Slice(pos));
                    pos += pair.EncodedName.Length;
                    chars[pos++] = '=';

                    var decodedName = pair.DecodeName();
                    if (state.PendingValue1.HasValue && decodedName.Span.Equals(state.PendingValue1.Value.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        state.PendingValue1.Value.CopyValueToOutput(chars, ref pos); // TODO: Encode
                        usedPendingValue1 = true;
                    }
                    else if (state.PendingValue2.HasValue && decodedName.Span.Equals(state.PendingValue2.Value.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        state.PendingValue2.Value.CopyValueToOutput(chars, ref pos); // TODO: Encode
                        usedPendingValue2 = true;
                    }
                    else if (state.PendingValue3.HasValue && decodedName.Span.Equals(state.PendingValue3.Value.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        state.PendingValue3.Value.CopyValueToOutput(chars, ref pos); // TODO: Encode
                        usedPendingValue3 = true;
                    }
                    else
                    {
                        pair.EncodedValue.Span.CopyTo(chars.Slice(pos));
                        pos += pair.EncodedValue.Length;
                    }
                }

                if (!usedPendingValue1 && state.PendingValue1.HasValue)
                {
                    chars[pos++] = first ? '?' : '&';
                    first = false;
                    state.PendingValue1.Value.CopyNameAndValueToOutput(chars, ref pos);
                }

                if (!usedPendingValue2 && state.PendingValue2.HasValue)
                {
                    chars[pos++] = first ? '?' : '&';
                    first = false;
                    state.PendingValue2.Value.CopyNameAndValueToOutput(chars, ref pos);
                }

                if (!usedPendingValue3 && state.PendingValue3.HasValue)
                {
                    chars[pos++] = first ? '?' : '&';
                    first = false;
                    state.PendingValue3.Value.CopyNameAndValueToOutput(chars, ref pos);
                }
            });
        }

        public QueryBuilder WithQuery<T>(string name, T value) where T : struct, ISpanFormattable
            => Append(PendingValue.Create(name, value));
        public QueryBuilder WithQuery<T>(string name, T? value) where T: struct, ISpanFormattable
            => Append(value.HasValue ? PendingValue.Create(name, value.Value) : PendingValue.Create(name, (string?)null));
        public QueryBuilder WithQuery(string name, string? value)
            => Append(PendingValue.Create(name, value));

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct PendingValue
        {
            private const int BufLength = 128;

            [FieldOffset(0)]
            public readonly string Name;

            [FieldOffset(8)]
            public string? FormattedString;

            [FieldOffset(16)]
            public int? FormattedCharsLength;

            [FieldOffset(24)]
            public fixed char FormattedChars[BufLength];

            public static PendingValue Create(string name, string? value)
            {
                var result = new PendingValue(name);
                result.FormattedString = value;
                return result;
            }

            public static PendingValue Create<T>(string name, T value) where T: ISpanFormattable
            {
                var result = new PendingValue(name);
                result.Format(value);
                return result;
            }

            private void Format<T>(T value) where T : ISpanFormattable
            {
                fixed (char* buffer = FormattedChars)
                {
                    var span = new Span<char>(buffer, BufLength);
                    if (value.TryFormat(span, out var charsWritten, default, default))
                    {
                        FormattedCharsLength = charsWritten;
                    }
                    else
                    {
                        FormattedString = value.ToString();
                    }
                }
            }

            public void CopyNameAndValueToOutput(Span<char> output, ref int pos)
            {
                Name.CopyTo(output.Slice(pos));
                pos += Name.Length;
                output[pos++] = '=';
                CopyValueToOutput(output, ref pos);
            }

            public void CopyValueToOutput(Span<char> output, ref int pos)
            {
                if (FormattedCharsLength.HasValue)
                {
                    fixed (char* formattedChars = FormattedChars)
                    {
                        var source = new Span<char>(formattedChars, FormattedCharsLength.Value);
                        source.CopyTo(output.Slice(pos));
                        pos += FormattedCharsLength.Value;
                    }
                }
                else if (FormattedString is not null && FormattedString.Length > 0)
                {
                    FormattedString.AsSpan().CopyTo(output.Slice(pos));
                    pos += FormattedString.Length;
                }
            }

            private PendingValue(string name)
            {
                this = default;
                Name = name;
            }
        }
    }
}
