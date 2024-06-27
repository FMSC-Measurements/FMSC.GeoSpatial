using FMSC.Core;
using System;

namespace FMSC.GeoSpatial.UTM
{
    [Serializable]
    public struct UTMCoords
    {
        public Double X { get; }
        public Double Y { get; }

        public Int32 Zone { get; }

        public Datum Datum { get; }


        public UTMCoords(double x, double y, Datum datum, int zone)
        {
            X = x;
            Y = y;
            Datum = datum;
            Zone = zone;
        }

        public UTMCoords(double[] coords, Datum datum, int zone)
        {
            if (coords == null || coords.Length < 2) throw new Exception("Invalid number of ordinates");

            X = coords[0];
            Y = coords[1];
            Datum = datum;
            Zone = zone;
        }


        public Point ToPoint() => new Point(X, Y);
    }
}
