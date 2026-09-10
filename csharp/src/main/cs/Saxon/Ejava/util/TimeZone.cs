using System;
using Saxon.Hej.trans;
using SysCollGen = System.Collections.Generic;

namespace Saxon.Ejava.util {
    /// <summary>
    /// Implements a subset of the functionality of Java's Saxon.Ejava.util.TimeZone as a wrapper over
    /// the .NET TimeZoneInfo class. Note that some methods are slightly different from the Java
    /// equivalent, to avoid unnecessary use of Java Date/Time classes.
    /// </summary>
    /// <remarks>
    /// Note that since .NET Core 6, .NET supports both Windows and IANA (Olson) timezone names, which
    /// simplifies the logic.
    /// </remarks>
    public class TimeZone {
        private readonly TimeZoneInfo timeZoneInfo;

        private TimeZone(TimeZoneInfo info) {
            this.timeZoneInfo = info;
        }

        public static TimeZone getDefault() {
            return new TimeZone(TimeZoneInfo.Local);
        }

        // A few timezones have conventional short names, especially those used in the US. This table gives a mapping
        // from the Windows time zone name (specifically, the Name property of the System.TimeZoneInfo object) to the
        // short name.
        
        private static SysCollGen.Dictionary<string, string> shortNames = new SysCollGen.Dictionary<string, string> {
            {"Atlantic Standard Time", "AST"},
            {"Atlantic Daylight Time", "ADT"},
            {"Eastern Standard Time", "EST"},
            {"Eastern Daylight Time", "EDT"},
            {"Central Standard Time", "CST"},
            {"Central Daylight Time", "CDT"},
            {"Mountain Standard Time", "MST"},
            {"Mountain Daylight Time", "MDT"},
            {"Pacific Standard Time", "PST"},
            {"Pacific Daylight Time", "PDT"},
            {"Hawaii-Aleutian Standard Time", "HST"},
            {"Hawaii-Aleutian Daylight Time", "HDT"},
            {"Greenwich Mean Time", "GMT"},
            {"British Summer Time", "BST"},
            {"Central European Standard Time", "CEST"},
            {"India Standard Time", "IST"}
        };

        
        public static TimeZone getTimeZone(string place) {
            // .NET Core 6 supports both IANA (Olson) timezone names, and Windows timezone names
            // So we no longer need to try both.
            try {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(place);
                return new TimeZone(info);
            }
            catch (TimeZoneNotFoundException) {
                throw new XPathException("Unknown timezone " + place);
            }
        }
        public static bool isAvailableID(string id) {
            try {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(id);
                return true;
            }
            catch (TimeZoneNotFoundException) {
                return false;
            }
        }

        public int getOffset(long millisecsSinceEpoch) {
            // Console.Error.WriteLine("TZ datum " + JavaTimeStampToDateTime(date));
            // Console.Error.WriteLine("TZ offset Timespan " + timeZoneInfo.GetUtcOffset(JavaTimeStampToDateTime(date)));
            return (int) timeZoneInfo.GetUtcOffset(JavaTimeStampToDateTime(millisecsSinceEpoch)).TotalMilliseconds;
        }

        private static System.DateTime JavaTimeStampToDateTime(long javaTimeStamp) {
            // Java timestamp is milliseconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public string getDisplayName(bool summerTime, int format) {
            string longName = summerTime ? timeZoneInfo.DaylightName : timeZoneInfo.StandardName;
            if (shortNames.ContainsKey(longName)) {
                return shortNames[longName];
            }
            else {
                return null;
            }
        }

        public bool inDaylightTime(long millisecsSinceEpoch) {
            return timeZoneInfo.SupportsDaylightSavingTime &&
                   timeZoneInfo.IsDaylightSavingTime(JavaTimeStampToDateTime(millisecsSinceEpoch));
        }
    }
}