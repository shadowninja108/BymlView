namespace BymlView.Frontend.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public int SelectedIndex { get; set; }

        public BymlEditorViewModel BymlEditor { get; set; } = new();

        public MainWindowViewModel()
        {

        }

    }
}
