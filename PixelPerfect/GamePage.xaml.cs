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
using Microsoft.AdMediator.WindowsPhone8;
using System.Diagnostics;
using Windows.ApplicationModel.Store;
using StoreExitAction = System.Action<string, string, bool>;

namespace PixelPerfect
{
    public partial class GamePage : PhoneApplicationPage
    {
        private Game1 _game;
        public static GamePage Instance = null;

        private static StoreExitAction StoreExitCallback;

        // Constructor
        public GamePage()
        {
            InitializeComponent();


            CurrentApp.LicenseInformation.LicenseChanged += LicenseInformation_LicenseChanged;

            if (Instance != null)
                throw new InvalidOperationException("There can be only one GamePage object!");
            Instance = this;
            _game = XamlGame<Game1>.Create("", this);
            AdsOff();
            //AdRotator.Invalidate(null);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            //AdRotator.Invalidate(null);            

        }

        void LicenseInformation_LicenseChanged()
        {
            Globals.noads = CurrentApp.LicenseInformation.ProductLicenses["noads"].IsActive;
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

        void AdMediator_Bottom_AdSdkEvent(object sender, Microsoft.AdMediator.Core.Events.AdSdkEventArgs e)
        {
            Debug.WriteLine("AdSdk event {0} by {1}", e.EventName, e.Name);
        }

        void AdMediator_Bottom_AdMediatorError(object sender, Microsoft.AdMediator.Core.Events.AdMediatorFailedEventArgs e)
        {
            Debug.WriteLine("AdMediatorError:" + e.Error + " " + e.ErrorCode);
            // if (e.ErrorCode == AdMediatorErrorCode.NoAdAvailable)
            // AdMediator will not show an ad for this mediation cycle
        }

        void AdMediator_Bottom_AdFilled(object sender, Microsoft.AdMediator.Core.Events.AdSdkEventArgs e)
        {
            Debug.WriteLine("AdFilled:" + e.Name);
        }

        void AdMediator_Bottom_AdError(object sender, Microsoft.AdMediator.Core.Events.AdFailedEventArgs e)
        {
            Debug.WriteLine("AdSdkError by {0} ErrorCode: {1} ErrorDescription: {2} Error: {3}", e.Name, e.ErrorCode, e.ErrorDescription, e.Error);
        }

        public void AdsOff()
        {
#if !WINDOWS
            AdMediator_6FC9F8.Disable();
            //AdMediator_6FC9F8.Visibility = System.Windows.Visibility.Collapsed;
#endif
        }


        public void AdsOn()
        {
            if (Globals.noads)
                return;
#if !WINDOWS
            //AdMediator_6FC9F8.Visibility = System.Windows.Visibility.Visible;            
            AdMediator_6FC9F8.Resume();
#endif
        }


#if !WINDOWS
        public void LaunchStoreForProductPurchase(string productID, bool requestReceipt, StoreExitAction storeExitCallback)
        {

            StoreExitCallback = storeExitCallback;
            Dispatcher.BeginInvoke(() => LaunchStoreForProductPurchaseASync(productID, requestReceipt));
        }

        private static async void LaunchStoreForProductPurchaseASync(string productID, bool requestReceipt)
        {
            bool productPurchaseError = false;
            string receipt = "";

            try
            {
                receipt = await CurrentApp.RequestProductPurchaseAsync(productID, requestReceipt);

            }
            catch (Exception ex)
            {
                productPurchaseError = true;
            }

            try
            {
                if (StoreExitCallback != null) StoreExitCallback(productID, receipt, productPurchaseError);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
#endif
    }
}