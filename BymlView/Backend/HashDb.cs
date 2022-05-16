using LibBlitz.Sead;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BymlView.Backend
{
    public class HashDb
    {
        private static readonly List<uint> Hashes = new();
        private static readonly List<string> Values = new();

        private static readonly EventWaitHandle WaitForLoadHandle = new(false, EventResetMode.AutoReset);

        public static bool LoadCompleted;
        public static bool Load(FileInfo file)
        {
            if (LoadCompleted)
                return false;
            if (!file.Exists)
                return false;

            try
            {
                return LoadImpl(file);
            } catch
            {
                return false;
            }
        }

        private static bool LoadImpl(FileInfo file)
        {
            file = new(@"C:\Users\shado\Downloads\params-filter-nodup.txt");

            using var stream = file.OpenRead();
            using var reader = new StreamReader(stream);

            var lines = File.ReadLines(file.FullName);

            foreach (var line in lines)
            {
                /*
                string[] split = line.Split(",");
                if (split.Length != 2)
                {
                    Console.WriteLine("Malformed line! (Split by ',' failed)");
                    continue;
                }

                var hashStr = split[0].Trim();
                var hashStr = line;
                if (!uint.TryParse(hashStr, System.Globalization.NumberStyles.HexNumber, null, out var hash))
                {
                    Console.WriteLine("Malformed line! (Hash malformed)");
                    continue;
                }

                var value = split[1].Trim();
                if (hash != HashCrc32.CalcStringHash(value))
                {
                    Console.WriteLine("Malformed line! (Hash does not match value)");
                    continue;
                }
                */

                var value = line;
                var hash = HashCrc32.CalcStringHash(value);

                if(!TryAddHash(value, hash))
                {
                    Console.WriteLine("Malformed line! (Duplicate line)");
                    continue;
                }
            }


            LoadCompleted = true;
            WaitForLoadHandle.Set();
            return true;
        }

        public static bool TryAddHash(string value, uint hash)
        {
            var idx = BackendUtils.BinarySearch(Hashes, hash);

            if (idx >= 0)
                return false;

            idx = ~idx;

            Hashes.Insert(idx, hash);
            Values.Insert(idx, value);
            return true;
        }

        public static string? FindByHash(uint hash)
        {
            if(!LoadCompleted)
            {
                WaitForLoadHandle.WaitOne();
            }

            int idx = BackendUtils.BinarySearch(Hashes, hash);
            if (idx < 0)
                return null;
            return Values[idx];
        }
    }
}
