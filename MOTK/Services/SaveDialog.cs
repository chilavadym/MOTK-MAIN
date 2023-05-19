using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOTK.Services
{
    public static class SaveDialog
    {
        public static async void Save(string title, string defaultFilename, Action<string> saveAction)
        {
            var filename = await Save(title, defaultFilename);
            if (filename != null)
                saveAction(filename);
        }

        public static async Task<string> Save(string title, string defaultFilename)
        {
            var saveDialog = new SaveFileDialog()
            {
                Title = title,
                Filters = PDFFilters,
                InitialFileName = defaultFilename
            };
            var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            return await saveDialog.ShowAsync(mainWindow);
        }
        public static async Task<string> SaveTSV(string title, string defaultFilename)
        {
            var saveDialog = new SaveFileDialog()
            {
                Title = title,
                Filters = TSVFilters,
                InitialFileName = defaultFilename
            };
            var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            return await saveDialog.ShowAsync(mainWindow);
        }

        private static List<FileDialogFilter> PDFFilters => new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "PDF files (.pdf)", Extensions = new List<string> {"pdf"} },
        new FileDialogFilter { Name = "All files", Extensions = new List<string> {"*"} }
    };
        private static List<FileDialogFilter> TSVFilters => new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "TSV files (.tsv)", Extensions = new List<string> {"tsv"} },
        new FileDialogFilter { Name = "All files", Extensions = new List<string> {"*"} }
    };
    }
}

