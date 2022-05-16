using BymlView.Common;
using BymlView.Frontend.Models;
using LibBlitz.Lp.Byml;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace BymlView.Frontend.ViewModels
{
    public class BymlEditorViewModel : ViewModelBase
    {
        public ObservableCollection<BymlNodeAdapter> Items { get; } = new();
        public BymlNodeAdapter? SelectedItem { get; set; }

        private void OpenImpl(string path)
        {
            FileInfo fi = new(path);

            if (!fi.Exists)
                return;

            var s = new LocalFile(fi.FullName, LibHac.Fs.OpenMode.Read).AsStorage();
            Byml by = new(s);

            Items.Add(new(by, fi.Name));

            // var fit = new FileInfo(@"C:\Users\shado\Downloads\test.byml");
            // BymlWriter writer = new();
            // 
            // writer.PushIter(by.Root);

        }

        public override void OnOpen(string path)
        {
            FrontendUtils.DoLongTask(new()
            {
                Task = () =>
                {
                    OpenImpl(path);
                    return true;
                },
                LoadingMessage = "Loading file...",
                LoadingDelay = 700
            });
        }

        public override void OnOpen(IEnumerable<string> paths)
        {
            FrontendUtils.DoLongTask(new()
            {
                Task = () =>
                {
                    paths.ForEach(x => OpenImpl(x));
                    return true;
                },
                LoadingMessage = "Loading files...",
                LoadingDelay = 700
            });
        }

        public override void OnClose(object node)
        {
            if (node is not BymlNodeAdapter n)
                throw new Exception("Invalid node OnClose.");

            Items.Remove(n);
        }
    }
}
