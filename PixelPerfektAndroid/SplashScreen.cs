
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PixelPerfect
{
	[Activity (Theme = "@style/Theme.Splash", MainLauncher = false, NoHistory = true)]			
	public class SplashScreen : Activity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			Thread.Sleep(100); // Simulate a long loading process on app startup.
			StartActivity(typeof(Activity1));
			this.Window.AddFlags (WindowManagerFlags.Fullscreen);
		}
	}
}

