using System;
using DH = DialogHost.DialogHost;

namespace BymlView.Frontend
{
    public class DialogMgr
    {
        private const string Identifier = "Main";

        private static DH MainDH;

        private static Action? OnClosedCallback;

        public static bool IsOpen => DH.IsDialogOpen(Identifier);

        private static void OnClosedImpl(object sender, DialogHost.DialogOpenedEventArgs eventArgs)
        {
            OnClosedCallback?.Invoke();

            OnClosedCallback = null;
        }

        public static void Init(DH dh)
        {
            MainDH = dh;
        }
        
        public struct DialogPresentArg
        {
            public object Value;
            public bool CanDismiss;
            public Action? ClosedCallback;

            public DialogPresentArg()
            {
                Value = null;
                CanDismiss = true;
                ClosedCallback = null;
            }
        }

        public static void Show(object model)
        {
            Show(new DialogPresentArg() { Value = model });
        }

        public static void Show(object model, Action onClosedCallback)
        {
            Show(new DialogPresentArg() { Value = model, ClosedCallback = onClosedCallback });
        }

        public static void Show(object model, bool canDismiss, Action closedCallback = null)
        {
            Show(new DialogPresentArg() { Value = model, CanDismiss = canDismiss, ClosedCallback = closedCallback});
        }

        public static void Show(DialogPresentArg arg)
        {
            MainDH.CloseOnClickAway = arg.CanDismiss;
            OnClosedCallback = arg.ClosedCallback;
            DH.Show(arg.Value, OnClosedImpl);
        }

        public static void Close()
        {
            DH.Close(Identifier);
        }
    }
}
