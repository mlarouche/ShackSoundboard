using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

namespace ShackSoundboard
{
    public partial class MainWindow : Window
    {
        private List<SoundItem> _loadedItems = new List<SoundItem>();

        private SoundItem music1 = new SoundItem();
        private SoundItem music2 = new SoundItem();
        private SoundItem sfx = new SoundItem();
        private SoundItem jingle = new SoundItem();

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

            music1.Path = @"C:\Users\micha\Music\Shack_Block1.wav";
            music1.SoundType = SoundType.Music;
            music1.FadeOutTime = 0.5f;
            music2.Path = @"C:\Users\micha\Music\Shack_Block2.wav";
            music2.FadeInTime = 1f;
            music2.SoundType = SoundType.Music;
            sfx.Path = @"C:\Users\micha\Music\Sunsoft.wav";
            sfx.SoundType = SoundType.SFX;
            jingle.Path = @"C:\Users\micha\Music\Guidoune.wav";
            jingle.SoundType = SoundType.Jingle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Init();
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

        private void buttonMusic1_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Play(music1);
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.StopAll();
        }

        private void buttonMusic2_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Play(music2);
        }

        private void buttonJingle_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Play(jingle);
        }

        private void buttonSFX_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.Instance.Play(sfx);
        }
    }
}
