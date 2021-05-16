using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace AndroidGB
{
    [Activity(Label = "AndroidGB"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        Game1 game;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            game = new Game1();
            SetContentView((View)game.Services.GetService(typeof(View)));
            game.Run();

            
        }

        public override void OnBackPressed()
        {
            game.Close();
            base.OnBackPressed();
        }
    }
}

