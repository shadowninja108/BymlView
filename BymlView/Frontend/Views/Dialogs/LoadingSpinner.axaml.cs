using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BymlView.Frontend.Views.Dialogs
{
    public partial class LoadingSpinner : UserControl
    {
        public string Label { get; set; } = "";

        public LoadingSpinner()
        {
            InitializeComponent();
            DataContext = this;
        }
        public LoadingSpinner(string label)
        {
            Label = label;

            InitializeComponent();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
