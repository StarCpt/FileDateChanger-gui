using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using LurkingMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace filedatechangergui
{
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        public bool ChangeCreationTime
        {
            get => changeCreationTime;
            set
            {
                if (SetValue(ref changeCreationTime, value))
                {
                    RaisePropertyChanged(nameof(EnableApplyChange));
                }
            }
        }

        public bool ChangeModifiedTime
        {
            get => changeModifiedTime;
            set
            {
                if (SetValue(ref changeModifiedTime, value))
                {
                    RaisePropertyChanged(nameof(EnableApplyChange));
                }
            }
        }

        public bool ChangeAccessedTime
        {
            get => changeAccessedTime;
            set
            {
                if (SetValue(ref changeAccessedTime, value))
                {
                    RaisePropertyChanged(nameof(EnableApplyChange));
                }
            }
        }

        public DateTime NewDate
        {
            get => newDate;
            set => SetValue(ref newDate, value);
        }

        public TimeSpan NewTime
        {
            get => newTime;
            set => SetValue(ref newTime, value);
        }

        public ThemeVariant[] AvailableThemes { get; } = new ThemeVariant[]
        {
            //ThemeVariant.Default,
            ThemeVariant.Dark,
            ThemeVariant.Light,
        };

#pragma warning disable CS8602, CS8603
        public ThemeVariant CurrentTheme
        {
            get => App.Current.RequestedThemeVariant;
            set
            {
                if (App.Current.RequestedThemeVariant != value)
                {
                    App.Current.RequestedThemeVariant = value;
                    RaisePropertyChanged(nameof(CurrentTheme));
                }
            }
        }
#pragma warning restore CS8602, CS8603

        public bool EnableApplyChange
        {
            get => (ChangeCreationTime || ChangeModifiedTime || ChangeAccessedTime) && SelectedFiles.Count > 0;
        }

        public bool EnableBrowseFiles
        {
            get => enableBrowseFiles;
            set => SetValue(ref enableBrowseFiles, value);
        }

        public ObservableCollection<IStorageFile> SelectedFiles
        {
            get => selectedFiles;
            //set => SetValue(ref selectedFiles, value);
        }

        public ICommand BrowseFilesCommand { get; }
        public ICommand ApplyChangesCommand { get; }

        bool changeCreationTime;
        bool changeModifiedTime;
        bool changeAccessedTime;
        DateTime newDate;
        TimeSpan newTime;
        bool enableBrowseFiles;
        ObservableCollection<IStorageFile> selectedFiles;

        public MainWindowViewModel()
        {
            changeCreationTime = false;
            changeModifiedTime = false;
            changeAccessedTime = false;
            newDate = DateTime.Now.Date;
            newTime = DateTime.Now.TimeOfDay;
            enableBrowseFiles = true;
            selectedFiles = new ObservableCollection<IStorageFile>();

            BrowseFilesCommand = new Command(ExecuteBrowseFilesCommand);
            ApplyChangesCommand = new Command(ExecuteApplyChangesCommand);

            SelectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
        }

        private void SelectedFiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(EnableApplyChange));
        }

        public void ExecuteBrowseFilesCommand(object? param)
        {
            if (param is not Window win)
            {
                return;
            }

            EnableBrowseFiles = false;

            if (!win.StorageProvider.CanOpen)
            {
                ShowErrorDialogAsync("Cannot open the file picker dialog", win, delegate { EnableBrowseFiles = true; });
                return;
            }

            var options = new FilePickerOpenOptions
            {
                AllowMultiple = true,
                //Title = "Browse Files",
            };
            win.StorageProvider.OpenFilePickerAsync(options)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        EnableBrowseFiles = true;
                        var result = task.Result;
                        if (result.Count > 0)
                        {
                            SelectedFiles.Clear();
                            foreach (var item in result)
                            {
                                SelectedFiles.Add(item);
                            }
                        }
                    }
                    else
                    {
                        ShowErrorDialogAsync("File picker task did not complete successfully", win, delegate { EnableBrowseFiles = true; });
                    }
                });
        }

        public void ExecuteApplyChangesCommand(object? param)
        {
            foreach (var file in SelectedFiles)
            {
                string? path = file.TryGetLocalPath();
                if (!File.Exists(path))
                    continue;

                FileInfo handle = new FileInfo(path);

                if (ChangeCreationTime)
                    handle.CreationTime = NewDate + NewTime;
                if (ChangeModifiedTime)
                    handle.LastWriteTime = NewDate + NewTime;
                if (ChangeAccessedTime)
                    handle.LastAccessTime = NewDate + NewTime;
            }
        }

        static void ShowErrorDialogAsync(string message, Window owner, TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs>? callback = null)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                Margin = new Thickness(0, 32, 0, 0),
            };

            if (callback != null)
            {
                dialog.Closing += callback;
            }

            dialog.ShowAsync(owner);

        }
    }
}
