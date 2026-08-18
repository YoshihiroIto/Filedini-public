using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Filedini.Localization;

public static class SmartFormat
{
    private const int InitialBufferSize = 256;
    private const string PluralPrefix = "plural:";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(string format, object? arg0)
        => FormatCore(format, new FormatArguments(arg0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(string format, object? arg0, object? arg1)
        => FormatCore(format, new FormatArguments(arg0, arg1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(string format, object? arg0, object? arg1, object? arg2)
        => FormatCore(format, new FormatArguments(arg0, arg1, arg2));

    private static string FormatCore(string format, in FormatArguments arguments)
    {
        Span<char> initialBuffer = stackalloc char[InitialBufferSize];
        var builder = new PooledCharBuilder(initialBuffer);
        var culture = CultureInfo.CurrentCulture;

        try
        {
            AppendFormat(format.AsSpan(), in arguments, culture, ref builder);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static void AppendFormat(
        ReadOnlySpan<char> format,
        in FormatArguments arguments,
        CultureInfo culture,
        ref PooledCharBuilder builder)
    {
        var position = 0;

        while (position < format.Length)
        {
            var relativeBraceIndex = format[position..].IndexOfAny('{', '}');
            if (relativeBraceIndex < 0)
            {
                builder.Append(format[position..]);
                return;
            }

            var braceIndex = position + relativeBraceIndex;
            builder.Append(format[position..braceIndex]);

            if (format[braceIndex] == '{')
            {
                if (braceIndex + 1 < format.Length && format[braceIndex + 1] == '{')
                {
                    builder.Append('{');
                    position = braceIndex + 2;
                    continue;
                }

                var placeholderEnd = FindPlaceholderEnd(format, braceIndex + 1);
                if (placeholderEnd < 0)
                    ThrowInvalidFormat();

                AppendPlaceholder(
                    format[(braceIndex + 1)..placeholderEnd],
                    in arguments,
                    culture,
                    ref builder);
                position = placeholderEnd + 1;
                continue;
            }

            if (braceIndex + 1 < format.Length && format[braceIndex + 1] == '}')
            {
                builder.Append('}');
                position = braceIndex + 2;
                continue;
            }

            ThrowInvalidFormat();
        }
    }

    private static void AppendPlaceholder(
        ReadOnlySpan<char> placeholder,
        in FormatArguments arguments,
        CultureInfo culture,
        ref PooledCharBuilder builder)
    {
        if (placeholder.IsEmpty)
            ThrowInvalidFormat();

        var index = placeholder[0] - '0';
        if ((uint)index > 2u)
            ThrowInvalidFormat();

        var value = arguments.Get(index);
        if (placeholder.Length == 1)
        {
            builder.AppendFormatted(value, default, culture);
            return;
        }

        if (placeholder[1] != ':')
            ThrowInvalidFormat();

        var valueFormat = placeholder[2..];
        if (valueFormat.StartsWith(PluralPrefix, StringComparison.Ordinal))
        {
            AppendPlural(valueFormat[PluralPrefix.Length..], value, culture, ref builder);
            return;
        }

        builder.AppendFormatted(value, valueFormat, culture);
    }

    private static void AppendPlural(
        ReadOnlySpan<char> pluralFormat,
        object? value,
        CultureInfo culture,
        ref PooledCharBuilder builder)
    {
        var separatorIndex = FindPluralSeparator(pluralFormat);
        if (separatorIndex < 0)
            ThrowInvalidFormat();

        var selected = IsOne(value)
            ? pluralFormat[..separatorIndex]
            : pluralFormat[(separatorIndex + 1)..];

        AppendPluralBranch(selected, value, culture, ref builder);
    }

    private static void AppendPluralBranch(
        ReadOnlySpan<char> branch,
        object? value,
        CultureInfo culture,
        ref PooledCharBuilder builder)
    {
        var position = 0;

        while (position < branch.Length)
        {
            var relativeBraceIndex = branch[position..].IndexOfAny('{', '}');
            if (relativeBraceIndex < 0)
            {
                builder.Append(branch[position..]);
                return;
            }

            var braceIndex = position + relativeBraceIndex;
            builder.Append(branch[position..braceIndex]);

            if (branch[braceIndex] == '{')
            {
                if (braceIndex + 1 < branch.Length && branch[braceIndex + 1] == '{')
                {
                    builder.Append('{');
                    position = braceIndex + 2;
                    continue;
                }

                var relativeEnd = branch[(braceIndex + 1)..].IndexOf('}');
                if (relativeEnd < 0)
                    ThrowInvalidFormat();

                var placeholderEnd = braceIndex + 1 + relativeEnd;
                var placeholder = branch[(braceIndex + 1)..placeholderEnd];

                if (placeholder.IsEmpty)
                    builder.AppendFormatted(value, default, culture);
                else if (placeholder[0] == ':')
                    builder.AppendFormatted(value, placeholder[1..], culture);
                else
                    ThrowInvalidFormat();

                position = placeholderEnd + 1;
                continue;
            }

            if (braceIndex + 1 < branch.Length && branch[braceIndex + 1] == '}')
            {
                builder.Append('}');
                position = braceIndex + 2;
                continue;
            }

            ThrowInvalidFormat();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOne(object? value)
    {
        return value switch
        {
            byte number => number == 1,
            sbyte number => number == 1,
            short number => number == 1,
            ushort number => number == 1,
            int number => number == 1,
            uint number => number == 1,
            long number => number == 1,
            ulong number => number == 1,
            // ReSharper disable CompareOfFloatsByEqualityOperator
            float number => number == 1f,
            double number => number == 1d,
            // ReSharper restore CompareOfFloatsByEqualityOperator
            decimal number => number == 1m,
            IConvertible convertible when value is not (bool or string) => ConvertToDecimalIsOne(convertible),
            _ => ThrowInvalidPluralValue(),
        };
    }

    private static bool ConvertToDecimalIsOne(IConvertible value)
    {
        try
        {
            return value.ToDecimal(CultureInfo.InvariantCulture) == 1m;
        }
        catch (InvalidCastException)
        {
            return ThrowInvalidPluralValue();
        }
        catch (FormatException)
        {
            return ThrowInvalidPluralValue();
        }
        catch (OverflowException)
        {
            return ThrowInvalidPluralValue();
        }
    }

    private static int FindPlaceholderEnd(ReadOnlySpan<char> format, int start)
    {
        var nestedDepth = 0;

        for (var i = start; i < format.Length; i++)
        {
            switch (format[i])
            {
                case '{':
                    nestedDepth++;
                    break;
                case '}' when nestedDepth == 0:
                    return i;
                case '}':
                    nestedDepth--;
                    break;
            }
        }

        return -1;
    }

    private static int FindPluralSeparator(ReadOnlySpan<char> format)
    {
        var nestedDepth = 0;

        for (var i = 0; i < format.Length; i++)
        {
            switch (format[i])
            {
                case '{':
                    nestedDepth++;
                    break;
                case '}':
                    nestedDepth--;
                    break;
                case '|' when nestedDepth == 0:
                    return i;
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidFormat()
        => throw new FormatException("Input string was not in a correct Filedini format.");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ThrowInvalidPluralValue()
        => throw new FormatException("Plural formatting requires a numeric value.");

    private readonly struct FormatArguments
    {
        private readonly object? _arg0;
        private readonly object? _arg1;
        private readonly object? _arg2;
        private readonly byte _count;

        public FormatArguments(object? arg0)
        {
            _arg0 = arg0;
            _arg1 = null;
            _arg2 = null;
            _count = 1;
        }

        public FormatArguments(object? arg0, object? arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = null;
            _count = 2;
        }

        public FormatArguments(object? arg0, object? arg1, object? arg2)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _count = 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? Get(int index)
        {
            if ((uint)index >= _count)
                ThrowInvalidFormat();

            return index switch
            {
                0 => _arg0,
                1 => _arg1,
                2 => _arg2,
                _ => null,
            };
        }
    }

    private ref struct PooledCharBuilder(Span<char> initialBuffer)
    {
        private Span<char> _buffer = initialBuffer;
        private char[]? _arrayToReturnToPool;
        private int _position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char value)
        {
            if ((uint)_position < (uint)_buffer.Length)
            {
                _buffer[_position++] = value;
                return;
            }

            Grow(1);
            _buffer[_position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
                return;

            if (value.Length > _buffer.Length - _position)
                Grow(value.Length);

            value.CopyTo(_buffer[_position..]);
            _position += value.Length;
        }

        public void AppendFormatted(object? value, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            if (value is null)
                return;

            if (value is ISpanFormattable spanFormattable)
            {
                while (true)
                {
                    if (spanFormattable.TryFormat(
                            _buffer[_position..], out var charsWritten, format, provider))
                    {
                        _position += charsWritten;
                        return;
                    }

                    Grow(Math.Max(64, _buffer.Length));
                }
            }

            if (value is IFormattable formattable)
            {
                var formatString = format.IsEmpty ? null : format.ToString();
                Append(formattable.ToString(formatString, provider));
                return;
            }

            Append(value.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Append(string? value)
        {
            if (value is not null)
                Append(value.AsSpan());
        }

        public override string ToString()
            => _buffer[.._position].ToString();

        public void Dispose()
        {
            var array = _arrayToReturnToPool;
            this = default;

            if (array is not null)
                ArrayPool<char>.Shared.Return(array);
        }

        private void Grow(int additionalCapacity)
        {
            var requiredCapacity = checked(_position + additionalCapacity);
            var doubledCapacity = checked(_buffer.Length * 2);
            var newArray = ArrayPool<char>.Shared.Rent(Math.Max(requiredCapacity, doubledCapacity));
            _buffer[.._position].CopyTo(newArray);

            var previousArray = _arrayToReturnToPool;
            _buffer = newArray;
            _arrayToReturnToPool = newArray;

            if (previousArray is not null)
                ArrayPool<char>.Shared.Return(previousArray);
        }
    }
}
