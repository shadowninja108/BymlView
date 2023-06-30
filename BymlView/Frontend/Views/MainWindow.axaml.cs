using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using BymlView.Backend;
using BymlView.Frontend.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BymlView.Frontend.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public static readonly FileInfo DictionaryFileInfo = new("dict.txt");

        public List<ViewModelBase> ChildViewModels => MainTabControl.Items.Cast<TabItem>().Select(x => x.Content).Cast<ViewModelBase>().ToList();
        public ViewModelBase PresentingViewModel => ChildViewModels[ViewModel.SelectedIndex];

        public MainWindow()
        {
            this.WhenActivated(disposables => { /* Handle interactions etc. */ });
            InitializeComponent();

            AddHandler(DragDrop.DropEvent, OnDragDrop);
            DialogMgr.Init(DH);
        }

        static bool DoneInit = false;
        void OnActivated(object? sender, System.EventArgs e)
        {
            /* Ensure this is only ever done once. */
            if (DoneInit)
                return;

            DoneInit = true;

            FrontendUtils.DoLongTask(new()
            {
                Task = () =>
                {
                    return HashDb.Load(DictionaryFileInfo);
                },
                LoadingDelay = 700, /* ms */
                LoadingMessage = "Loading hash db...",
                OnComplete = (succeeded) =>
                {
                    if(!succeeded)
                    {
                        DialogMgr.Show("Failed to load hash db.\nIt may be missing or corrupt.");
                    }
                }
            });
        }

        void OnDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var filenames = e.Data.GetFileNames();
                Debug.Assert(filenames != null);
                PresentingViewModel.OnOpen(filenames);
            }
        }
    }
}
