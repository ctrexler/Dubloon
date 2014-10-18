using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dubloon.Models;

namespace Dubloon.ViewModels
{
    class PullFromAzure
    {
        public async Task<List<TableHunts>> PullHuntsFromAzure()
        {
            return await App.MobileService.GetTable<TableHunts>().ToListAsync();
        }

        public async Task<List<TableTrails>> PullTrailsFromAzure()
        {
            return await App.MobileService.GetTable<TableTrails>().ToListAsync();
        }
        public async Task<List<TableNodes>> PullNodesFromAzure()
        {
            return await App.MobileService.GetTable<TableNodes>().ToListAsync();
        }
    }
}
