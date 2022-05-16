using System.IO;

namespace LibBlitz.Lp.Byml.Writer
{
    public interface IBymlData
    {
        void MakeIndex();
        int CalcPackSize();
        BymlNodeId GetTypeCode();
        bool IsContainer();
        void Write(Stream stream);
    }
}
