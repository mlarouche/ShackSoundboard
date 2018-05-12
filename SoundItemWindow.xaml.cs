using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ShackSoundboard
{
    public partial class SoundItemWindow : Window
    {
        public SoundItem EditedSoundItem
        {
            get;
            set;
        }

        public SoundItemWindow()
        {
            InitializeComponent();
        }

        public void SetToAddMode()
        {
            Title = "Add New Sound Item";
            buttonOK.Content = "Add";

            EditedSoundItem = new SoundItem();
        }

        public void SetToEditMode()
        {
            Title = "Edit Sound Item";
            buttonOK.Content = "Edit";

            if (EditedSoundItem != null)
            {
                textBoxName.Text = EditedSoundItem.DisplayName;
                textFilePath.Text = EditedSoundItem.Path;
                textImagePath.Text = EditedSoundItem.ImagePath;
                comboSoundType.SelectedIndex = (int)EditedSoundItem.SoundType;
                sliderVolume.Value = EditedSoundItem.Volume.Linear;
                singleFadeInTime.Value = EditedSoundItem.FadeInTime;
                singleFadeOutTime.Value = EditedSoundItem.FadeOutTime;
                checkBoxIgnoreAutoPlaylist.IsChecked = EditedSoundItem.IgnoreFromAutoPlaylist;
            }
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.DisplayName = textBoxName.Text;
            }
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "WAV file (*.wav)|*.wav";

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                EditedSoundItem.Path = fileDialog.FileName;

                textFilePath.Text = fileDialog.FileName;
            }
        }

        private void buttonBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Images image (*.png, *.jpeg, *.gif)|*.png;*.jpeg;*.gif;*.jpg;*.PNG;*.GIF;*.JPG;*.JPEG";

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                EditedSoundItem.ImagePath = fileDialog.FileName;

                textImagePath.Text = fileDialog.FileName;
            }
        }

        private void comboSoundType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.SoundType = (SoundType)comboSoundType.SelectedIndex;
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.Volume = new SoundVolume((float)sliderVolume.Value);

                labelVolume.Content = $"{EditedSoundItem.Volume.Decibel:F0} dB";
            }
        }

        private void singleFadeInTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.FadeInTime = singleFadeInTime.Value.Value;
            }
        }

        private void singleFadeOutTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.FadeOutTime = singleFadeInTime.Value.Value;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void checkBoxIgnoreAutoPlaylist_Checked(object sender, RoutedEventArgs e)
        {
            if (EditedSoundItem != null)
            {
                EditedSoundItem.IgnoreFromAutoPlaylist = checkBoxIgnoreAutoPlaylist.IsChecked.Value;
            }
        }
    }
}
