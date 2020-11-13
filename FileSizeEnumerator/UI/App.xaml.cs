using System.Diagnostics;
using System.Text;
using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileSizeEnumerator.UI
{
    public partial class App : Application
    {
        #region Methods

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Log(2, "Worker", e.Exception.ToString());
                return;
            }

            StringBuilder message = new StringBuilder();
            message.AppendLine($"Type: {e.Exception.GetType().FullName}");
            message.AppendLine($"Message: {e.Exception.Message}");
            message.Append($"StackTrace: {e.Exception.StackTrace}");

            using var dialog = new TaskDialog()
            {
                Cancelable = false,
                Caption = "File Size Enumerator",
                InstructionText = "Error",
                DefaultButton = TaskDialogDefaultButton.Ok,
                DetailsExpanded = false,
                ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter,
                Icon = TaskDialogStandardIcon.Error,
                StartupLocation = TaskDialogStartupLocation.CenterScreen,
                HyperlinksEnabled = false,
                StandardButtons = TaskDialogStandardButtons.Ok,
                Text = "An unhandled exception was thrown and application must be closed.",
                DetailsCollapsedLabel = "More details",
                DetailsExpandedLabel = "Less details",
                DetailsExpandedText = message.ToString()
            };

            dialog.Show();

            dialog.Closing += (sender, e) => Shutdown(-1);
        }

        #endregion
    }
}
