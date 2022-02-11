using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.number {
    public class NamedTimeZone {

        static NamedTimeZone() {
            Saxon.Impl.Helpers.SetUtils.AddAll(knownTimeZones, Saxon.Ejava.time.ZoneId.getAvailableZoneIds());
            tz("us", "America/New_York", true);
            tz("us", "America/Chicago", true);
            tz("us", "America/Denver", true);
            tz("us", "America/Los_Angeles", true);
            tz("us", "America/Anchorage", true);
            tz("us", "America/Halifax", true);
            tz("us", "Pacific/Honolulu", true);
            tz("ca", "Canada/Pacific");
            tz("ca", "Canada/Mountain");
            tz("ca", "Canada/Central");
            tz("ca", "Canada/Eastern");
            tz("ca", "Canada/Atlantic");
            tz("au", "Australia/Sydney", true);
            tz("au", "Australia/Darwin", true);
            tz("au", "Australia/Perth", true);
            tz("ru", "Europe/Moscow", true);
            tz("ru", "Europe/Samara");
            tz("ru", "Asia/Yekaterinburg");
            tz("ru", "Asia/Novosibirsk");
            tz("ru", "Asia/Krasnoyarsk");
            tz("ru", "Asia/Irkutsk");
            tz("ru", "Asia/Chita");
            tz("ru", "Asia/Vladivostok");
            tz("an", "Europe/Andorra");
            tz("ae", "Asia/Abu_Dhabi");
            tz("af", "Asia/Kabul");
            tz("al", "Europe/Tirana");
            tz("am", "Asia/Yerevan");
            tz("ao", "Africa/Luanda");
            tz("ar", "America/Buenos_Aires");
            tz("as", "Pacific/Samoa");
            tz("at", "Europe/Vienna");
            tz("aw", "America/Aruba");
            tz("az", "Asia/Baku");
            tz("ba", "Europe/Sarajevo");
            tz("bb", "America/Barbados");
            tz("bd", "Asia/Dhaka");
            tz("be", "Europe/Brussels", true);
            tz("bf", "Africa/Ouagadougou");
            tz("bg", "Europe/Sofia");
            tz("bh", "Asia/Bahrain");
            tz("bi", "Africa/Bujumbura");
            tz("bm", "Atlantic/Bermuda");
            tz("bn", "Asia/Brunei");
            tz("bo", "America/La_Paz");
            tz("br", "America/Sao_Paulo");
            tz("bs", "America/Nassau");
            tz("bw", "Gaborone");
            tz("by", "Europe/Minsk");
            tz("bz", "America/Belize");
            tz("cd", "Africa/Kinshasa");
            tz("ch", "Europe/Zurich");
            tz("ci", "Africa/Abidjan");
            tz("cl", "America/Santiago");
            tz("cn", "Asia/Shanghai");
            tz("co", "America/Bogota");
            tz("cr", "America/Costa_Rica");
            tz("cu", "America/Cuba");
            tz("cv", "Atlantic/Cape_Verde");
            tz("cy", "Asia/Nicosia");
            tz("cz", "Europe/Prague");
            tz("de", "Europe/Berlin");
            tz("dj", "Africa/Djibouti");
            tz("dk", "Europe/Copenhagen");
            tz("do", "America/Santo_Domingo");
            tz("dz", "Africa/Algiers");
            tz("ec", "America/Quito");
            tz("ee", "Europe/Tallinn");
            tz("eg", "Africa/Cairo");
            tz("er", "Africa/Asmara");
            tz("es", "Europe/Madrid");
            tz("fi", "Europe/Helsinki");
            tz("fj", "Pacific/Fiji");
            tz("fk", "America/Stanley");
            tz("fr", "Europe/Paris");
            tz("ga", "Africa/Libreville");
            tz("gb", "Europe/London");
            tz("gd", "America/Grenada");
            tz("ge", "Asia/Tbilisi");
            tz("gh", "Africa/Accra");
            tz("gm", "Africa/Banjul");
            tz("gn", "Africa/Conakry");
            tz("gr", "Europe/Athens");
            tz("gy", "America/Guyana");
            tz("hk", "Asia/Hong_Kong");
            tz("hn", "America/Tegucigalpa");
            tz("hr", "Europe/Zagreb");
            tz("ht", "America/Port-au-Prince");
            tz("hu", "Europe/Budapest");
            tz("id", "Asia/Jakarta");
            tz("ie", "Europe/Dublin");
            tz("il", "Asia/Tel_Aviv", true);
            tz("in", "Asia/Calcutta", true);
            tz("iq", "Asia/Baghdad");
            tz("ir", "Asia/Tehran");
            tz("is", "Atlantic/Reykjavik");
            tz("it", "Europe/Rome");
            tz("jm", "America/Jamaica");
            tz("jo", "Asia/Amman");
            tz("jp", "Asia/Tokyo", true);
            tz("ke", "Africa/Nairobi");
            tz("kg", "Asia/Bishkek");
            tz("kh", "Asia/Phnom_Penh");
            tz("kp", "Asia/Pyongyang");
            tz("kr", "Asia/Seoul");
            tz("kw", "Asia/Kuwait");
            tz("lb", "Asia/Beirut");
            tz("li", "Europe/Liechtenstein");
            tz("lk", "Asia/Colombo");
            tz("lr", "Africa/Monrovia");
            tz("ls", "Africa/Maseru");
            tz("lt", "Europe/Vilnius");
            tz("lu", "Europe/Luxembourg");
            tz("lv", "Europe/Riga");
            tz("ly", "Africa/Tripoli");
            tz("ma", "Africa/Rabat");
            tz("mc", "Europe/Monaco");
            tz("md", "Europe/Chisinau");
            tz("mg", "Indian/Antananarivo");
            tz("mk", "Europe/Skopje");
            tz("ml", "Africa/Bamako");
            tz("mm", "Asia/Rangoon");
            tz("mn", "Asia/Ulaanbaatar");
            tz("mo", "Asia/Macao");
            tz("mq", "America/Martinique");
            tz("mt", "Europe/Malta");
            tz("mu", "Indian/Mauritius");
            tz("mv", "Indian/Maldives");
            tz("mw", "Africa/Lilongwe");
            tz("mx", "America/Mexico_City");
            tz("my", "Asia/Kuala_Lumpur");
            tz("na", "Africa/Windhoek");
            tz("ne", "Africa/Niamey");
            tz("ng", "Africa/Lagos");
            tz("ni", "America/Managua");
            tz("nl", "Europe/Amsterdam");
            tz("no", "Europe/Oslo");
            tz("np", "Asia/Kathmandu");
            tz("nz", "Pacific/Aukland");
            tz("om", "Asia/Muscat");
            tz("pa", "America/Panama");
            tz("pe", "America/Lima");
            tz("pg", "Pacific/Port_Moresby");
            tz("ph", "Asia/Manila");
            tz("pk", "Asia/Karachi");
            tz("pl", "Europe/Warsaw");
            tz("pr", "America/Puerto_Rico");
            tz("pt", "Europe/Lisbon");
            tz("py", "America/Asuncion");
            tz("qa", "Asia/Qatar");
            tz("ro", "Europe/Bucharest");
            tz("rs", "Europe/Belgrade");
            tz("rw", "Africa/Kigali");
            tz("sa", "Asia/Riyadh");
            tz("sd", "Africa/Khartoum");
            tz("se", "Europe/Stockholm");
            tz("sg", "Asia/Singapore");
            tz("si", "Europe/Ljubljana");
            tz("sk", "Europe/Bratislava");
            tz("sl", "Africa/Freetown");
            tz("so", "Africa/Mogadishu");
            tz("sr", "America/Paramaribo");
            tz("sv", "America/El_Salvador");
            tz("sy", "Asia/Damascus");
            tz("sz", "Africa/Mbabane");
            tz("td", "Africa/Ndjamena");
            tz("tg", "Africa/Lome");
            tz("th", "Asia/Bangkok");
            tz("tj", "Asia/Dushanbe");
            tz("tm", "Asia/Ashgabat");
            tz("tn", "Africa/Tunis");
            tz("to", "Pacific/Tongatapu");
            tz("tr", "Asia/Istanbul");
            tz("tw", "Asia/Taipei");
            tz("tz", "Africa/Dar_es_Salaam");
            tz("ua", "Europe/Kiev");
            tz("ug", "Africa/Kampala");
            tz("uk", "Europe/London", true);
            tz("uy", "America/Montevideo");
            tz("uz", "Asia/Tashkent");
            tz("ve", "America/Caracas");
            tz("vn", "Asia/Hanoi");
            tz("za", "Africa/Johannesburg");
            tz("zm", "Africa/Lusaka");
            tz("zw", "Africa/Harare");
        }
        static System.Collections.Generic.ISet<string> knownTimeZones = new System.Collections.Generic.HashSet<string>(50);
        static System.Collections.Generic.Dictionary<string,System.Collections.Generic.IList<string>> idForCountry = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<string>>(50);
        static System.Collections.Generic.IList<string> worldTimeZones = new System.Collections.Generic.List<string>(20);
        static void tz(string country, string zoneId) {
            System.Collections.Generic.IList<string> list = Saxon.Impl.Helpers.MapUtils.Get(idForCountry, country);
            if ((list == null)) {
                list = new System.Collections.Generic.List<string>(4);
            }
            list.Add(zoneId);
            Saxon.Impl.Helpers.MapUtils.Put(idForCountry, country, list);
        }
        static void tz(string country, string zoneId, bool major) {
            tz(country, zoneId);
            if (major) {
                worldTimeZones.Add(zoneId);
            }
        }
        public static string getTimeZoneNameForDate(Saxon.Hej.value.DateTimeValue date, string place) {
            if (!(date.hasTimezone())) {
                return "";
            }
            Saxon.Ejava.time.ZoneId referenceTimezone = null;
            if (place.StartsWith("America/")) {
                place = "us";
            }
            switch (place) {
              case "us":
                referenceTimezone = Saxon.Ejava.time.ZoneId.of("America/New_York");
                break ;
              case "uk":
              case "gb":
                referenceTimezone = Saxon.Ejava.time.ZoneId.of("Europe/London");
                break ;
            }
            if ((referenceTimezone == null)) {
                return formatTimeZoneOffset(date);
            }
            bool summerTime = inDaylightTime(referenceTimezone, ((long)date.secondsSinceEpoch()));
            int tzMinutes = date.getTimezoneInMinutes();
            if (summerTime) {
                switch (tzMinutes) {
                  case 330:
                    return "IST";
                  case 120:
                    return "CEST";
                  case 60:
                    return "BST";
                  case 0:
                    return "GMT";
                  case -240:
                    return "EDT";
                  case -300:
                    return "CDT";
                  case -360:
                    return "MDT";
                  case -420:
                    return "PDT";
                  case -480:
                    return "AKDT";
                  case -540:
                    return "HDT";
                  default:
                    return formatTimeZoneOffset(date);
                }
            } else {
                switch (tzMinutes) {
                  case 330:
                    return "IST";
                  case 60:
                    return "CET";
                  case 0:
                    return "GMT";
                  case -300:
                    return "EST";
                  case -360:
                    return "CST";
                  case -420:
                    return "MST";
                  case -480:
                    return "PST";
                  case -540:
                    return "AKST";
                  case -600:
                    return "HST";
                  default:
                    return formatTimeZoneOffset(date);
                }
            }
        }
        public static string formatTimeZoneOffset(Saxon.Hej.value.DateTimeValue timeValue) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            Saxon.Hej.value.CalendarValue.appendTimezone(timeValue.getTimezoneInMinutes(), sb);
            return sb.ToString();
        }
        public static string getOlsonTimeZoneName(Saxon.Hej.value.DateTimeValue date, string country) {
            if (!(date.hasTimezone())) {
                return "";
            }
            System.Collections.Generic.IList<string> possibleIds = Saxon.Impl.Helpers.MapUtils.Get(idForCountry, country.ToLower());
            string exampleId;
            if ((possibleIds == null)) {
                return formatTimeZoneOffset(date);
            } else {
                exampleId = possibleIds[0];
            }
            Saxon.Ejava.time.ZoneId exampleZone = Saxon.Ejava.time.ZoneId.of(exampleId);
            bool inSummerTime = inDaylightTime(exampleZone, ((long)date.secondsSinceEpoch()));
            int tzMinutes = date.getTimezoneInMinutes();
            foreach (string olson in possibleIds) {
                Saxon.Ejava.time.ZoneId possibleTimeZone = Saxon.Ejava.time.ZoneId.of(olson);
                int offsetSeconds = getOffsetInSecondsAtDateTime(possibleTimeZone, date);
                if ((offsetSeconds == (tzMinutes*60))) {
                    return (inSummerTime ? (olson+"*") : olson);
                }
            }
            return formatTimeZoneOffset(date);
        }
        public static Saxon.Ejava.util.Optional<bool> inSummerTime(Saxon.Hej.value.DateTimeValue date, string region) {
            string olsonName;
            if ((region.Length == 2)) {
                System.Collections.Generic.IList<string> possibleIds = Saxon.Impl.Helpers.MapUtils.Get(idForCountry, region.ToLower());
                if ((possibleIds == null)) {
                    return Saxon.Ejava.util.Optional<bool>.empty();
                } else {
                    olsonName = possibleIds[0];
                }
            } else {
                olsonName = region;
            }
            Saxon.Ejava.time.ZoneId zone = olsonZoneOrUtc(olsonName);
            return Saxon.Ejava.util.Optional<bool>.of(inDaylightTime(zone, ((long)date.secondsSinceEpoch())));
        }
        private static Saxon.Ejava.time.ZoneId olsonZoneOrUtc(string olsonName) {
            try {
                return Saxon.Ejava.time.ZoneId.of(olsonName);
            } catch (/* substitute exception */ System.Exception e) {
                return Saxon.Ejava.time.ZoneId.of("UTC");
            }
        }
        private static bool inDaylightTime(Saxon.Ejava.time.ZoneId zone, long secondsSinceEpoch) {
            return zone.inDaylightTime(secondsSinceEpoch);
        }
        public static int civilTimeOffsetInSeconds(Saxon.Hej.value.DateTimeValue date, string olsonName) {
            Saxon.Ejava.time.ZoneId zone = olsonZoneOrUtc(olsonName);
            return getOffsetInSecondsAtDateTime(zone, date);
        }
        public static Saxon.Ejava.time.ZoneId getNamedTimeZone(string olsonName) {
            if (knownTimeZones.Contains(olsonName)) {
                return Saxon.Ejava.time.ZoneId.of(olsonName);
            } else {
                return null;
            }
        }
        private static int getOffsetInSecondsAtDateTime(Saxon.Ejava.time.ZoneId zone, Saxon.Hej.value.DateTimeValue dateTime) {
            return zone.getOffsetInSecondsAtDateTime(dateTime);
        }
    
    }
}