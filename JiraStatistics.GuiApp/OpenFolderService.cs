using System.Threading.Tasks;
using Avalonia.Controls;

namespace JiraStatistics.GuiApp
{
    public class OpenFolderService
    {
        private readonly Window _window;
        private readonly OpenFolderDialog _openFolderDialog;

        public OpenFolderService(Window window)
        {
            this._window = window;
            this._openFolderDialog = new OpenFolderDialog
            {
                Title = "Select a directory",
            };
        }
        
        public Task<string> SelectDirectory(string defaultDirectory)
        {
            this._openFolderDialog.Directory = defaultDirectory;
            return this._openFolderDialog.ShowAsync(this._window);
        }
    }
}