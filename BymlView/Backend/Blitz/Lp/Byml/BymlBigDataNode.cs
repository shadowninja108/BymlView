using System;
using System.IO;
using LibBlitz.Lp.Byml;

namespace BymlView.Backend.Blitz.Lp.Byml
{
    public class BymlBigDataNode<T> : IBymlNode
    {
        public BymlNodeId Id { get; }
        public T Value { get; }

        public BymlBigDataNode(BymlNodeId id, BinaryReader reader, Func<BinaryReader, T> callback)
        {
            Id = id;
            using (reader.BaseStream.TemporarySeek(reader.ReadUInt32(), SeekOrigin.Begin))
            {
                Value = callback(reader);
            }
        }
    }
}
