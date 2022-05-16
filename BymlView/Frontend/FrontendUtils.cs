using Avalonia.Threading;
using BymlView.Frontend.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BymlView.Frontend
{
    public static class FrontendUtils
    {
        public static Task DoOnUIThread(Action action)
        {
            return Dispatcher.UIThread.InvokeAsync(action);
        }

        public struct LongTaskArg
        {
            public Func<bool> Task;
            public Action<bool>? OnComplete;
            public string LoadingMessage;
            public int LoadingDelay;

            public bool PresentLoadingImmediately => LoadingDelay <= 0;
            public bool LoadingDelayed => LoadingDelay > 0;
        }

        /* TODO: work on the possible race conditions that could arise... */
        public static void DoLongTask(LongTaskArg arg)
        {
            void PresentSpinner()
            {
                DoOnUIThread(() =>
                {
                    LoadingSpinner s = new(arg.LoadingMessage);
                    DialogMgr.Show(s, false);
                });
            }

            /* Startup background task. */
            var task = Task.Run(async () =>
            {
                /* Present the spinner if it's set to immediately appear. */
                if(arg.PresentLoadingImmediately)
                {
                    PresentSpinner();
                }

                /* Do task. */
                var r = arg.Task();

                /* Move over to UI thread to check on the dialog. */
                await DoOnUIThread(() =>
                {
                    /* Close open dialog if needed. */
                    if (DialogMgr.IsOpen)
                        DialogMgr.Close();

                    /* Signal that the task completed. */
                    arg.OnComplete?.Invoke(r);
                });
            });

            /* Only bother with this second task if the spinner needs delayed. */
            if (arg.LoadingDelayed)
            {
                /* Setup task to check later if the hash db finished loading. If not, present a spinner. */
                Task.Delay(arg.LoadingDelay).ContinueWith((_) =>
                {
                    /* See if the task completed already. */
                    if (!task.IsCompleted)
                    {
                        /* Present spinner since the task is taking too much time. */
                        PresentSpinner();
                    }
                });
            }
        }
    }
}
