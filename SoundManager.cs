using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShackSoundboard
{
    class FadeRequest
    {
        private float _timer = 0f;

        public float Duration { get; set; }

        public SoundVolume SourceVolume { get; set; }
        public SoundVolume TargetVolume { get; set; }

        public SoundInstance Instance { get; set; }

        public bool RemoveInstance { get; set; }

        public bool IsDone
        {
            get
            {
                return _timer >= Duration;
            }
        }

        public void Update(float deltaTime)
        {
            Instance.Volume = new SoundVolume(Lerp(SourceVolume.Linear, TargetVolume.Linear, _timer / Duration));
            Instance.Volume *= SoundManager.Instance.MasterVolume;

            _timer += deltaTime;
        }

        private float Lerp(float a, float b, float dt)
        {
            return a + (b - a) * dt;
        }
    }

    class SoundManager
    {
        private static readonly SoundManager _instance = new SoundManager();

        private TimeSpan _lastTimestamp = new TimeSpan(Stopwatch.GetTimestamp());
        private Thread _updateThread;
        private object _mutex = new object();

        private SoundVolume _masterVolume = new SoundVolume(1f);
        private SoundInstance _currentMusic;
        private List<SoundInstance> _activeInstances = new List<SoundInstance>();
        private List<FadeRequest> _fadeRequests = new List<FadeRequest>();

        public static SoundManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public SoundVolume MasterVolume
        {
            get
            {
                return _masterVolume;
            }
            set
            {
                _masterVolume = value;
            }
        }

        private SoundManager()
        {
        }

        static SoundManager()
        {
        }

        public void Init()
        {
            _updateThread = new Thread(UpdateThread);
            _updateThread.Priority = ThreadPriority.Normal;
            _updateThread.Start();
        }

        public void Play(SoundItem item)
        {
            var instance = item.CreateSoundInstance();
            instance.OnCompleted += onInstanceCompleted;
            instance.Volume = MasterVolume * item.Volume;

            switch (item.SoundType)
            {
                case SoundType.SFX:
                    {
                        if (item.FadeOutTime > 0f)
                        {
                            createFadeInRequest(instance);
                        }

                        instance.Play();
                        break;
                    }
                case SoundType.Music:
                    {
                        if (_currentMusic != null)
                        {
                            if (_currentMusic.Item.FadeOutTime > 0f)
                            {
                                createStopFadeOutRequest(_currentMusic);
                            }
                            else
                            {
                                _currentMusic.Stop();

                                for (int i = _activeInstances.Count - 1; i >= 0; --i)
                                {
                                    if (_activeInstances[i].Item == item)
                                    {
                                        _activeInstances.RemoveAt(i);
                                    }
                                }
                            }
                        }

                        if (item.FadeInTime > 0f)
                        {
                            createFadeInRequest(instance);
                        }

                        _currentMusic = instance;
                        _currentMusic.Play();
                        break;
                    }
                case SoundType.Jingle:
                    {
                        for (int i = _activeInstances.Count - 1; i >= 0; --i)
                        {
                            var activeInstance = _activeInstances[i];
                            if (activeInstance.Item == item)
                            {
                                if (activeInstance.Item.FadeOutTime > 0f)
                                {
                                    createStopFadeOutRequest(activeInstance);
                                }
                                else
                                {
                                    activeInstance.Stop();

                                    _activeInstances.RemoveAt(i);
                                }
                            }
                        }

                        if (_currentMusic != null)
                        {
                            _currentMusic.Pause();
                        }

                        if (item.FadeInTime > 0f)
                        {
                            createFadeInRequest(instance);
                        }

                        instance.Play();
                        break;
                    }
            }

            _activeInstances.Add(instance);
        }

        public void UpdateThread()
        {
            while (App.Current != null)
            {
                var currentTimestamp = new TimeSpan(Stopwatch.GetTimestamp());
                var deltaTimestamp = currentTimestamp - _lastTimestamp;

                float deltaTime = (float)deltaTimestamp.TotalSeconds;

                List<FadeRequest> copyFadeRequest;
                lock (_mutex)
                {
                    copyFadeRequest = new List<FadeRequest>(_fadeRequests);
                }

                for (int i = copyFadeRequest.Count - 1; i >= 0; --i)
                {
                    var request = copyFadeRequest[i];

                    request.Update(deltaTime);

                    if (request.IsDone)
                    {
                        lock (_mutex)
                        {
                            _fadeRequests.RemoveAt(i);
                        }

                        if (request.RemoveInstance)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                onInstanceCompleted(request.Instance);
                            }
                            );
                        }
                    }
                }

                _lastTimestamp = currentTimestamp;
                Thread.Yield();
            }
        }

        public void StopAll()
        {
            foreach(var instance in _activeInstances)
            {
                instance.Stop();
            }

            _activeInstances.Clear();
        }

        public void UpdateAllVolumes()
        {
            foreach(var instance in _activeInstances)
            {
                if (!_fadeRequests.Exists(x => x.Instance == instance))
                {
                    instance.Volume = instance.Item.Volume * MasterVolume;
                }
            }
        }

        private void createFadeInRequest(SoundInstance instance)
        {
            FadeRequest fadeRequest = new FadeRequest();
            fadeRequest.Instance = instance;
            fadeRequest.SourceVolume = new SoundVolume(0f);
            fadeRequest.TargetVolume = instance.Item.Volume;
            fadeRequest.Duration = instance.Item.FadeInTime;

            instance.Volume = fadeRequest.SourceVolume;

            lock (_mutex)
            {
                _fadeRequests.Add(fadeRequest);
            }
        }

        private void createStopFadeOutRequest(SoundInstance instance)
        {
            FadeRequest fadeRequest = new FadeRequest();
            fadeRequest.Instance = instance;
            fadeRequest.SourceVolume = instance.Volume;
            fadeRequest.TargetVolume = new SoundVolume(0f);
            fadeRequest.Duration = instance.Item.FadeOutTime;
            fadeRequest.RemoveInstance = true;

            lock (_mutex)
            {
                _fadeRequests.Add(fadeRequest);
            }
        }

        private void onInstanceCompleted(SoundInstance instance)
        {
            if (instance.Type == SoundType.Jingle)
            {
                if (_currentMusic != null)
                {
                    _currentMusic.Resume();
                }
            }

            _activeInstances.Remove(instance);
        }
    }
}

