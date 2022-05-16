using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace BymlView.Common
{
    public static class Utils
    {
        public static DirectoryInfo GetDirectory(this DirectoryInfo dir, string sub)
        {
            return new DirectoryInfo(Path.Combine(dir.FullName, sub));
        }

        public static FileInfo GetFile(this DirectoryInfo dir, string name)
        {
            return new FileInfo(Path.Combine(dir.FullName, name));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignUp(uint num, uint align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AlignUp(int num, int align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AlignUp(ulong num, ulong align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AlignUp(long num, long align)
        {
            return (num + (align - 1)) & ~(align - 1);
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
    }
}
