using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShackSoundboard
{
    public enum SoundType
    {
        Music,
        Jingle,
        SFX
    }

    public class SoundItem : INotifyPropertyChanged, ICloneable
    {
        private SoundVolume _soundVolume = new SoundVolume(1f);
        private string _displayName = string.Empty;
        private string _imagePath = string.Empty;
        private SoundType _soundType = SoundType.Music;

        public event PropertyChangedEventHandler PropertyChanged;

        public SoundType SoundType
        {
            get
            {
                return _soundType;
            }
            set
            {
                _soundType = value;

                notifyPropertyChanged();
            }
        }

        public string Path { get; set; }

        public SoundVolume Volume
        {
            get
            {
                return _soundVolume;
            }
            set
            {
                _soundVolume = value;
            }
        }

        public float FadeInTime { get; set; }

        public float FadeOutTime { get; set; }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;

                notifyPropertyChanged();
            }
        }

        public string ImagePath
        {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;

                notifyPropertyChanged();
            }
        }

        public SoundInstance CreateSoundInstance()
        {
            return new SoundInstance(this);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void Replace(SoundItem item)
        {
            SoundType = item.SoundType;
            Path = item.Path;
            Volume = item.Volume;
            FadeInTime = item.FadeInTime;
            FadeOutTime = item.FadeOutTime;
            DisplayName = item.DisplayName;
            ImagePath = item.ImagePath;
        }

        private void notifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
