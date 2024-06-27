using FMSC.Core;
using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial.UTM
{
    [Serializable]
    public class UtmExtent
    {
        public UTMCoords NorthEast { get; set; }
        public UTMCoords SouthWest { get; set; }
        public Int32 Zone { get; private set; }
        public Datum Datum { get; private set; }

        public Double North => NorthEast.Y;
        public Double South => SouthWest.Y;
        public Double East => NorthEast.X;
        public Double West => SouthWest.X;

        public UtmExtent(UTMCoords northEast, UTMCoords southWest)
        {
            if (northEast.Zone != southWest.Zone)
                throw new Exception("Mismatched Zone");

            if (northEast.Datum != southWest.Datum)
                throw new Exception("Mismatched Datum");

            this.NorthEast = northEast;
            this.SouthWest = southWest;
            this.Zone = northEast.Zone;
            this.Datum = northEast.Datum;
        }

        public UtmExtent(double north, double east, double south, double west, int zone, Datum datum = Datum.WGS84)
        {
            this.NorthEast = new UTMCoords(east, north, datum, zone);
            this.SouthWest = new UTMCoords(west, south, datum, zone);
            this.Zone = zone;
            this.Datum = datum;
        }


        public class Builder
        {
            List<double> xpos = new List<double>();
            List<double> ypos = new List<double>();
            int zone;
            Datum datum;

            public Builder(int zone, Datum datum = Datum.WGS84)
            {
                this.zone = zone;
                this.datum = datum;
            }

            public void Include(double x, double y)
            {
                xpos.Add(x);
                ypos.Add(y);
            }

            public void Include(Point point)
            {
                xpos.Add(point.X);
                ypos.Add(point.Y);
            }

            public void Include(IEnumerable<Point> points)
            {
                foreach (Point p in points)
                    Include(p);
            }

            public void Include(UTMCoords position)
            {
                if (position.Zone != zone)
                    throw new Exception("Mismatched Zone");

                if (position.Datum != datum)
                    throw new Exception("Mismatched Datum");

                xpos.Add(position.X);
                ypos.Add(position.Y);
            }

            public void Include(IEnumerable<UTMCoords> coords)
            {
                foreach (UTMCoords c in coords)
                    Include(c);
            }

            public void Include(UtmExtent extent)
            {
                if (extent.NorthEast.Zone != zone || extent.SouthWest.Zone != zone)
                    throw new Exception("Mismatched Zone");

                if (extent.NorthEast.Datum != datum || extent.SouthWest.Datum != datum)
                    throw new Exception("Mismatched Zone");

                xpos.Add(extent.East);
                xpos.Add(extent.West);
                ypos.Add(extent.North);
                ypos.Add(extent.South);
            }

            public void Include(IEnumerable<UtmExtent> extents)
            {
                foreach (UtmExtent e in extents)
                    Include(e);
            }

            public UtmExtent Build()
            {
                double north = double.NegativeInfinity,
                    south = double.PositiveInfinity,
                    east = double.NegativeInfinity,
                    west = double.PositiveInfinity;

                if (xpos.Count < 1)
                    throw new Exception("No positions");

                foreach (double y in ypos)
                {
                    if (y > north)
                        north = y;

                    if (y < south)
                        south = y;
                }

                foreach (double x in xpos)
                {
                    if (x > east)
                        east = x;

                    if (x < west)
                        west = x;
                }

                return new UtmExtent(north, east, south, west, zone, datum);
            }
        }
    }
}
