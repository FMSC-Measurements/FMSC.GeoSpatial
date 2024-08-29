using System;

namespace FMSC.GeoSpatial
{
    public enum Datum
    {
        NAD83 = 0,
        WGS84 = 1,
        NSRS = 2
    }

    public static partial class GeoSpatialTypes
    {
        public static Datum ParseDatum(string value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "nad83": return Datum.NAD83;
                case "1":
                case "wgs84": return Datum.WGS84;
                case "2":
                case "nsrs": return Datum.NSRS;
            }

            throw new Exception("Unknown Datum");
        }
    }
}
