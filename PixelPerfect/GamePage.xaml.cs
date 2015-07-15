using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using MonoGame.Framework.WindowsPhone;
using PixelPerfect.Resources;

namespace PixelPerfect
{
    public partial class GamePage : PhoneApplicationPage
    {
        private Game1 _game;
        public static GamePage Instance = null;

        // Constructor
        public GamePage()
        {
            InitializeComponent();

            if (Instance != null)
                throw new InvalidOperationException("There can be only one GamePage object!");
            Instance = this;
            _game = XamlGame<Game1>.Create("", this);
            //AdRotator.Invalidate(null);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        public void AdsOff()
        {
            AdsControl(false);
        }

        public void AdsOn()
        {
            AdsControl(true);
        }

        public void AdsControl(bool value)
        {
            if (AdRotator == null)
                return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                AdRotator.IsEnabled = value;
                AdRotator.Visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);                
            });
        }
    }
}