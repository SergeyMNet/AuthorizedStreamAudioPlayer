# AuthorizedStreamAudioPlayer
Xamarin forms - authorized stream audio player (android/ios)

Use HMAC Authentication for secure audio stream


#backend:
    [HmacAuthorize]    
    public class DataController : Controller
    
#client:
    public const string AppId = "12b901a3-fb5c-457c-8d55-eb26748df1a2";
    public const string AppKey = "UAszSt1DJyAjoKg2VjZBjWOyzlKTp33v5QkMLhwxRp7=";

#Android player
    // Get Token
    var handler = new CustomDelegatingHandler();
    var header = await handler.GetToken(url);

    // create header
    Dictionary<String, String> headers = new Dictionary<string, string>();
    headers.Add("Authorization", header);

    var uri = Android.Net.Uri.Parse(url);

    _player.SetDataSource(Forms.Context, uri, headers);
    _player.Prepare();
    
#iOS player
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

