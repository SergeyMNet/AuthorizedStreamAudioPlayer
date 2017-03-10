using Android.App;
using Android.Content;
using Android.Telephony;
using Xamarin.Forms;

namespace XamStreamPlayer.Droid.Services
{
    [BroadcastReceiver()]
    [IntentFilter(new[] { "android.intent.action.PHONE_STATE" })]
    public class IncomingCallReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // ensure there is information
            if (intent.Extras != null)
            {
                // get the incoming call state
                string state = intent.GetStringExtra(TelephonyManager.ExtraState);

                // check the current state
                if (state == TelephonyManager.ExtraStateRinging)
                {
                    // read the incoming call telephone number...
                    //Debug.WriteLine("--------Call incoming!");
                    MessagingCenter.Send<IncomingCallReceiver>(this, "incoming");
                }
                else if (state == TelephonyManager.ExtraStateOffhook)
                {
                    // incoming call answer
                    //Debug.WriteLine("--------Call answer!");
                    //MessagingCenter.Send<IncomingCallReceiver>(this, "answer");
                }
                else if (state == TelephonyManager.ExtraStateIdle)
                {
                    // incoming call end
                    //Debug.WriteLine("--------Call end!");
                    MessagingCenter.Send<IncomingCallReceiver>(this, "end");
                }
            }
        }

    }
}