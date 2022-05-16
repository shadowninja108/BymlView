using ReactiveUI;
using System.Collections.Generic;
using BymlView.Common;
using System.Reactive.Disposables;

namespace BymlView.Frontend.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public virtual void OnCopy() { }
        public virtual void OnCut() { }
        public virtual void OnPaste() { }
        public virtual void OnSave() { }
        public virtual void OnOpen(string path) { }
        public virtual void OnOpen(IEnumerable<string> paths) { paths.ForEach(x => OnOpen(x)); }
        public virtual void OnClose(object node) { }

        public ViewModelBase()
        {
            this.WhenActivated(disposables =>
            {
                /* Handle activation */
                Disposable
                    .Create(() => { /* Handle deactivation */ })
                    .DisposeWith(disposables);
            });
        }
    }
}
