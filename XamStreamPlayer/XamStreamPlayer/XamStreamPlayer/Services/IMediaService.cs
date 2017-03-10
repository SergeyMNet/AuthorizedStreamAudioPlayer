using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamStreamPlayer.Services
{
    public interface IMediaService
    {
        void Initialize(string url);
        void Play();
        void Pause();
        void Stop();

        bool IsPlaying();

        event EventHandler StatusChanged;
        event EventHandler Error;
    }
}
