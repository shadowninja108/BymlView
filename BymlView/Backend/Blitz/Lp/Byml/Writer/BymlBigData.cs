using BymlView.Backend;
using System.IO;

namespace LibBlitz.Lp.Byml.Writer
{
    public abstract class BymlBigData : BymlData
    {
        public int Offset;

        private readonly BymlBigDataList ParentList;

        protected BymlBigData(BymlBigDataList parentList)
        { 
            ParentList = parentList;
            parentList.AddData(this);
        }

        public override void Write(Stream stream)
        {
            stream.AsBinaryWriter().Write(Offset);
        }
        public abstract int CalcBigDataSize();
        public abstract void WriteBigData(Stream stream);

    }
}
