using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace BymlView.Backend
{
    public static class BackendUtils
    {
        public static Span<byte> AsSpan<T>(ref T val) where T : unmanaged
        {
            Span<T> valSpan = MemoryMarshal.CreateSpan(ref val, 1);
            return MemoryMarshal.Cast<T, byte>(valSpan);
        }

        public static uint ReadUInt24(this BinaryReader reader)
        {
            /* Read out 3 bytes into a sizeof(uint) buffer. */
            var bytes = new byte[4];
            reader.BaseStream.Read(bytes, 0, 3);

            /* Convert buffer into uint. */
            uint v = BitConverter.ToUInt32(bytes, 0);

            return v;
        }
        public static void WriteUInt24(this BinaryWriter writer, uint value)
        {
            /* Build a byte array from the value. */
            var bytes = new byte[3]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
            };

            /* Write array. */
            writer.BaseStream.Write(bytes);
        }
        public static T[] ReadArray<T>(this Stream stream, uint count) where T : struct
        {
            /* Read data. */
            T[] data = new T[count];

            /* Read into casted span. */
            stream.Read(MemoryMarshal.Cast<T, byte>(data));

            return data;
        }

        public static void WriteArray<T>(this Stream stream, ReadOnlySpan<T> array) where T : struct
        {
            stream.Write(MemoryMarshal.Cast<T, byte>(array));
        }

        public static TemporarySeekHandle TemporarySeek(this Stream stream)
        {
            return stream.TemporarySeek(0, SeekOrigin.Begin);
        }

        public static TemporarySeekHandle TemporarySeek(this Stream stream, long offset, SeekOrigin origin)
        {
            long ret = stream.Position;
            stream.Seek(offset, origin);
            return new TemporarySeekHandle(stream, ret);
        }

        public static int BinarySearch<T, K>(IList<T> arr, K v) where T : IComparable<K>
        {
            var start = 0;
            var end = arr.Count - 1;

            while (start <= end)
            {
                var mid = (start + end) / 2;
                var entry = arr[mid];
                var cmp = entry.CompareTo(v);

                if (cmp == 0)
                    return mid;
                if (cmp > 0)
                    end = mid - 1;
                else /* if (cmp < 0) */
                    start = mid + 1;
            }

            return ~start;
        }
        public static BinaryReader AsBinaryReader(this Stream stream)
        {
            return new BinaryReader(stream);
        }
        public static BinaryWriter AsBinaryWriter(this Stream stream)
        {
            return new BinaryWriter(stream);
        }

    }
}
