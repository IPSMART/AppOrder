using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class GeoLocation
    {
        public plus_code plus_code { get; set; }
        public List<Result> results { get; set; }
        public string status { get; set; }
    }
    public class plus_code
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }
    public class Result
    {
        public List<AddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public geometry geometry { get; set; }
        public List<navigation_points> navigation_points { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }
    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }
    public class geometry
    {
        public location location { get; set; }
        public string location_type { get; set; }
        public viewport viewport { get; set; }
    }
    public class location
    {
        public string lat { get; set; }
        public string lng { get; set; }

        public string latitude { get; set; }
        public string longitude { get; set; }
    }
    public class viewport
    {
        public northeast northeast { get; set; }
        public southwest southwest { get; set; }
    }
    public class northeast
    {
        public string lat { get; set; }
        public string lng { get; set; }
    }
    public class southwest
    {
        public string lat { get; set; }
        public string lng { get; set; }
    }
    public class navigation_points
    {
        public location location { get; set; }
    }
}
