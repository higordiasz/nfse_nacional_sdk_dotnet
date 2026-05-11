#if NETSTANDARD2_0
using System.Globalization;

namespace System;

public readonly struct DateOnly : IComparable<DateOnly>, IEquatable<DateOnly>, IFormattable
{
    private readonly DateTime _date;

    public DateOnly(int year, int month, int day)
    {
        _date = new DateTime(year, month, day);
    }

    private DateOnly(DateTime date)
    {
        _date = date.Date;
    }

    public int Year => _date.Year;

    public int Month => _date.Month;

    public int Day => _date.Day;

    public static DateOnly FromDateTime(DateTime dateTime)
    {
        return new DateOnly(dateTime.Date);
    }

    public static bool TryParse(
        string? value,
        IFormatProvider? provider,
        DateTimeStyles styles,
        out DateOnly result)
    {
        if (DateTime.TryParse(value, provider, styles, out var parsed))
        {
            result = new DateOnly(parsed);
            return true;
        }

        result = default;
        return false;
    }

    public int CompareTo(DateOnly other)
    {
        return _date.CompareTo(other._date);
    }

    public bool Equals(DateOnly other)
    {
        return _date.Equals(other._date);
    }

    public override bool Equals(object? obj)
    {
        return obj is DateOnly other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _date.GetHashCode();
    }

    public override string ToString()
    {
        return _date.ToString("d", CultureInfo.CurrentCulture);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return _date.ToString(format, formatProvider);
    }

    public static bool operator ==(DateOnly left, DateOnly right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DateOnly left, DateOnly right)
    {
        return !left.Equals(right);
    }
}
#endif
