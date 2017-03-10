using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XamStreamPlayer.Annotations;
using XamStreamPlayer.Services;

namespace XamStreamPlayer.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        private readonly IMediaService _mediaService;

        private bool IsStoped = true;
        private CancellationTokenSource cancelTokenSource;

        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public bool _isError = false;
        public bool IsError
        {
            get
            {
                return _isError;
            }

            set
            {
                _isError = value;
                OnPropertyChanged();
            }
        }

        public bool _isPlay = false;
        public bool IsPlay
        {
            get
            {
                return _isPlay;
            }

            set
            {
                _isPlay = value;
                OnPropertyChanged();
            }
        }


        public string _urlSound = "";
        public string UrlSound
        {
            get
            {
                return _urlSound;
            }

            set
            {
                _urlSound = value;
                OnPropertyChanged();
            }
        }


        public ICommand StartPlayCommand => new Command(StartPlay);
        public ICommand PausePlayCommand => new Command(PausePlay);
        public ICommand StopPlayCommand => new Command(StopPlay);

        public ICommand ReconectedCommand => new Command(Reconected);


        public MainVM()
        {
            _mediaService = DependencyService.Get<IMediaService>();
            IsPlay = _mediaService.IsPlaying();
        }

        public async Task InitializeAsync(object navigationData)
        {
            IsBusy = true;

            //todo: Get url from server
            await Task.Delay(500);
            UrlSound = "";

            IsBusy = false;
        }


        
        private async void StartPlay(object obj)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                IsPlay = true;

                cancelTokenSource?.Cancel();

                cancelTokenSource = new CancellationTokenSource();
                CancellationToken cancelToken = cancelTokenSource.Token;
                
                await Task.Run(() =>
                {
                    if (IsStoped)
                    {
                        _mediaService.Error += _mediaService_Error;
                        _mediaService.StatusChanged += _mediaService_StatusChanged;

                        _mediaService.Initialize(UrlSound);
                    }

                }, cancelToken);
                _mediaService.Play();

                IsStoped = false;
                IsBusy = false;
            }
        }
        
        private void PausePlay(object obj)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                IsPlay = false;

                _mediaService.Pause();

                IsBusy = false;
            }
        }

        private void StopPlay(object obj)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                IsPlay = false;
                IsStoped = true;

                _mediaService.Stop();

                IsBusy = false;
            }
        }
        

        private void Reconected(object obj)
        {
            IsError = false;
            StopPlay(null);
        }


        #region Helpers

        private void _mediaService_Error(object sender, EventArgs e)
        {
            IsError = true;
        }

        private void _mediaService_StatusChanged(object sender, System.EventArgs e)
        {
            IsPlay = _mediaService.IsPlaying();
        }


        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 
        #endregion
    }
}
