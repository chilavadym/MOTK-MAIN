// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common.Units;

public interface IUnitValue : IComparable<double>, IComparable
{
    double BaseValue { get; }

    double Value { get; }

    string ToString();
    string ToString(string format, Unit? unit);
    double ValueAs(Unit? unit);
}