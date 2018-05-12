using System;
using System.Windows.Media;

namespace ShackSoundboard
{
    class SoundInstance
    {
        private SoundItem _item;
        private MediaPlayer _player = new MediaPlayer();
        private SoundVolume _instanceVolume = new SoundVolume();

        public event Action<SoundInstance> OnCompleted;

        public SoundItem Item
        {
            get
            {
                return _item;
            }
        }

        public MediaPlayer Player
        {
            get
            {
                return _player;
            }
        }

        public SoundVolume Volume
        {
            get
            {
                return _instanceVolume;
            }
            set
            {
                _instanceVolume = value;

                App.Current.Dispatcher.Invoke(() => {
                    _player.Volume = value.Linear;
                });
            }
        }

        public SoundType Type
        {
            get
            {
                return Item.SoundType;
            }
        }

        public SoundInstance(SoundItem item)
        {
            _item = item;
            _player.MediaEnded += mediaEnded;
        }

        private void mediaEnded(object sender, EventArgs e)
        {
            OnCompleted?.Invoke(this);
        }

        public void Play()
        {
            _player.Open(new Uri(Item.Path));
            _player.Play();
        }

        public void Stop()
        {
            _player.Stop();
        }

        public void Resume()
        {
            _player.Play();
        }

        public void Pause()
        {
            _player.Pause();
        }
    }
}
