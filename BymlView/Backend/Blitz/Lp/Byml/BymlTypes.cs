using System;
using System.Runtime.InteropServices;

namespace LibBlitz.Lp.Byml
{
    public enum BymlNodeId : byte
    {
        String = 0xA0,
        Bin = 0xA1,
        Array = 0xC0,
        Hash = 0xC1,
        StringTable = 0xC2,
        Bool = 0xD0,
        Int = 0xD1,
        Float = 0xD2,
        UInt = 0xD3,
        Int64 = 0xD4,
        UInt64 = 0xD5,
        Double = 0xD6,
        Null = 0xFF,
    };

    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct BymlHeader
    {
        public ushort Magic;
        public ushort Version;
        public uint HashKeyOffset;
        public uint StringTableOffset;
        public uint RootOffset;
    }

    public struct BymlHashPair : IComparable<string>, IComparable<BymlHashPair>
    {
        public string Name;
        public BymlNodeId Id;
        public IBymlNode Value;

        public int CompareTo(string? other)
        {
            if (other == null) return 1;
            return string.CompareOrdinal(Name, other);
        }

        public int CompareTo(BymlHashPair other)
        {
            return CompareTo(other.Name);
        }
    }

    public interface IBymlNode
    {
        public BymlNodeId Id { get; }
    }

    public class BymlNode<T> : IBymlNode
    {
        public BymlNodeId Id { get; }
        public T Data;

        public BymlNode(BymlNodeId id, T data)
        {
            Id = id;
            Data = data;
        }
    }
}
