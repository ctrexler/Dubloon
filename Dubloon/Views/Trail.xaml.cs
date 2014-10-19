using Dubloon.Common;
using Dubloon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Dubloon.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Trail : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public ObservableCollection<TableNodes> nodes = new ObservableCollection<TableNodes>();
        bool mapIconExists = false;

        public Trail()
        {
            this.InitializeComponent();
            Initialize();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public async void Initialize()
        {
            CenterMap();
            var nodessResponse = await ViewModels.PullFromAzure.PullNodesFromAzure();
            foreach (TableNodes n in nodessResponse.Where(id => id.TrailId == PassedData.Id))
            {
                nodes.Add(n);
            }
            TreasureMap_Populate();
        }
        async private void CenterMap()
        {
            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = await geolocator.GetGeopositionAsync();
            TreasureMap.Center = geoposition.Coordinate.Point;
            TreasureMap.ZoomLevel = 16;
        }
        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CenterMap();
        }

        private void TreasureMap_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
            List<MapElement> pushpins = new List<MapElement>();
            pushpins = TreasureMap.FindMapElementsAtOffset(args.Position).ToList();

            if (pushpins.Count != 0)
            {
                this.Frame.Navigate(typeof(Node));
                PassedData.Title = (pushpins.First() as MapIcon).Title;
            }
            if (mapIconExists)
            {
                TreasureMap.MapElements.Remove(TreasureMap.MapElements.Last());
            }
            InputLatitude.Text = Math.Round(args.Location.Position.Latitude, 3).ToString();
            InputLongitude.Text = Math.Round(args.Location.Position.Longitude, 3).ToString();
            MapIcon mapicon = new MapIcon();
            mapicon.Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = args.Location.Position.Latitude,
                Longitude = args.Location.Position.Longitude
            });
            mapicon.NormalizedAnchorPoint = new Point(0.5, 0.5);
            mapicon.Title = InputName.Text;
            mapicon.Image =
            RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/xMarksTheSpot.png"));
            TreasureMap.MapElements.Add(mapicon);
            mapIconExists = true;
        }

        private void TreasureMap_Populate()
        {
            foreach (TableNodes n in nodes)
            {
                MapIcon mapicon = new MapIcon();
                mapicon.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = n.Latitude,
                    Longitude = n.Longitude
                });
                mapicon.NormalizedAnchorPoint = new Point(0.5, 0.5);
                mapicon.Title = n.Name;
                mapicon.Image =
                RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/xMarksTheSpot.png"));
                TreasureMap.MapElements.Add(mapicon);
            }
        }

        async private void ButtonSubmitNode_Click(object sender, RoutedEventArgs e)
        {
            var nodesResponse = await ViewModels.PullFromAzure.PullNodesFromAzure();
            if (!nodesResponse.Any(n => n.Name == InputName.Text))
            {
                var item = await ViewModels.AddToAzure.AddNodeToAzure(InputName.Text, Double.Parse(InputLatitude.Text), Double.Parse(InputLongitude.Text), Int32.Parse(InputRadius.Text), PassedData.Id);
                nodes.Add(item);
                Main.CreateGeofence(item.Name, item.Latitude, item.Longitude, item.Radius);
                System.Diagnostics.Debug.WriteLine("Sent to azure!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Already in the list!");
            }
            TreasureMap_Populate();
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Node));
        }

        private void ButtonCreateNode_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
