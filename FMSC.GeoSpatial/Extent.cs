using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Extent
    {
        public Position NorthEast { get; set; }
        public Position SouthWest { get; set; }
        public Position Center { get; set; }

        public Double North => NorthEast.Latitude;
        public Double South => SouthWest.Latitude;
        public Double East => NorthEast.Longitude;
        public Double West => SouthWest.Longitude;


        public Extent(Position northEast, Position southWest)
        {
            this.NorthEast = northEast;
            this.SouthWest = southWest;

            Center = GeoTools.getGeoMidPoint(northEast, southWest);
        }

        public Extent(double north, double east, double south, double west) :
            this(new Position(north, east), new Position(south, west)) { }


        public class Builder
        {
            List<double> lats = new List<double>();
            List<double> lons = new List<double>();

            public bool HasPositions => lats.Count > 0;

            public void Include(double latitude, double longitude)
            {
                lats.Add(latitude);
                lons.Add(longitude);
            }

            public void Include(Position position)
            {
                lats.Add(position.Latitude);
                lons.Add(position.Longitude);
            }

            public void Include(Extent extent)
            {
                Include(extent.NorthEast);
                Include(extent.SouthWest);
            }

            public Extent Build()
            {
                double north, south, east, west;

                if (lats.Count < 1)
                    throw new Exception("No positions");

                north = south = lats[0];

                foreach (double lat in lats)
                {
                    if (lat > north)
                        north = lat;

                    if (lat < south)
                        south = lat;
                }

                east = west = lons[0];

                foreach (double lon in lons)
                {
                    if (lon > east)
                        east = lon;

                    if (lon < west)
                        west = lon;
                }

                return new Extent(north, east, south, west);
            }
        }
    }
}
