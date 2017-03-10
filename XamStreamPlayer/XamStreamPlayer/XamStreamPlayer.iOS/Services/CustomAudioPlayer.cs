using System;
using System.Collections.Generic;
using System.Text;
using AVFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms;
using XamStreamPlayer.DataServices;
using XamStreamPlayer.iOS.Services;
using XamStreamPlayer.Services;

[assembly: Dependency(typeof(CustomAudioPlayer))]
namespace XamStreamPlayer.iOS.Services
{
    public class CustomAudioPlayer : IMediaService
    {
        private static AVPlayer _player;
        private static NSUrl _soundUrl;
        private nint _backgroundIndentifier;

        public event EventHandler StatusChanged;
        public event EventHandler Error;

        private bool _isStoped = true;
        private bool _isMustPlayed;
        private bool IsMustPlayed
        {
            get { return _isMustPlayed; }
            set { _isMustPlayed = value; DoStatusChanged(); }
        }


        public CustomAudioPlayer()
        {

        }

        /// <summary>
        /// Init player
        /// </summary>
        /// <param name="url"></param>
        public void Initialize(string url)
        {
            if (_isStoped)
            {
                _isStoped = false;

                Dispose();
                SubscribeCalls();


                InitPlayer(url);
                ConfigureBackgroundAudioTask();
                ConfigureAudioSession();
                ConfigureObservers();
            }
        }

        /// <summary>
        /// Start play
        /// </summary>
        public void Play()
        {
            if (_player.Error != null)
            {
                Stop();
                DoError();
                DoStatusChanged();
            }

            IsMustPlayed = true;
            _player.Play();
        }

        /// <summary>
        /// Pause player
        /// </summary>
        public void Pause()
        {
            IsMustPlayed = false;
            _player.Pause();
        }

        /// <summary>
        /// Stop player
        /// </summary>
        public void Stop()
        {
            IsMustPlayed = false;
            _player?.Pause();
            Dispose();
            _isStoped = true;
        }

        /// <summary>
        /// Get Status player
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            return IsMustPlayed;
        }





        #region events
        private void DoError()
        {
            EventHandler eh = Error;
            if (eh != null)
                eh(null, EventArgs.Empty);
        }

        private void DoStatusChanged()
        {
            EventHandler eh = StatusChanged;
            if (eh != null)
                eh(null, EventArgs.Empty);
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Init player to url with token
        /// </summary>
        /// <param name="url"></param>
        private async void InitPlayer(string url)
        {
            // Get Token
            var handler = new CustomDelegatingHandler();
            var header = await handler.GetToken(url);

            _soundUrl = NSUrl.FromString(url);

            NSMutableDictionary headers = new NSMutableDictionary();
            headers.SetValueForKey(NSObject.FromObject(header), new NSString("Authorization"));
            var dict = new NSDictionary(@"AVURLAssetHTTPHeaderFieldsKey", headers);
            var asset = new AVUrlAsset(_soundUrl, dict);

            AVPlayerItem item = new AVPlayerItem(asset);
            _player = AVPlayer.FromPlayerItem(item);
        }

        /// <summary>
        /// Subscribe for resume play after incoming call
        /// </summary>
        private void SubscribeCalls()
        {
            MessagingCenter.Subscribe<AppDelegate>(this, "OnActivatedIos", (sender) =>
            {
                if (IsMustPlayed)
                {
                    Play();
                }
            });
        }

        /// <summary>
        /// Unscribe and EndBackgroundTask
        /// </summary>
        private void Dispose()
        {
            MessagingCenter.Unsubscribe<AppDelegate>(this, "OnActivatedIos");

            UIApplication.SharedApplication.EndBackgroundTask(_backgroundIndentifier);
            _backgroundIndentifier = UIApplication.BackgroundTaskInvalid;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Configure for Background Audio
        /// </summary>
        private void ConfigureBackgroundAudioTask()
        {
            _backgroundIndentifier = UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                UIApplication.SharedApplication.EndBackgroundTask(_backgroundIndentifier);
                _backgroundIndentifier = UIApplication.BackgroundTaskInvalid;
            });
        }

        /// <summary>
        /// Configure for play Audio
        /// </summary>
        private void ConfigureAudioSession()
        {
            try
            {
                AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
                AVAudioSession.SharedInstance().SetActive(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ConfigureObservers()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.PlaybackStalledNotification, handleStall);
        }


        private void handleStall(NSNotification p_NSNotification)
        {
            _player?.Pause();
            _player?.Play();

            Console.WriteLine("Notification: {0}", p_NSNotification);
        }

        #endregion

    }
}
