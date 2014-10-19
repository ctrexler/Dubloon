using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dubloon.Models;

namespace Dubloon.ViewModels
{
    class AddToAzure
    {
        public static async Task<TableHunts> AddHuntToAzure(string title, string author, string description, double difficulty, int duration = 5)
        {
            TableHunts item = new TableHunts
            {
                Title = title,
                Author = author,
                Description = description,
                Difficulty = difficulty,
                Duration = duration
            };
            await App.MobileService.GetTable<TableHunts>().InsertAsync(item);
            return item;
        }

        public static async Task<TableTrails> AddTrailToAzure(string name, string huntid)
        {
            TableTrails item = new TableTrails
            {
                Name = name,
                HuntId = huntid,
            };
            await App.MobileService.GetTable<TableTrails>().InsertAsync(item);
            return item;
        }
        public static async void AddNodeToAzure(string name, double latitude, double longitude, int radius, string trailid)
        {
            TableNodes item = new TableNodes
            {
                Name = name,
                Latitude = latitude,
                Longitude = longitude,
                Radius = radius,
                TrailId = trailid
            };
            await App.MobileService.GetTable<TableNodes>().InsertAsync(item);
        }
    }
}
