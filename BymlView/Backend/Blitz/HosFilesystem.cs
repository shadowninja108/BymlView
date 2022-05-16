using LibHac.Fs.Fsa;
using System;
using System.IO;
using System.Linq;
using LibHac.FsSystem;
using LibHac.Common.Keys;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.FsSystem;
using BymlView.Common;

namespace LibBlitz
{
    public class HosFilesystem
    {
        /* Paths for loading keysets. */
        private static readonly DirectoryInfo UserProfileDirectoryInfo = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        private static readonly DirectoryInfo SwitchDirectoryInfo = UserProfileDirectoryInfo.GetDirectory(".switch");
        private static readonly FileInfo ProdKeysPath = SwitchDirectoryInfo.GetFile("prod.keys");
        private static readonly FileInfo TitleKeysPath = SwitchDirectoryInfo.GetFile("title.keys");

        private static readonly KeySet Keyset = new();

        static HosFilesystem()
        {
            /* Check keys are actually present. */
            if (!ProdKeysPath.Exists)
            {
                Console.WriteLine($"Expects keyset at {ProdKeysPath.FullName}");
                Environment.Exit(0);
            }

            if (!TitleKeysPath.Exists)
            {
                Console.WriteLine($"Expects keyset at {TitleKeysPath.FullName}");
                Environment.Exit(0);
            }

            /* Load our keys. */
            ExternalKeyReader.ReadKeyFile(Keyset, ProdKeysPath.FullName, TitleKeysPath.FullName, null, null);
        }

        /* All content IDs of known anticheat protected applications. */
        public enum BlitzContentId : ulong
        {
            /* Retail games. */
            JP = 0x01003C700009C000,
            US = 0x01003BC0000A0000,
            EU = 0x0100F8F0000A2000,

            /* Special demo. */
            TrialJP = 0x01009C900D458000,
            TrialUS = 0x01006BB00D45A000,
            TrialEU = 0x01007E200D45C000,

            /* 2020 special demo. */
            TrialJP20 = 0x0100998011330000,
            TrialUS20 = 0x01002120116C4000,
            TrialEU20 = 0x01007E200D45C000, /* TODO: check this is right? */
        };

        private static bool IsRetail(BlitzContentId id) => id == BlitzContentId.JP || id == BlitzContentId.US || id == BlitzContentId.EU;

        private static SwitchFs BaseFs, UpdateFs;
        private static Application BaseApp, UpdateApp;

        private static Nca BaseNca => BaseApp?.Main?.MainNca?.Nca;
        private static Nca UpdateNca => UpdateApp?.Patch?.MainNca?.Nca;

        public static IFileSystem RomFs, ExeFs;

        public static bool InitializeFromNcas(IFileSystem baseFs, IFileSystem updateFs)
        {
            BaseFs = SwitchFs.OpenNcaDirectory(Keyset, baseFs);

            if(updateFs != null)
                UpdateFs = SwitchFs.OpenNcaDirectory(Keyset, updateFs);

            if (!FindNcas())
                return false;
            OpenFileSystems();

            return true;
        }

        private static bool FindNcas()
        {
            if(BaseFs.Applications.Count < 1)
            {
                Console.WriteLine("No applications found in base.");
                return false;
            }

            /* Find Blitz content IDs where base is present. */
            BlitzContentId[] applicableBases = BaseFs.Applications.Keys
                .Where(x => Enum.IsDefined(typeof(BlitzContentId), x)) /* Only take valid Blitz content IDs. */
                .Cast<BlitzContentId>()
                .ToArray();


            if (applicableBases.Length < 1)
            {
                Console.WriteLine("Could not find any content to analyze.");
                return false;
            }

            /* TODO: How do we handle when several content IDs are present? */
            BlitzContentId pickedApp = applicableBases[0];
            BaseApp = BaseFs.Applications[(ulong)pickedApp];

            /* Extra validation for retail game. */
            if (IsRetail(pickedApp))
            {
                if (UpdateFs == null)
                {
                    Console.WriteLine("Update is needed for retail games.");
                    return false;
                }

                UpdateApp = UpdateFs.Applications[(ulong)pickedApp];

                /* Make sure we can get the NCA. */
                if (UpdateNca == null)
                {
                    Console.WriteLine("Could not find update NCA.");
                    return false;
                }
            }

            /* Make sure we can get the NCA. */
            if (BaseNca == null)
            {
                Console.WriteLine("Could not find base NCA.");
                return false;
            }

            return true;
        }

        private static void OpenFileSystems()
        {
            ExeFs = OpenFileSystem(NcaSectionType.Code);
            RomFs = OpenFileSystem(NcaSectionType.Data);
        }

        private static IFileSystem OpenFileSystem(NcaSectionType type)
        {
            if (UpdateNca != null)
                return BaseNca.OpenFileSystemWithPatch(UpdateNca, type, IntegrityCheckLevel.ErrorOnInvalid);

            return BaseNca.OpenFileSystem(type, IntegrityCheckLevel.ErrorOnInvalid);
        }
    }
}
