using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;

namespace NistTime2
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var url = Configuration.GetValue<string>("Settings:URL");

            DateTime dtBeg = DateTime.Now;

            Console.WriteLine("NistTime2 running...");

            var tmp = (new GetTime()).GetNistTime(url);
            if (tmp != null)
            {
                Console.WriteLine("Current Date & Time: " + tmp.ToString());

                DateTime dtEnd = DateTime.Now;
                TimeSpan tm = dtEnd - dtBeg;

                DateTime dateTime = ((DateTime)tmp).ToUniversalTime();

                double correction = tm.TotalMilliseconds + 500;
                Console.WriteLine($"Add correction: {correction}");

                dateTime.AddMilliseconds(correction);
                SYSTEMTIME st = new SYSTEMTIME();

                st.wYear = (short)dateTime.Year;
                st.wMonth = (short)dateTime.Month;
                st.wDay = (short)dateTime.Day;
                st.wHour = (short)dateTime.Hour;
                st.wMinute = (short)dateTime.Minute;
                st.wSecond = (short)dateTime.Second;
                SetSystemTime(ref st);
            }
            else
            {
                Console.WriteLine("Current Date & Time did not got");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);
        
    }

    class GetTime 
    {
        public DateTime? GetNistTime(string url)
        {
            while (true)
            {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                var tmp = WebRequest.Create(url);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                HttpWebRequest? request = null;

                if (tmp != null)
                    request = (HttpWebRequest)tmp;

                if (request != null)
                {
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Timeout = Timeout.Infinite;
                    request.KeepAlive = true;
                    request.Method = WebRequestMethods.Http.Get;
                    request.ContentType = "application/text; charset=utf-8";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        string todaysDates = response.Headers["date"] ?? string.Empty;
                        if (todaysDates != string.Empty)
                        {
                            return DateTime.ParseExact(todaysDates,
                                                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                                        CultureInfo.InvariantCulture.DateTimeFormat,
                                                        DateTimeStyles.AssumeUniversal);
                        }
                    }
                }
                return null;
            }
        }
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