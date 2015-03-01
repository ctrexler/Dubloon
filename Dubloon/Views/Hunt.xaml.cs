using Dubloon.Common;
using Dubloon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
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
    public sealed partial class Hunt : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        ObservableCollection<TableTrails> trails = new ObservableCollection<TableTrails>();

        public Hunt()
        {
            this.InitializeComponent();
            Initialize();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public async void Initialize()
        {
            var trailsResponse = await ViewModels.PullFromAzure.PullTrailsFromAzure();
            foreach (TableTrails t in trailsResponse.Where(id => id.HuntId == PassedData.Id))
            {
                trails.Add(t);
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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Trail));
        }

        private void ListViewTrails_Loaded(object sender, RoutedEventArgs e)
        {
            this.ListViewTrails = (ListView)sender;
            this.ListViewTrails.ItemsSource = trails;
        }
        private void ButtonCreateTrail_Click(object sender, RoutedEventArgs e)
        {
            FormCreateTrail.Visibility = Visibility.Visible;
            ButtonCreateTrail.Visibility = Visibility.Collapsed;
            ButtonCancelTrail.Visibility = Visibility.Visible;
            ButtonSubmitTrail.Visibility = Visibility.Visible;

            InputName.Text = "";
        }
        private void ButtonCancelTrail_Click(object sender, RoutedEventArgs e)
        {
            FormCreateTrail.Visibility = Visibility.Collapsed;
            ButtonCreateTrail.Visibility = Visibility.Visible;
            ButtonCancelTrail.Visibility = Visibility.Collapsed;
            ButtonSubmitTrail.Visibility = Visibility.Collapsed;
        }
        private async void ButtonSubmitTrail_Click(object sender, RoutedEventArgs e)
        {
            var trailsResponse = await ViewModels.PullFromAzure.PullTrailsFromAzure();
            if (!trailsResponse.Any(t => t.Name == InputName.Text))
            {
                var item = await ViewModels.AddToAzure.AddTrailToAzure(InputName.Text, PassedData.Id);
                trails.Add(item);
                System.Diagnostics.Debug.WriteLine("Sent to azure!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Already in the list!");
            }

            FormCreateTrail.Visibility = Visibility.Collapsed;
            ButtonCreateTrail.Visibility = Visibility.Visible;
            ButtonCancelTrail.Visibility = Visibility.Collapsed;
            ButtonSubmitTrail.Visibility = Visibility.Collapsed;
        }
        private void ListViewTrails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Frame.Navigate(typeof(Views.Trail));
            var ListViewSelection = e.AddedItems.Cast<TableTrails>().ToList().First();
            Views.PassedData.Title = ListViewSelection.Name;
            Views.PassedData.Id = ListViewSelection.HuntId;
        }
    }
}
