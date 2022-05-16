using BymlView.Backend;
using LibHac.Fs;
using LibHac.Tools.FsSystem;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibBlitz.Sead
{
    public class Yaz0
    {
        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        private struct Yaz0Header
        {
            public uint Magic;
            public uint DecompressedSize;
            public uint DecompressedAlignment;
            public uint Reserved;
        }

        /* Yaz0 backwards, because it's big endian. */
        private const uint Magic = 0x307A6159;

        public static byte[] Decompress(IStorage storage)
        {
            Yaz0Header header = new();
            storage.Read(0, BackendUtils.AsSpan(ref header));

            if (header.Magic != Magic)
                return null;

            /* Reverse because it's big endian. */
            header.DecompressedSize = ReverseBytes(header.DecompressedSize);
            header.DecompressedAlignment = ReverseBytes(header.DecompressedAlignment);

            /* Get storage to the body of the data. */
            var body = storage.Slice(Unsafe.SizeOf<Yaz0Header>());
            body.GetSize(out var bodySize).ThrowIfFailure();
            BinaryReader inputReader = new(body.AsStream());

            byte[] output = new byte[header.DecompressedSize];

            uint dst = 0;
            byte groupHeader = 0;
            int chunksLeft = 0;

            while(inputReader.BaseStream.Position < bodySize && dst < header.DecompressedSize)
            {
                if(chunksLeft == 0)
                {
                    groupHeader = inputReader.ReadByte();
                    chunksLeft = 8;
                }

                if((groupHeader & 0x80) == 0x80)
                {
                    output[dst++] = inputReader.ReadByte();
                } 
                else
                {
                    var pair = ReverseBytes(inputReader.ReadUInt16());

                    var distance = (pair & 0x0FFF) + 1;
                    var length = ((pair >> 12) != 0 ? (pair >> 12) : (inputReader.ReadByte() + 16)) + 2;

                    var b = dst - distance;

                    if (b < 0 || dst + length > header.DecompressedSize)
                        throw new InvalidDataException("Corrupt data!");

                    while (length-- > 0)
                        output[dst++] = output[b++];
                }

                groupHeader <<= 1;
                chunksLeft--;
            }

            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ReverseBytes(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static bool IsYaz0(IStorage storage)
        {
            Yaz0Header header = new();
            storage.Read(0, BackendUtils.AsSpan(ref header));

            return header.Magic == Magic;
        }
    }
}
