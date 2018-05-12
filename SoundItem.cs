using System.Windows.Media;

namespace ShackSoundboard
{
    enum SoundType
    {
        Music,
        Jingle,
        SFX
    }

    class SoundItem
    {
        private MediaPlayer _mediaInstance = new MediaPlayer();
        private SoundVolume _soundVolume = new SoundVolume(1f);
        private SoundVolume _duckedVolume = new SoundVolume(1f);

        public SoundType SoundType { get; set; }
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

        public SoundVolume DuckedVolume
        {
            get
            {
                return _duckedVolume;
            }
            set
            {
                _duckedVolume = value;
            }
        }

        public float FadeInTime { get; set; }

        public float FadeOutTime { get; set; }

        public string DisplayName { get; set; }

        public string ImagePath { get; set; }

        public SoundInstance CreateSoundInstance()
        {
            return new SoundInstance(this);
        }
    }
}
