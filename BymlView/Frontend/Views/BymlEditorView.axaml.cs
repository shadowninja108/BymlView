using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using BymlView.Frontend.Models;
using BymlView.Frontend.ViewModels;
using BymlView.Frontend.Views.Dialogs;
using ReactiveUI;

namespace BymlView.Frontend.Views
{
    public partial class BymlEditorView : ReactiveUserControl<BymlEditorViewModel>
    {
        public BymlEditorView()
        {
            this.WhenActivated(disposables => { /* Handle interactions etc. */ });
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void OnClose(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var node = menuItem?.DataContext as BymlNodeAdapter;

            if (node == null)
                return;

            ViewModel?.OnClose(node);
        }

        void OnCopy(object sender, RoutedEventArgs e)
        {
            var textblock = sender as TextBlock;

            if (textblock == null)
                return;

            /* TODO */
            ViewModel?.OnCopy();
            Application.Current?.Clipboard?.SetTextAsync(textblock.Text);
        }

        private void OnEditValue(object? sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem == null)
                return;

            var node = menuItem.DataContext as BymlNodeAdapter;


            DialogMgr.Show(new EditControl("Hello!"), true);
            ;
        }

        private void OnEditName(object? sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem == null)
                return;

            var node = menuItem.DataContext as BymlNodeAdapter;

            DialogMgr.Show(new EditControl("Hello!"), true);
            ;
        }
    }
}
