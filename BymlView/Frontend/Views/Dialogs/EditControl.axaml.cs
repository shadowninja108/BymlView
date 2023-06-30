using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BymlView.Frontend.Views.Dialogs
{
    public partial class EditControl : UserControl
    {
        private string Message { get; set; }

        public EditControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EditControl(string message)
        {
            Message = message;

            InitializeComponent();
            DataContext = this;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
