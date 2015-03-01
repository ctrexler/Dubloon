using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dubloon.Models
{
    public class TableHunts
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public double Difficulty { get; set; }
        public int Duration { get; set; }
    }

    public class TableTrails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HuntId { get; set; }
    }

    public class TableNodes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public string TrailId { get; set; }
    }
}
