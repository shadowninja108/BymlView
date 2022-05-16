using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LibBlitz.Sead;
using System.Collections.Generic;
using System.Linq;

namespace LibBlitz
{
    public static class LpFilesystem
    {
        /* Packs to mount, in the order they're mounted. */
        private static readonly string[] MountPaths =
        {
            "/Pack/Map.pack",
            "/Pack/Mush.release.pack",
            "/Pack/Mush.pack",
            "/Pack/Param.pack",
        };

        public static readonly Sarc[] Mounts = new Sarc[MountPaths.Length];
        public static readonly Dictionary<string, int> PathDictionary = new();
        public static void Initialize()
        {
            var romfs = HosFilesystem.RomFs;
            for(int i = 0; i < MountPaths.Length; i++)
            {
                string mountPath = MountPaths[i];

                /* Don't do anything if the pack doesn't exist. */
                if (!romfs.FileExists(mountPath))
                    continue;

                /* Open SARC. */
                using var file = new UniqueRef<IFile>();
                romfs.OpenFile(ref file.Ref(), (U8Span)mountPath, OpenMode.Read).ThrowIfFailure();
                var sarc = new Sarc(file.Get.AsStorage());

                /* Register SARC as a mount. */
                Mounts[i] = sarc;

                /* Iterate through filepaths and register the mount index. */
                var paths = sarc.FileNodes.Select(x => sarc.GetNodeFilename(x));
                foreach(var path in paths)
                    PathDictionary.Add(path, i);
            }

            /* Populate HOS filepaths */
            var hosPaths = romfs.EnumerateEntries()
                .Where(x => x.Type == DirectoryEntryType.File) /* Only collect files. */
                .Select(x => x.FullPath[1..]); /* Trim leading '/' */

            foreach(var path in hosPaths)
            {
                PathDictionary.TryAdd(path, -1);
            }
        }

        private static IStorage GetStorageFromRomfs(string path)
        {
            /* ResMgr has no starting '/' in paths, while LibHac has a starting '/'. We work out both ahead of time for convenience. */
            string lpPath = path;
            string libhacPath = path;

            if (lpPath[0] == '/')
                lpPath = lpPath[1..];
            if (libhacPath[0] != '/')
                libhacPath = "/" + libhacPath;

            /* Is this file valid? */
            if (!PathDictionary.TryGetValue(lpPath, out int idx))
            {
                /* Invalid path. */
                return null;
            }

            if (idx == -1)
            {
                /* Open file from HOS romfs. */
                using var file = new UniqueRef<IFile>();
                HosFilesystem.RomFs.OpenFile(ref file.Ref(), (U8Span)libhacPath, OpenMode.Read).ThrowIfFailure();
                return file.Get.AsStorage();
            }
            else
            {
                /* Open file from mounted SARC. */
                var sarc = Mounts[idx];
                return sarc.OpenFile(sarc.GetNodeIndex(lpPath));
            }
        }

        public static IStorage OpenRomfs(string path)
        {
            var storage = GetStorageFromRomfs(path);
            if (storage == null)
                return null;

            /* Check if the file is nisasyst encrypted. */
            if (Nisasyst.IsNisasyst(storage))
            {
                /* Decrypt file. */
                storage = new MemoryStorage(Nisasyst.Decrypt(path, storage));
            }

            /* Is this file a SZS? */
            if (path.EndsWith(".szs"))
            {
                /* The game assumes all .szs files are Yaz0 compressed. */

                /* Decompress file. */
                storage = new MemoryStorage(Yaz0.Decompress(storage));
            }

            return storage;
        }
    }
}
