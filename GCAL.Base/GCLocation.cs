﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class GCLocation: GSCore
    {
        private string p_timezonename = "";
        private TTimeZone p_timezone = null;

        public string Title;
        public double Longitude;
        public double Latitude;
        public double OffsetUtcHours
        {
            get
            {
                if (TimeZone == null)
                    return 0.0;
                return TimeZone.OffsetMinutes / 60.0;
            }
        }
        public string TimeZoneName
        {
            get
            {
                return p_timezonename;
            }
            set
            {
                p_timezonename = value; p_timezone = null;
            }
        }
        public TTimeZone TimeZone
        {
            get
            {
                if (p_timezone != null) return p_timezone;
                p_timezone = TTimeZone.FindTimeZoneByName(p_timezonename);
                return p_timezone;
            }
            set
            {
                p_timezone = value;
                p_timezonename = p_timezone.Name;
            }
        }
        // !!!!!!!!
        // when adding new properties, dont forget to modify EncodedString and DefaultEncodedString properties


        public GCLocation()
        {
            Title = string.Empty;
            TimeZone = TTimeZone.GetDefaultTimeZone();
        }

        public GCLocation(GCLocation cl)
        {
            Title = string.Empty;
            TimeZone = TTimeZone.GetDefaultTimeZone();
            Set(cl);
        }

        public override bool Equals(object obj)
        {
            if (obj is GCLocation)
            {
                GCLocation arg = obj as GCLocation;
                return arg.Title.Equals(Title) 
                    && arg.TimeZoneName.Equals(TimeZoneName)
                    && arg.GetLatitudeString().Equals(GetLatitudeString()) 
                    && arg.GetLongitudeString().Equals(GetLongitudeString());
            }
            else if (obj is string)
            {
                return (obj as string).Equals(GetFullName());
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode() + TimeZoneName.GetHashCode() + Latitude.GetHashCode() + Longitude.GetHashCode();
        }

        public GCEarthData GetEarthData()
        {
            GCEarthData e = new GCEarthData();
            e.TimeZone = TimeZone;
            e.latitudeDeg = Latitude;
            e.longitudeDeg = Longitude;
            return e;
        }

        public GCEarthData SetEarthData(GCEarthData e)
        {
            Longitude = e.longitudeDeg;
            Latitude = e.latitudeDeg;
            TimeZone = e.TimeZone;
            return e;
        }

        public void Set(GCLocation L)
        {
            if (L.p_timezone != null)
                this.TimeZone = L.TimeZone;
            else
                this.TimeZoneName = L.TimeZoneName;
            Title = L.Title;
            Longitude = L.Longitude;
            Latitude = L.Latitude;
        }

        public override GSCore GetPropertyValue(string Token)
        {
            GSCore result = null;
            switch(Token)
            {
                case "latitude":
                    result = new GSNumber() { DoubleValue = this.Latitude };
                    break;
                case "longitude":
                    result = new GSNumber() { DoubleValue = this.Longitude };
                    break;
                case "latitudeString":
                    result = new GSString() { Value = this.GetLatitudeString() };
                    break;
                case "longitudeString":
                    result = new GSString() { Value = this.GetLongitudeString() };
                    break;
                case "name":
                    result = new GSString() { Value = this.Title };
                    break;
                case "timezoneName":
                    result = new GSString() { Value = this.TimeZoneName };
                    break;
                case "fullName":
                    result = new GSString(this.GetFullName());
                    break;
                default:
                    result = base.GetPropertyValue(Token);
                    break;
            }

            return result;
        }

        public string GetLatitudeString()
        {
            return GCEarthData.GetTextLatitude(Latitude);
        }

        public string GetLongitudeString()
        {
            return GCEarthData.GetTextLongitude(Longitude);
        }

        public string GetFullName()
        {
            return string.Format("{0} ({1}, {2}, {3}: {4})"
                    , Title
                    , GCEarthData.GetTextLatitude(Latitude)
                    , GCEarthData.GetTextLongitude(Longitude)
                    , GCStrings.Localized("Timezone")
                    , TimeZoneName);
        }


        public string GetNameAsFileName()
        {
            StringBuilder sb = new StringBuilder();
            int m = 0;
            foreach (char c in Title)
            {
                if (Char.IsLetter(c))
                {
                    if (m == 0)
                        sb.Append(Char.ToUpper(c));
                    else
                        sb.Append(Char.ToLower(c));
                    m = 1;
                }
                else
                {
                    m = 0;
                }
            }
            return sb.ToString();
        }

        public string Format(string format, params string[] args)
        {
            StringBuilder sb = new StringBuilder(format);

            if (format.IndexOf("{locationName}") >= 0)
                format = format.Replace("{locationName}", Title);
            if (format.IndexOf("{longitudeDeg}") >= 0)
                format = format.Replace("{longitudeDeg}", Longitude.ToString());
            if (format.IndexOf("{latitudeDeg}") >= 0)
                format = format.Replace("{latitudeDeg}", Latitude.ToString());
            if (format.IndexOf("{longitudeText}") >= 0)
                format = format.Replace("{longitudeText}", GetLongitudeString());
            if (format.IndexOf("{latitudeText}") >= 0)
                format = format.Replace("{latitudeText}", GetLatitudeString());


            if (format.IndexOf("{timeZoneName}") >= 0)
                format = format.Replace("{timeZoneName}", TimeZoneName);

            if (args == null || args.Length == 0)
                return format.ToString();
            else
                return string.Format(format.ToString(), args);
        }

        public string EncodedString
        {
            get
            {
                if (Title == null)
                    return string.Empty;
                return string.Format("{0}|{1}|{2}|{3}", GCFestivalBase.StringToSafe(Title),
                    Longitude, Latitude, TimeZoneName);
            }
            set
            {
                if (value == null)
                    Title = null;
                else
                {
                    string[] a = value.Split('|');
                    if (a.Length >= 4)
                    {
                        double LO, LA;
                        if (double.TryParse(a[1], out LO)
                            && double.TryParse(a[2], out LA))
                        {
                            Title = GCFestivalBase.SafeToString(a[0]);
                            Longitude = LO;
                            Latitude = LA;
                            TimeZoneName = a[3];
                        }
                    }
                }
            }
        }

        public static string DefaultEncodedString
        {
            get
            {
                return "Vrindavan (India)|77.73|27.583|Asia/Calcutta";
            }
        }
    }
}
