using Dubloon.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation.Geofencing;
using Windows.UI.Notifications;

namespace Dubloon
{
    class Toast
    {
        public static void Trigger(string value)
        {
            //foreach (GeofenceStateChangeReport report in GeofenceMonitor.Current.ReadReports())
            //{
            //    Geofence geofence = report.Geofence;
            //    value = geofence.Id.ToString();
            //}

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
