using FMSC.Core;
using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Latitude LatitudeDMS => new Latitude(Latitude);
        public Longitude LongitudeDMS => new Longitude(Longitude);


        public bool IsNorth => Latitude >= 0;
        public bool IsWest => Longitude < 0;

        public NorthSouth LatDir => IsNorth ? NorthSouth.North : NorthSouth.South;
        public EastWest LonDir => IsWest ? EastWest.West : EastWest.East;


        public Position() { }

        public Position(Position position) : this(position.Latitude, position.Longitude) { }

        public Position(Latitude latitude, Longitude longitude) : this(latitude.toSignedDecimal(), longitude.toSignedDecimal()) { }

        public Position(Point point) : this(point.Y, point.X) { }

        public Position(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }



        public override string ToString()
        {
            return $"Lat: {Latitude} Lon: {Longitude}";
        }
    }
}
