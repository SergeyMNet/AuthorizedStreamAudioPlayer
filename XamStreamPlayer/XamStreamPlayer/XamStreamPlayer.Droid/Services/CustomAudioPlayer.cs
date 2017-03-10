using System;
using System.Collections.Generic;
using Android.Media;
using Xamarin.Forms;
using XamStreamPlayer.Droid.Services;
using XamStreamPlayer.Services;
using System.Diagnostics;
using XamStreamPlayer.DataServices;

[assembly: Dependency(typeof(CustomAudioPlayer))]
namespace XamStreamPlayer.Droid.Services
{
    public class CustomAudioPlayer : IMediaService
    {
        private MediaPlayer _player = new MediaPlayer();

        private string oldUrl = "";
        private bool isInitialize;


        public async void Initialize(string url)
        {
            if (oldUrl != url || _player == null || !isInitialize)
            {
                Dispose();
                oldUrl = url;

                SubscribeCalls();

                _player = new MediaPlayer();
                _player.SetAudioStreamType(Android.Media.Stream.Music);

                // Get Token
                var handler = new CustomDelegatingHandler();
                var header = await handler.GetToken(url);

                // create header
                Dictionary<String, String> headers = new Dictionary<string, string>();
                headers.Add("Authorization", header);

                var uri = Android.Net.Uri.Parse(url);

                // crash - no internet 
                try
                {
                    isInitialize = false;

                    _player.BufferingUpdate += _player_BufferingUpdate;

                    _player.SetDataSource(Forms.Context, uri, headers);
                    _player.Prepare();
                    //_player.Prepared += _player_Prepared;

                    _player.Error += _player_Error;
                    _player.Completion += _player_Completion;
                    isInitialize = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    DoError();
                }
            }
        }

        private void _player_Completion(object sender, EventArgs e)
        {
            DoStatusChanged();
        }

        private void _player_BufferingUpdate(object sender, MediaPlayer.BufferingUpdateEventArgs e)
        {
            Debug.WriteLine("-------buffer = " + e.Percent);
        }

        private async void _player_Error(object sender, MediaPlayer.ErrorEventArgs e)
        {
            Debug.WriteLine("-------_player_Error " + e.What);
            isInitialize = false;
        }

        public void Play()
        {
            _player?.Start();
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void Stop()
        {
            _player?.Pause();
            _player?.SeekTo(0);
        }

        public bool IsPlaying()
        {
            return _player != null ? _player.IsPlaying : false;
        }



        public event EventHandler StatusChanged;
        private void DoStatusChanged()
        {
            EventHandler eh = StatusChanged;
            if (eh != null)
                eh(null, EventArgs.Empty);
        }

        public event EventHandler Error;
        private void DoError()
        {
            EventHandler eh = Error;
            if (eh != null)
                eh(null, EventArgs.Empty);
        }


        /// <summary>
        /// Kill Player
        /// </summary>
        public void Dispose()
        {
            _player?.Stop();
            _player?.Dispose();
            _player = null;

            MessagingCenter.Unsubscribe<IncomingCallReceiver>(this, "incoming");
            MessagingCenter.Unsubscribe<IncomingCallReceiver>(this, "end");
        }


        #region CallReceiver

        private bool _isReceiverPause;
        private void SubscribeCalls()
        {
            _isReceiverPause = false;

            MessagingCenter.Subscribe<IncomingCallReceiver>(this, "incoming", (sender) =>
            {
                //Debug.WriteLine("--------Call incoming! Recive");
                if (IsPlaying())
                {
                    Pause();
                    _isReceiverPause = true;
                }

            });

            MessagingCenter.Subscribe<IncomingCallReceiver>(this, "end", (sender) =>
            {
                //Debug.WriteLine("--------Call end! Recive");
                if (_isReceiverPause)
                {
                    Play();
                    _isReceiverPause = false;
                }
            });
        }
        #endregion
    }
}