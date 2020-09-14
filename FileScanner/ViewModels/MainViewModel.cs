using FileScanner.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;

namespace FileScanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string selectedFolder;
        private ObservableCollection<string> folderItems = new ObservableCollection<string>();
         
        public DelegateCommand<string> OpenFolderCommand { get; private set; }
        public DelegateCommand<string> ScanFolderCommand { get; private set; }

        public ObservableCollection<string> FolderItems { 
            get => folderItems;
            set 
            { 
                folderItems = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                OnPropertyChanged();
                ScanFolderCommand.RaiseCanExecuteChanged();
            }
        }

        public MainViewModel()
        {
            OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
            ScanFolderCommand = new DelegateCommand<string>(ScanFolderAsync, CanExecuteScanFolder);
        }

        private bool CanExecuteScanFolder(string obj)
        {
            return !string.IsNullOrEmpty(SelectedFolder);
        }

        private void OpenFolder(string obj)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolder = fbd.SelectedPath;
                }
            }
        }

        private void ScanFolder(string dir)
        {
            FolderItems = new ObservableCollection<string>(GetDirs(dir));

            foreach (var item in Directory.EnumerateFiles(dir, "*"))
            {
                FolderItems.Add(item);
            }
        }

        private async void ScanFolderAsync(string dir)
        {
            FolderItems = await Task.Run(() => new ObservableCollection<string>(GetDirs(dir)));

            foreach (var item in Directory.EnumerateFiles(dir, "*"))
            {
                FolderItems.Add(item);
            }
        }

        IEnumerable<string> GetDirs(string dir)
        {
            IEnumerable<string> directories = null;
            IEnumerable<string> files = null;
            
            try
            {
                directories = Directory.EnumerateDirectories(dir, "*");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e);
            }

            foreach (var d in directories)
            {
                yield return d;

                try
                {
                    files = Directory.EnumerateFiles(d, "*");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e);
                }

                foreach (var f in files)
                {
                    yield return f;
                }
            }
        }

        ///TODO : Tester avec un dossier avec beaucoup de fichier
        ///TODO : Rendre l'application asynchrone
        ///TODO : Ajouter un try/catch pour les dossiers sans permission
    }
}
