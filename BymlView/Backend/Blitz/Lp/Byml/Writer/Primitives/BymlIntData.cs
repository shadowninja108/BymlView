using BymlView.Backend;
using System.IO;

namespace LibBlitz.Lp.Byml.Writer.Primitives
{
    public class BymlIntData : BymlData
    {
        private readonly int Value;
        public BymlIntData(int value)
        {
            Value = value;
        }

        public override BymlNodeId GetTypeCode() => BymlNodeId.Int;

        public override void Write(Stream stream)
        {
            stream.AsBinaryWriter().Write(Value);
        }
    }
}
