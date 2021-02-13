using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;

namespace NistTime
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DateTime: " + GetNistTime().ToString());

            DateTime dateTime = GetNistTime().ToUniversalTime();
            dateTime.AddMilliseconds(500);
            SYSTEMTIME st = new SYSTEMTIME();

            st.wYear = (short)dateTime.Year;
            st.wMonth = (short)dateTime.Month;
            st.wDay = (short)dateTime.Day;
            st.wHour = (short)dateTime.Hour;
            st.wMinute = (short)dateTime.Minute;
            st.wSecond = (short)dateTime.Second;
            SetSystemTime(ref st);
        }

        public static DateTime GetNistTime()
        {
            var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
            var response = myHttpWebRequest.GetResponse();
            string todaysDates = response.Headers["date"];
            return DateTime.ParseExact(todaysDates,
                                       "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                       CultureInfo.InvariantCulture.DateTimeFormat,
                                       DateTimeStyles.AssumeUniversal);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

}
