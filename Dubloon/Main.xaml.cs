using Dubloon.Common;
using Dubloon.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Devices.Geolocation.Geofencing;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dubloon.Models;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Dubloon
{
    public sealed partial class Main : Page
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";
        private const string ThirdGroupName = "ThirdGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private Geolocator geo = null;
        private CoreDispatcher _cd;

        ObservableCollection<TableHunts> hunts = new ObservableCollection<TableHunts>();

        public Main()
        {
            this.InitializeComponent();
            _cd = Window.Current.CoreWindow.Dispatcher;
            Initialize();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }
        private async void Initialize()
        {
            // other initialization logic
            GeofenceMonitor.Current.Geofences.Clear();
            RegisterBackgroundTask();
            GeofenceMonitor.Current.GeofenceStateChanged += OnGeofenceStateChanged;
            if (geo == null)
            {
                geo = new Geolocator();
            }
            geo.ReportInterval = 1000;
            geo.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.High;
            //geo.PositionChanged +=
            //    new TypedEventHandler<Geolocator,
            //        PositionChangedEventArgs>(geo_PositionChanged);
            //TEST GEOFENCE
            CreateGeofence("School", 40.427628, -86.917016, 100);

            var huntsResponse = await ViewModels.PullFromAzure.PullHuntsFromAzure();
            foreach (TableHunts h in huntsResponse)
            {
                hunts.Add(h);
            }

            //ViewModels.AddToAzure vm = new ViewModels.AddToAzure();
            //TableHunts blah = new TableHunts();
            //blah.Title = "TreasureHunt";
            //blah.Author = "Corbin Trexler";
            //blah.Description = "A fun hunt!";
            //blah.Difficulty = 5;
            //blah.Duration = 10;
            //vm.AddHuntToAzure(blah);

            //TableTrails blah2 = new TableTrails();
            //blah2.Name = "Trail Name";
            //blah2.HuntId = "w345yrgtv245wrtv";
            //vm.AddTrailToAzure(blah2);

            //TableNodes blah3 = new TableNodes();
            //blah3.Name = "Node Name";
            //blah3.Latitude = 40.111;
            //blah3.Longitude = 85.111;
            //blah3.Radius = 100;
            //blah3.TrailId = "w345yrgtv245wrtv";
            //vm.AddNodeToAzure(blah3);

            //ViewModels.PullFromAzure vm2 = new ViewModels.PullFromAzure();
            //var blah4 = await vm2.PullHuntsFromAzure();
            //System.Diagnostics.Debug.WriteLine(blah4[0].Title);

            //var blah5 = await vm2.PullTrailsFromAzure();
            //System.Diagnostics.Debug.WriteLine(blah5[0].Name);

            //var blah6 = await vm2.PullNodesFromAzure();
            //System.Diagnostics.Debug.WriteLine(blah6[0].Name);
        }

        public async void OnGeofenceStateChanged(GeofenceMonitor sender, object e)
        {
            var reports = sender.ReadReports();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (GeofenceStateChangeReport report in reports)
                {
                    GeofenceState state = report.NewState;

                    Geofence geofence = report.Geofence;

                    if (state == GeofenceState.Removed)
                    {
                        // remove the geofence from the geofences collection
                        GeofenceMonitor.Current.Geofences.Remove(geofence);
                    }
                    else if (state == GeofenceState.Entered)
                    {
                        // Your app takes action based on the entered event
                        //TEST GEOFENCE ENTRANCE
                        System.Diagnostics.Debug.WriteLine("You've enetered a geofence!");

                        // NOTE: You might want to write your app to take particular
                        // action based on whether the app has internet connectivity.

                    }
                    else if (state == GeofenceState.Exited)
                    {
                        // Your app takes action based on the exited event

                        // NOTE: You might want to write your app to take particular
                        // action based on whether the app has internet connectivity.

                    }
                }
            });
        }


        async private void RegisterBackgroundTask(/*object sender, RoutedEventArgs e*/)
        {
            // Get permission for a background task from the user. If the user has already answered once,
            // this does nothing and the user must manually update their preference via PC Settings.
            BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            // Regardless of the answer, register the background task. If the user later adds this application
            // to the lock screen, the background task will be ready to run.
            // Create a new background task builder
            BackgroundTaskBuilder geofenceTaskBuilder = new BackgroundTaskBuilder();

            geofenceTaskBuilder.Name = "GeofenceBackgroundTask";
            geofenceTaskBuilder.TaskEntryPoint = typeof(BackgroundTasks.GeofenceBackgroundTask).FullName;

            // Create a new location trigger
            var trigger = new LocationTrigger(LocationTriggerType.Geofence);

            // Associate the locationi trigger with the background task builder
            geofenceTaskBuilder.SetTrigger(trigger);

            // If it is important that there is user presence and/or
            // internet connection when OnCompleted is called
            // the following could be called before calling Register()
            // SystemCondition condition = new SystemCondition(SystemConditionType.UserPresent | SystemConditionType.InternetAvailable);
            // geofenceTaskBuilder.AddCondition(condition);

            // Register the background task
            BackgroundTaskRegistration geofenceTask = geofenceTaskBuilder.Register();

            // Associate an event handler with the new background task
            geofenceTask.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

            //BackgroundTaskState.RegisterBackgroundTask(BackgroundTaskState.LocationTriggerBackgroundTaskName);

            //switch (backgroundAccessStatus)
            //{
            //    case BackgroundAccessStatus.Unspecified:
            //    case BackgroundAccessStatus.Denied:
            //        rootPage.NotifyUser("This application must be added to the lock screen before the background task will run.", NotifyType.ErrorMessage);
            //        break;

            //}
        }

        async private void OnCompleted(IBackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs e)
        {
            if (sender != null)
            {
                // Update the UI with progress reported by the background task
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // do your apps work here
                    //TEST RETURN-TO-APP EVENT
                    System.Diagnostics.Debug.WriteLine("Returning to app!!!");
                });
            }
        }

        # region NAVIGATION
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
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }
        # endregion

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        //private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string groupName = this.pivot.SelectedIndex == 0 ? FirstGroupName : SecondGroupName;
        //    var group = this.DefaultViewModel[groupName] as SampleDataGroup;
        //    var nextItemId = group.Items.Count + 1;
        //    var newItem = new SampleDataItem(
        //        string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
        //        string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
        //        string.Empty,
        //        string.Empty,
        //        this.resourceLoader.GetString("NewItemDescription"),
        //        string.Empty);

        //    group.Items.Add(newItem);

        //    // Scroll the new item into view.
        //    var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
        //    var listView = container.ContentTemplateRoot as ListView;
        //    listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        //}

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

        // CONTINUALLY TRACK AND DISPLAY LOCATION CHANGES
        //async private void geo_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        //{
        //    await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        Geoposition pos = e.Position;
        //        textLatitude.Text = "Latitude: " + pos.Coordinate.Point.Position.Latitude.ToString();
        //        textLongitude.Text = "Longitude: " + pos.Coordinate.Point.Position.Longitude.ToString();
        //        textAccuracy.Text = "Accuracy: " + pos.Coordinate.Accuracy.ToString();
        //    });
        //}

        private void CreateGeofence(string Id, double Latitude, double Longitude, double Radius)
        {
            Geofence geofence = null;

            string fenceKey = Id;

            BasicGeoposition position;
            position.Latitude = Latitude;
            position.Longitude = Longitude;
            position.Altitude = 0.0;
            double radius = Radius;

            // the geofence is a circular region
            Geocircle geocircle = new Geocircle(position, radius);

            bool singleUse = true;

            // want to listen for enter geofence, exit geofence and remove geofence events
            // you can select a subset of these event states
            MonitoredGeofenceStates mask = 0;

            mask |= MonitoredGeofenceStates.Entered;
            //mask |= MonitoredGeofenceStates.Exited;
            //mask |= MonitoredGeofenceStates.Removed;

            // setting up how long you need to be in geofence for enter event to fire
            TimeSpan dwellTime = new TimeSpan(0);
            // DWELL TIME: "ParseTimeSpan" not working
            //if ("" != DwellTime.Text)
            //{
            //    dwellTime = new TimeSpan(ParseTimeSpan(DwellTime.Text, defaultDwellTimeSeconds));
            //}
            //else
            //{
            //    dwellTime = new TimeSpan(ParseTimeSpan("0", defaultDwellTimeSeconds));
            //}

            // setting up how long the geofence should be active
            TimeSpan duration = new TimeSpan(0);

            //TIME SPAN: "ParseTimeSpan" not working
            //if ("" != Duration.Text)
            //{
            //    duration = new TimeSpan(ParseTimeSpan(Duration.Text, 0));
            //}
            //else
            //{
            //    duration = new TimeSpan(ParseTimeSpan("0", 0));
            //}

            // setting up the start time of the geofence
            DateTimeOffset startTime = DateTime.Now;

            //START TIME: How to set start time
            //if ("" != StartTime.Text)
            //{
            //    startTime = DateTimeOffset.Parse(StartTime.Text);
            //}
            //else
            //{
            //    // if you don't set start time in C# the start time defaults to 1/1/1601
            //    calendar.SetToNow();

            //    startTime = calendar.GetDateTime();
            //}

            geofence = new Geofence(fenceKey, geocircle, mask, singleUse, dwellTime, startTime, duration);
            GeofenceMonitor.Current.Geofences.Add(geofence);

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as Pivot).SelectedIndex == 0)
            {
                ButtonCreateHunt.Visibility = Visibility.Collapsed;
            }
            else if ((sender as Pivot).SelectedIndex == 1)
            {
                ButtonCreateHunt.Visibility = Visibility.Visible;
            }
            else if ((sender as Pivot).SelectedIndex == 2)
            {
                ButtonCreateHunt.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Hunt));
        }

        private void ButtonCreateHunt_Click(object sender, RoutedEventArgs e)
        {
            FormCreateHunt.Visibility = Visibility.Visible;
            ButtonCreateHunt.Visibility = Visibility.Collapsed;
            ButtonCancelHunt.Visibility = Visibility.Visible;
            ButtonSubmitHunt.Visibility = Visibility.Visible;

            InputTitle.Text = "";
            InputAuthor.Text = "";
            InputDescription.Text = "";
            InputDifficulty.Value = 3;
        }
        private void ButtonCancelHunt_Click(object sender, RoutedEventArgs e)
        {
            FormCreateHunt.Visibility = Visibility.Collapsed;
            ButtonCreateHunt.Visibility = Visibility.Visible;
            ButtonCancelHunt.Visibility = Visibility.Collapsed;
            ButtonSubmitHunt.Visibility = Visibility.Collapsed;
        }
        private async void ButtonSubmitHunt_Click(object sender, RoutedEventArgs e)
        {
            var huntsResponse = await ViewModels.PullFromAzure.PullHuntsFromAzure();
            if (!huntsResponse.Any(h => h.Title == InputTitle.Text))
            {
                var item = await ViewModels.AddToAzure.AddHuntToAzure(InputTitle.Text, InputAuthor.Text, InputDescription.Text, InputDifficulty.Value);
                hunts.Add(item);
                System.Diagnostics.Debug.WriteLine("Sent to azure!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Already in the list!");
            }

            FormCreateHunt.Visibility = Visibility.Collapsed;
            ButtonCreateHunt.Visibility = Visibility.Visible;
            ButtonCancelHunt.Visibility = Visibility.Collapsed;
            ButtonSubmitHunt.Visibility = Visibility.Collapsed;
        }

        private void ListViewHunts_Loaded(object sender, RoutedEventArgs e)
        {
            this.ListViewHunts = (ListView)sender;
            this.ListViewHunts.ItemsSource = hunts;
        }

        private void ListViewHunts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Hunt));
            var ListViewSelection = e.AddedItems.Cast<TableHunts>().ToList().First();
            Views.PassedData.Title = ListViewSelection.Title;
            Views.PassedData.Author = ListViewSelection.Author;
            Views.PassedData.Description = ListViewSelection.Description;
            Views.PassedData.Difficulty = ListViewSelection.Difficulty;
            Views.PassedData.Duration = ListViewSelection.Duration;
        }
    }
}
