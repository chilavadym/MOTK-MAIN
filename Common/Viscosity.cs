// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common;

public class Viscosity : IComparable<Viscosity>, IComparable<string>, IComparable, IEquatable<Viscosity>, IEquatable<string>
{
    public static ViscosityComparer ViscosityComparer { get; } = new();
    public static StringComparer StringComparer { get => StringComparer.InvariantCultureIgnoreCase; }

    public string Value { get; }

    public Viscosity(string? value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            Viscosity visc => CompareTo(visc),
            string str => CompareTo(str),
            _ => throw new ArgumentException($"Type must be of {GetType().Name} or {typeof(string).Name}, is actually of type {obj.GetType()}."),
        };
    }
    public int CompareTo(Viscosity? other)
    {
        return ViscosityComparer.Compare(this, other);
    }
    public int CompareTo(string? other)
    {
        return CompareTo((other is null ? null : new Viscosity(other)) ?? throw new InvalidOperationException());
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Viscosity visc => Equals(visc),
            string str => Equals(new Viscosity(str)),
            _ => false,
        };
    }
    public bool Equals(Viscosity? other)
    {
        return CompareTo(other) == 0;
    }
    public bool Equals(string? other)
    {
        return CompareTo(other) == 0;
    }

    public override int GetHashCode()
    {
        return Value.ToUpper().GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}