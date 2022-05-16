using BymlView.Backend;
using LibHac.Fs;
using LibHac.Tools.FsSystem;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LibBlitz.Sead
{
    public class Sarc
    {

        [StructLayout(LayoutKind.Sequential, Size=0x14)]
        public struct SarcHeader
        {
            public uint Magic;
            public ushort HeaderSize;
            public ushort Bom;
            public uint FileSize;
            public uint DataStart;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0xC)]
        private struct SfatHeader
        {
            public uint Magic;
            public ushort HeaderSize;
            public ushort NodeCount;
            public uint HashKey;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x8)]
        private struct SnftHeader
        {
            public uint Magic;
            public uint HeaderSize;
        }

        [StructLayout(LayoutKind.Sequential, Size=0x10)]
        public struct FileNode : IComparable<uint>
        {
            public uint NameHash;
            public uint FlagsAndNameOffset;
            public uint FileDataBegin;
            public uint FileDataEnd;

            public uint NameOffset => FlagsAndNameOffset & 0xffffff;
            public uint Flags => FlagsAndNameOffset >> 24;
            public uint FileDataLength => FileDataEnd - FileDataBegin;

            public int CompareTo(uint other)
            {
                return NameHash.CompareTo(other);
            }
        }

        private readonly IStorage Storage;

        public readonly FileNode[] FileNodes;
        private readonly Memory<byte> NameTable;
        private readonly string NameTableStr;
        private readonly IStorage DataStorage;

        public SarcHeader Header;
        private SfatHeader Sfat;
        private SnftHeader Sfnt;

        public Sarc(IStorage storage)
        {
            Storage = storage;

            using var stream = storage.AsStream();

            stream.Read(BackendUtils.AsSpan(ref Header));

            /* Validate SARC header. */
            if (Header.Bom != 0xFEFF)
                throw new NotImplementedException("Big endian SARCs are not supported!");
            if (Header.Magic != 0x43524153)
                throw new System.IO.InvalidDataException("Invalid SARC magic!");
            if (Header.HeaderSize != Unsafe.SizeOf<SarcHeader>())
                throw new System.IO.InvalidDataException("Invalid SARC header size!");

            DataStorage = Storage.Slice(Header.DataStart);

            stream.Read(BackendUtils.AsSpan(ref Sfat));

            /* Validate SFAT. */
            if (Sfat.Magic != 0x54414653)
                throw new System.IO.InvalidDataException("Invalid SFAT magic!");
            if (Sfat.HeaderSize != Unsafe.SizeOf<SfatHeader>())
                throw new System.IO.InvalidDataException("Invalid SFAT header size!");
            if(Sfat.NodeCount >> 0xE != 0)
                throw new System.IO.InvalidDataException("Invalid SFAT node count!");

            /* Read file nodes. */
            FileNodes = stream.ReadArray<FileNode>(Sfat.NodeCount);

            stream.Read(BackendUtils.AsSpan(ref Sfnt));

            /* Validate SFNT. */
            if (Sfnt.Magic != 0x544E4653)
                throw new System.IO.InvalidDataException("Invalid SNFT magic!");
            if (Sfnt.HeaderSize != Unsafe.SizeOf<SnftHeader>())
                throw new System.IO.InvalidDataException("Invalid SNFT header size!");

            /* Read name table into a string. */
            var nameTableOffset = stream.Position;
            NameTable = new byte[Header.DataStart - nameTableOffset];
            stream.Read(NameTable.Span);

            NameTableStr = Encoding.ASCII.GetString(NameTable.Span);
        }

        public string GetNodeFilename(FileNode node)
        {
            int idx = (int)(node.NameOffset * 4);
            int length = NameTableStr.IndexOf('\0', idx);
            length -= idx;

            if (length < 0)
                length = NameTableStr.Length;

            length = Math.Min(length, NameTableStr.Length-1 - idx);

            return NameTableStr.Substring(idx, length);
        }

        private uint Hash(string str)
        {
            uint hash = 0;

            for(int i = 0; i < str.Length; i++)
                hash = hash * Sfat.HashKey + str[i];

            return hash;
        }

        public int GetNodeIndex(string path)
        {
            var hash = Hash(path);

            return BackendUtils.BinarySearch(FileNodes, hash);
        }

        public IStorage OpenFile(int idx) => OpenFile(FileNodes[idx]);

        public IStorage OpenFile(FileNode node) => DataStorage.Slice(node.FileDataBegin, node.FileDataLength);

    }
}
