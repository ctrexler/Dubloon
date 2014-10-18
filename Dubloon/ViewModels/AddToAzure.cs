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
        public async void AddHuntToAzure(TableHunts item)
        {
            await App.MobileService.GetTable<TableHunts>().InsertAsync(item);
        }

        public async void AddTrailToAzure(TableTrails item)
        {
            await App.MobileService.GetTable<TableTrails>().InsertAsync(item);
        }
        public async void AddNodeToAzure(TableNodes item)
        {
            await App.MobileService.GetTable<TableNodes>().InsertAsync(item);
        }
    }
}
