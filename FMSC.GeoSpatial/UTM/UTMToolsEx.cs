using FMSC.Core;
using SharpProj;
using SharpProj.Proj;
using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial.UTM
{
    public static class UTMToolsEx
    {
        private const int WGS84_GPS_EPSG = 4326;
        private const int WGS84_Zone_North_EPSG_Start = 32600;
        private const int WGS84_Zone_South_EPSG_Start = 32700;
        private const int NAD83_Zone_EPSG_Start = 26900;

        private static readonly CoordinateReferenceSystem WGS84_GPS = CoordinateReferenceSystem.CreateFromEpsg(WGS84_GPS_EPSG);

        private static readonly Dictionary<int, CoordinateReferenceSystem> WGS84_UTM_NORTH_ZONES = new Dictionary<int, CoordinateReferenceSystem>();
        private static readonly Dictionary<int, CoordinateReferenceSystem> WGS84_UTM_SOUTH_ZONES = new Dictionary<int, CoordinateReferenceSystem>();
        private static readonly Dictionary<int, CoordinateReferenceSystem> NAD83_UTM_ZONES = new Dictionary<int, CoordinateReferenceSystem>();

        private static readonly Dictionary<string, CoordinateTransform> TRANSFORMATIONS = new Dictionary<string, CoordinateTransform>();


        private static CoordinateReferenceSystem GetWGS84CRS(int zone, NorthSouth northSouth)
        {
            CoordinateReferenceSystem crs = null;

            if (zone > 0 && zone < 61)
            {
                if (northSouth == NorthSouth.North)
                {
                    if (!WGS84_UTM_NORTH_ZONES.ContainsKey(zone))
                    {
                        crs = CoordinateReferenceSystem.CreateFromEpsg(WGS84_Zone_North_EPSG_Start + zone);
                        WGS84_UTM_NORTH_ZONES.Add(zone, crs);
                    }

                    return crs ?? WGS84_UTM_NORTH_ZONES[zone];
                }
                else
                {
                    if (!WGS84_UTM_SOUTH_ZONES.ContainsKey(zone))
                    {
                        crs = CoordinateReferenceSystem.CreateFromEpsg(WGS84_Zone_South_EPSG_Start + zone);
                        WGS84_UTM_SOUTH_ZONES.Add(zone, crs);
                    }

                    return crs ?? WGS84_UTM_SOUTH_ZONES[zone];
                }
            }

            throw new Exception("Invalid WGS84 Zone");
        }

        private static CoordinateReferenceSystem GetNAD83CRS(int zone)
        {
            CoordinateReferenceSystem crs = null;

            if (zone > 0 && zone < 24)
            {
                    if (!NAD83_UTM_ZONES.ContainsKey(zone))
                    {
                        crs = CoordinateReferenceSystem.CreateFromEpsg(NAD83_Zone_EPSG_Start + zone);
                        NAD83_UTM_ZONES.Add(zone, crs);
                    }

                    return crs ?? NAD83_UTM_ZONES[zone];
            }

            throw new Exception("Invalid NAD83 Zone");
        }


        private static CoordinateReferenceSystem GetCRS(Datum datum, int zone, NorthSouth northSouth)
        {
            switch (datum)
            {
                case Datum.WGS84: return GetWGS84CRS(zone, northSouth);
                case Datum.NAD83: return GetNAD83CRS(zone);
            }

            throw new NotImplementedException("Datum not supported");
        }


        private static CoordinateTransform GetTransformation(CoordinateReferenceSystem from, CoordinateReferenceSystem to)
        {
            string id = from.Name + to.Name;

            if (TRANSFORMATIONS.ContainsKey(id))
            {
                return TRANSFORMATIONS[id];
            }

            CoordinateTransform transform = CoordinateTransform.Create(from, to);
            TRANSFORMATIONS.Add(id, transform);

            return transform;
        }


        private static int GetUTMZone(Position position) => GetUTMZone(position.Latitude, position.Longitude);

        private static int GetUTMZone(double latitude, double longitude)
        {
            double longitudeTemp = (longitude + 180) - ((int)((longitude + 180) / 360)) * 360 - 180; // -180.00 .. 179.9;

            int zone = ((int)((longitudeTemp + 180) / 6)) + 1;

            if (latitude >= 56.0 && latitude < 64.0 && longitudeTemp >= 3.0 && longitudeTemp < 12.0)
                zone = 32;

            // Special zones for Svalbard
            if (latitude >= 72.0 && latitude < 84.0)
            {
                if (longitudeTemp >= 0.0 && longitudeTemp < 9.0) zone = 31;
                else if (longitudeTemp >= 9.0 && longitudeTemp < 21.0) zone = 33;
                else if (longitudeTemp >= 21.0 && longitudeTemp < 33.0) zone = 35;
                else if (longitudeTemp >= 33.0 && longitudeTemp < 42.0) zone = 37;
                }

            return zone;
        }
            



        public static UTMCoords GetUTM(Position position) =>
            GetUTM(position.Latitude, position.Longitude);

        public static UTMCoords GetUTM(Latitude latitude, Longitude longitude) =>
            GetUTM(latitude.toSignedDecimal(), longitude.toSignedDecimal());

        public static UTMCoords GetUTM(double latitude, double longitude, Datum datum = Datum.WGS84, int targetZone = 0)
        {
            int zone = targetZone > 0 && targetZone < 61 ? targetZone : GetUTMZone(latitude, longitude);

            return new UTMCoords(
                GetTransformation(
                    WGS84_GPS,
                    GetCRS(
                        datum,
                        zone,
                        NorthSouthEx.FromLatitude(latitude)
                    ))
                .Apply(new double[] { latitude, longitude }), Datum.WGS84, zone);
        }


        public static Position GetLatLong(UTMCoords coords, NorthSouth northSouth = NorthSouth.North)
        {
            return new Position(GetLatLongAsPoint(coords, northSouth));
        }

        public static Point GetLatLongAsPoint(UTMCoords coords, NorthSouth northSouth = NorthSouth.North)
        {
            double[] ords = GetTransformation(
                    GetCRS(
                        coords.Datum,
                        coords.Zone,
                        northSouth
                    ),
                    WGS84_GPS)
                .Apply(new double[] { coords.X, coords.Y });

            return new Point(ords[1], ords[0]);
        }


        public static UTMCoords TransformCoords(UTMCoords coords, Datum datumTo, int zoneTo)
        {
            double[] ords = GetTransformation(
                    GetCRS(
                        coords.Datum,
                        coords.Zone,
                        NorthSouth.North
                    ),
                    GetCRS(
                        datumTo,
                        zoneTo,
                        NorthSouth.North
                    ))
                .Apply(new double[] { coords.X, coords.Y });

            return new UTMCoords(ords[1], ords[0], datumTo, zoneTo);
        }
    }
}
