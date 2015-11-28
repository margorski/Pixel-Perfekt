using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Gms.Ads;

namespace PixelPerfect
{
    [Activity(Label = "Pixel Perfekt"
        , Icon = "@drawable/icon"
		, MainLauncher = true
		, Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
		AdView _bannerad;
		public static Activity1 Instance = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
			if (Instance != null)
				throw new InvalidOperationException("There can be only one GamePage object!");
			Instance = this;



			var g = new Game1();
			RelativeLayout f1 = new RelativeLayout (this);
			f1.AddView ((View)g.Services.GetService (typeof(View)));

			//SetContentView((View)g.Services.GetService(typeof(View)));
			//SetContentView(PixelPerfektAndroid.Resource.Layout.Main);

			//--- ad
			_bannerad = AdWrapper.ConstructStandardBanner(this, AdSize.Banner, "ca-app-pub-2435293225367694/4685976564");
			var listener = new AdEventListener ();
			listener.AdLoaded += () => { _bannerad.RequestLayout(); };
			_bannerad.AdListener = listener;
			_bannerad.CustomBuild ();

			RelativeLayout.LayoutParams rlParams = new RelativeLayout.LayoutParams (WindowManagerLayoutParams.FillParent, WindowManagerLayoutParams.WrapContent);
			rlParams.AddRule (LayoutRules.AlignParentBottom);
			f1.AddView (_bannerad, rlParams);
			_bannerad.Pause ();
			_bannerad.Visibility = ViewStates.Gone;
			//--- ad
		
			SetContentView (f1);

            g.Run();
        }
			
		protected override void OnResume()
		{
			if (_bannerad != null) _bannerad.Resume();
			base.OnResume();
		}

		protected override void OnPause()
		{
			if(_bannerad != null)_bannerad.Pause();
			base.OnPause();
		}

		public void AdsOn()
		{
			if (_bannerad != null) 
			{
				_bannerad.Resume ();
				_bannerad.Visibility = ViewStates.Visible;
			}
		}

		public void AdsOff()
		{
			if (_bannerad != null) 
			{
				_bannerad.Pause ();
				_bannerad.Visibility = ViewStates.Gone;
			}
		}
    }
}

