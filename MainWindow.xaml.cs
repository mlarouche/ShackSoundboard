using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.IO;

namespace ShackSoundboard
{
    public class SoundTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SoundType soundType = (SoundType)value;

            switch (soundType)
            {
                case SoundType.Music:
                    return new LinearGradientBrush(Colors.Green, Colors.White, 90.0);
                case SoundType.Jingle:
                    return new LinearGradientBrush(Colors.Magenta, Colors.White, 90.0);
                case SoundType.SFX:
                    return new LinearGradientBrush(Colors.Yellow, Colors.White, 90.0);
            }

            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        const string RegKey = "Software/BaroqueLarouche/ShackSoundboard";
        const string RegLastPath = "LastPath";

        private ObservableCollection<SoundItem> _loadedItems = new ObservableCollection<SoundItem>();
        private string _lastOpenedPath = string.Empty;

        private JsonSerializer _serializer;
        private JsonSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new JsonSerializer();
                    _serializer.NullValueHandling = NullValueHandling.Ignore;
                    _serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
                    _serializer.Formatting = Formatting.Indented;
                    _serializer.TypeNameHandling = TypeNameHandling.All;
                    _serializer.Converters.Add(new StringEnumConverter());
                }

                return _serializer;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Init();

            var regKey = Registry.CurrentUser.CreateSubKey(RegKey);
            if (regKey != null)
            {
                var lastPath = regKey.GetValue(RegLastPath) as string;
                if (lastPath != null)
                {
                    loadSettings(lastPath);
                }
            }

            buttonList.ItemsSource = _loadedItems;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        public void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _loadedItems.Clear();
            _lastOpenedPath = string.Empty;

            buttonList.ItemsSource = _loadedItems;
        }

        public void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Soundboard Settings (*.json)|*.json";

            var result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                loadSettings(fileDialog.FileName);

                var regKey = Registry.CurrentUser.CreateSubKey(RegKey);
                if (regKey != null)
                {
                    regKey.SetValue(RegLastPath, fileDialog.FileName);
                }
            }
        }

        public void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_lastOpenedPath))
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.Filter = "Soundboard Settings (*.json)|*.json";

                var result = fileDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    saveSettings(fileDialog.FileName);
                }
            }
            else
            {
                saveSettings(_lastOpenedPath);
            }
        }

        public void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Soundboard Settings (*.json)|*.json";

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                saveSettings(fileDialog.FileName);
            }
        }

        private void loadSettings(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    object readObject = Serializer.Deserialize(reader) as ObservableCollection<SoundItem>;

                    if (readObject != null)
                    {
                        _loadedItems = (ObservableCollection<SoundItem>)readObject;

                        buttonList.ItemsSource = _loadedItems;

                        _lastOpenedPath = path;
                    }
                }
            }
        }

        private void saveSettings(string path)
        {
            if (System.IO.Path.GetExtension(path) != ".json")
            {
                path = System.IO.Path.ChangeExtension(path, ".json");
            }

            string directory = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        Serializer.Serialize(writer, _loadedItems);
                    }
                }

                var regKey = Registry.CurrentUser.CreateSubKey(RegKey);
                if (regKey != null)
                {
                    regKey.SetValue(RegLastPath, path);
                }
            }
            catch (Exception)
            {
            }
        }

        private void sliderMainVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (labelMainVolume != null)
            {
                SoundManager.Instance.MasterVolume = new SoundVolume((float)e.NewValue);

                labelMainVolume.Content = $"{SoundManager.Instance.MasterVolume.Decibel:F0} dB";

                SoundManager.Instance.UpdateAllVolumes();
            }
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.StopAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                SoundItem selectedItem = button.DataContext as SoundItem;
                if (selectedItem != null)
                {
                    bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl);

                    if (!isCtrlDown && selectedItem.SoundType == SoundType.Music 
                        && SoundManager.Instance.CurrentMusic?.Item == selectedItem
                        && SoundManager.Instance.IsPlaying(selectedItem))
                    {
                        SoundManager.Instance.Toggle(selectedItem);
                    }
                    else
                    {
                        bool forceStop = false;

                        if (selectedItem.SoundType == SoundType.Music)
                        {
                            if (SoundManager.Instance.CurrentMusic?.Item == selectedItem)
                            {
                                forceStop = true;
                            }
                        }

                        SoundManager.Instance.Play(selectedItem, forceStop);
                    }
                }
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            SoundItemWindow itemWindow = new SoundItemWindow();
            itemWindow.SetToAddMode();

            var result = itemWindow.ShowDialog();

            if (result.HasValue && result.Value)
            {
                _loadedItems.Add(itemWindow.EditedSoundItem);
            }
        }

        private void menuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Button_Edit(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                SoundItem selectedItem = menuItem.DataContext as SoundItem;
                if (selectedItem != null)
                {
                    SoundItemWindow editWindow = new SoundItemWindow();
                    editWindow.EditedSoundItem = (SoundItem)selectedItem.Clone();
                    editWindow.SetToEditMode();

                    var result = editWindow.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        selectedItem.Replace(editWindow.EditedSoundItem);
                    }
                }
            }
        }

        private void Button_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                SoundItem selectedItem = menuItem.DataContext as SoundItem;
                if (selectedItem != null)
                {
                    _loadedItems.Remove(selectedItem);
                }
            }
        }
    }
}
