using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace BackgroundTasks
{
    public sealed class GeofenceBackgroundTask : IBackgroundTask
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            System.Diagnostics.Debug.WriteLine("Triggered from background!");
            string value = "";
            foreach (GeofenceStateChangeReport report in GeofenceMonitor.Current.ReadReports())
            {
                Geofence geofence = report.Geofence;
                value = geofence.Id.ToString();
                localSettings.Values["geofenceId"] = value;
            }

            System.Diagnostics.Debug.WriteLine("Toast");
            var toastTemplate = ToastTemplateType.ToastText02;
            var toastXML = ToastNotificationManager.GetTemplateContent(toastTemplate);
            var textElements = toastXML.GetElementsByTagName("text");
            textElements[0].AppendChild(toastXML.CreateTextNode("You triggered " + value + "!"));

            var toast = new ToastNotification(toastXML);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
            
        }
    }
}
