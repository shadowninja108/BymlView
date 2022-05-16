using BymlView.Backend;
using System.IO;

namespace LibBlitz.Lp.Byml.Writer.Primitives
{
    public class BymlNullData : BymlData
    {
        public override BymlNodeId GetTypeCode() => BymlNodeId.Null;
        public override void Write(Stream stream)
        {
            stream.AsBinaryWriter().Write((uint)0);
        }
    }
}
