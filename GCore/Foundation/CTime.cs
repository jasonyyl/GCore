using System;
using System.Text;
using System.Globalization;

namespace GCore
{
    public class CTime
    {
        public static DateTime ConvertToDateTime(long lTotalSeconds, DateTimeKind kind = 0)
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, kind);
            return time.AddSeconds((double)lTotalSeconds);
        }

        public static long ConvertToTimestamp(DateTime theDateTime, DateTimeKind kind = 0)
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, kind);
            TimeSpan span = (TimeSpan)(theDateTime - time);
            return (long)span.TotalSeconds;
        }

        public static DateTime GetDateTimeFromString(string sFullDateTime, char cDash = '-', char cSpace = ' ', char cColon = ':')
        {
            DateTime time;
            string format = string.Concat(new object[] { "yyyy", cDash, "MM", cDash, "dd", cSpace, "HH", cColon, "mm", cColon, "ss" });
            if (DateTime.TryParseExact(sFullDateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return time;
            }
            return new DateTime();
        }

        public static string GetDateTimeStr(bool bYear = true, bool bMonth = true, bool bDay = true, bool bHour = true, bool bMin = true, bool bSec = true, string sDash = "-", string sSpace = " ", string sColon = ":")
        {
            return GetDateTimeStr(DateTime.Now, bYear, bMonth, bDay, bHour, bMin, bSec, sDash, sSpace, sColon);
        }

        public static string GetDateTimeStr(DateTime now, bool bYear = true, bool bMonth = true, bool bDay = true, bool bHour = true, bool bMin = true, bool bSec = true, string sDash = "-", string sSpace = " ", string sColon = ":")
        {
            StringBuilder sbTime = new StringBuilder(20);
            GetDateTimeStr(ref sbTime, now, bYear, bMonth, bDay, bHour, bMin, bSec, sDash, sSpace, sColon);
            return sbTime.ToString();
        }

        public static void GetDateTimeStr(ref StringBuilder sbTime, DateTime now, bool bYear = true, bool bMonth = true, bool bDay = true, bool bHour = true, bool bMin = true, bool bSec = true, string sDash = "-", string sSpace = " ", string sColon = ":")
        {
            if (bYear)
            {
                sbTime.Append(now.Year.ToString("D2"));
            }
            if (bMonth)
            {
                if (bYear)
                {
                    sbTime.Append(sDash);
                }
                sbTime.Append(now.Month.ToString("D2"));
            }
            if (bDay)
            {
                if (bMonth)
                {
                    sbTime.Append(sDash);
                }
                else if (bYear)
                {
                    sbTime.Append(sDash);
                    sbTime.Append(sDash);
                }
                sbTime.Append(now.Day.ToString("D2"));
            }
            bool flag = (bYear || bMonth) || bDay;
            bool flag2 = (bHour || bMin) || bSec;
            if (flag && flag2)
            {
                sbTime.Append(sSpace);
            }
            if (bHour)
            {
                sbTime.Append(now.Hour.ToString("D2"));
            }
            if (bMin)
            {
                if (bHour)
                {
                    sbTime.Append(sColon);
                }
                sbTime.Append(now.Minute.ToString("D2"));
            }
            if (bSec)
            {
                if (bMin)
                {
                    sbTime.Append(sColon);
                }
                else if (bHour)
                {
                    sbTime.Append(sColon);
                    sbTime.Append(sColon);
                }
                sbTime.Append(now.Second.ToString("D2"));
            }
        }
    }
}
