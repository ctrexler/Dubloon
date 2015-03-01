using Dubloon.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    public sealed partial class Node : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        int flipsX = 0;
        int flipsY = 0;
        int checkerX = 0;
        int checkerY = 0;
        int CODE_X = 3;
        int CODE_Y = 2;
        AccelerometerReading reading;

        private Accelerometer _accelerometer;
        private uint _desiredReportInterval;
        private DispatcherTimer _dispatcherTimer;

        public Node()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
                // This value will be used later to activate the sensor.
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                _desiredReportInterval = minReportInterval > 16 ? minReportInterval : 16;

                // Set up a DispatchTimer
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += DisplayCurrentReading;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)_desiredReportInterval);
                reading = _accelerometer.GetCurrentReading();
            }
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
            ScenarioEnable();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ScenarioEnable(/*object sender, RoutedEventArgs e*/)
        {
            if (_accelerometer != null)
            {
                // Set the report interval to enable the sensor for polling
                _accelerometer.ReportInterval = _desiredReportInterval;

                Window.Current.VisibilityChanged += new WindowVisibilityChangedEventHandler(VisibilityChanged);
                _dispatcherTimer.Start();
            }
        }

        private void VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                // Re-enable sensor input (no need to restore the desired reportInterval... it is restored for us upon app resume)
                _dispatcherTimer.Start();
            }
            else
            {
                // Disable sensor input (no need to restore the default reportInterval... resources will be released upon app suspension)
                _dispatcherTimer.Stop();
            }
        }

        private void DisplayCurrentReading(object sender, object args)
        {
            reading = _accelerometer.GetCurrentReading();
            if (reading != null)
            {
                if (flippedX())
                {
                    flipsX++;
                    xBox.Text = "Flips X-axis: " + flipsX;
                }
                else if (flippedY())
                {
                    flipsY++;
                    yBox.Text = "Flips Y-axis: " + flipsY;
                }
                else if (flipsX == CODE_X && flipsY == CODE_Y)
                {
                    zBox.Text = "Unlocked!";
                }
            }
        }

        private bool flippedX()
        {
            //if (flipsX != CODE_X && Math.Round(reading.AccelerationY, 2) >= 0.60
            //                && Math.Round(reading.AccelerationY, 2) <= 1.00)
            //{
            //    flipsX = flipsY = 0;
            //    checkerX = checkerY = 0;
            //}
            if (Math.Round(reading.AccelerationX, 2) >= 0.80 &&
                Math.Round(reading.AccelerationX, 2) <= 1.00 && checkerX == 0)
            {
                checkerX = 1;
                //blahBox.Text = "CheckerX: " + checkerX;
            }
            else if (Math.Round(reading.AccelerationX, 2) >= -0.20 &&
                 Math.Round(reading.AccelerationX, 2) <= 0.20 && checkerX == 1)
            {
                checkerX = 2;
                //blahBox.Text = "CheckerX: " + checkerX;
            }
            else if (Math.Round(reading.AccelerationX, 2) <= -0.80 &&
                Math.Round(reading.AccelerationX, 2) >= -1.00 && checkerX == 2)
            {
                checkerX = 3;
                //blahBox.Text = "CheckerX: " + checkerX;
            }
            else if (Math.Round(reading.AccelerationX, 2) >= -0.20 &&
                Math.Round(reading.AccelerationX, 2) <= 0.20 && checkerX == 3)
            {
                checkerX = 0;
                //blahBox.Text = "CheckerX: " + checkerX;
                return true;
            }
            return false;
        }

        private bool flippedY()
        {
            //if (flipsY != CODE_Y && Math.Round(reading.AccelerationX, 2) >= 0.60
            //                && Math.Round(reading.AccelerationX, 2) <= 1.00)
            //{
            //    flipsX = flipsY = 0;
            //    checkerX = checkerY = 0;
            //}
            if (Math.Round(reading.AccelerationY, 2) >= 0.80 &&
                Math.Round(reading.AccelerationY, 2) <= 1.00 && checkerY == 0)
            {
                checkerY = 1;
                //blahBox.Text = "CheckerY: " + checkerY;
            }
            else if (Math.Round(reading.AccelerationY, 2) >= -0.20 &&
                 Math.Round(reading.AccelerationY, 2) <= 0.20 && checkerY == 1)
            {
                checkerY = 2;
                //blahBox.Text = "CheckerY: " + checkerY;
            }
            else if (Math.Round(reading.AccelerationY, 2) <= -0.80 &&
                Math.Round(reading.AccelerationY, 2) >= -1.00 && checkerY == 2)
            {
                checkerY = 3;
                //blahBox.Text = "CheckerY: " + checkerY;
            }
            else if (Math.Round(reading.AccelerationY, 2) >= -0.20 &&
                Math.Round(reading.AccelerationY, 2) <= 0.20 && checkerY == 3)
            {
                checkerY = 0;
                //blahBox.Text = "CheckerY: " + checkerY;
                return true;
            }
            return false;
        }
    }
}
