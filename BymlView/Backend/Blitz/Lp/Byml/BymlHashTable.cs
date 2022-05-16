using BymlView.Backend;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibBlitz.Lp.Byml
{
    public class BymlHashTable : IBymlNode
    {
        public BymlNodeId Id => BymlNodeId.Hash;

        public readonly BymlHashPair[] Pairs;

        public IBymlNode this[string key]
        {
            get {
                var idx = BackendUtils.BinarySearch(Pairs, key);
                if (idx < 0)
                    return null;
                return Pairs[idx].Value;
            }
        }

        public IEnumerable<string> Keys => Pairs.Select(x => x.Name);
        public IEnumerable<IBymlNode> Values => Pairs.Select(x => x.Value);

        public BymlHashTable(Byml by, Stream stream)
        {
            BinaryReader reader = new(stream);
            uint count = reader.ReadUInt24();

            Pairs = new BymlHashPair[count];
            for (int i = 0; i < count; i++)
            {
                var name = reader.ReadUInt24();
                var id = Byml.ReadNodeId(stream);
                if (!id.HasValue)
                    throw new InvalidDataException($"Invalid BYML node ID!");

                BymlHashPair entry = new()
                {
                    Id = id.Value,
                    Name = by.GetFromHashKeyTable(name),
                    Value = by.ReadNode(reader, id.Value),
                };

                Pairs[i] = entry;
            }
        }
    }
}
