// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Common.Units
{
    public class Unit
    {
        public Unit(string? symbol, string? word)
        {
            Symbol = symbol;
            Word = word;
        }

        public string? Symbol { get; }
        public string? Word { get; }


        public override string? ToString()
        {
            return string.IsNullOrWhiteSpace(Symbol) ? Word : Symbol;
        }
    }
}
