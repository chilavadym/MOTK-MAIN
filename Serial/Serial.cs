// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace Serial
{
    public interface ISerializer<TProp>
    {
        byte[] Serialize(TProp value);
    }
    public interface ISerializer<TProp, TAddr> : ISerializer<TProp>
    {
        byte[] Serialize(TProp value, TAddr addr);
    }

    public interface IDeserializer<TProp>
    {
        TProp Deserialize(byte[] data);
    }
    public interface IDeserializer<TProp, TAddr> : IDeserializer<TProp>
    {
        TProp Deserialize(byte[] data, TAddr addr);
    }
}
