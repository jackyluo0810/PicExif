using System;

namespace PicExif.Services
{
    public class CoordinateConverterService
    {
        private const double Pi = Math.PI;
        private const double A = 6378137.0;
        private const double E = 0.00669342162296594323;

        public (double lat, double lng) Wgs84ToGcj02(double wgsLat, double wgsLng)
        {
            if (IsOutOfChina(wgsLat, wgsLng))
            {
                return (wgsLat, wgsLng);
            }

            double dLat = TransformLat(wgsLng - 105.0, wgsLat - 35.0);
            double dLng = TransformLng(wgsLng - 105.0, wgsLat - 35.0);

            double radLat = wgsLat / 180.0 * Pi;
            double magic = Math.Sin(radLat);
            magic = 1 - E * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);

            dLat = (dLat * 180.0) / ((A * (1 - E)) / (magic * sqrtMagic) * Pi);
            dLng = (dLng * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * Pi);

            double gcjLat = wgsLat + dLat;
            double gcjLng = wgsLng + dLng;

            return (gcjLat, gcjLng);
        }

        private bool IsOutOfChina(double lat, double lng)
        {
            if (lng < 72.004 || lng > 137.8347)
                return true;
            if (lat < 0.8293 || lat > 55.8271)
                return true;
            return false;
        }

        private double TransformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * Pi) + 40.0 * Math.Sin(y / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * Pi) + 320 * Math.Sin(y * Pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        private double TransformLng(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * Pi) + 40.0 * Math.Sin(x / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * Pi) + 300.0 * Math.Sin(x / 30.0 * Pi)) * 2.0 / 3.0;
            return ret;
        }
    }
}
