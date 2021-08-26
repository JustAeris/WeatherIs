using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace WeatherIs.Web
{
    public static class Utils
    {
        /// <summary>
        /// An extension method to determine if an IP address is internal, as specified in RFC1918
        /// </summary>
        /// <param name="toTest">The IP address that will be tested</param>
        /// <returns>Returns true if the IP is internal, false if it is external</returns>
        public static bool IsInternal(this IPAddress toTest)
        {
            if (IPAddress.IsLoopback(toTest)) return true;
            else if (toTest.ToString() == "::1") return false;

            byte[] bytes = toTest.GetAddressBytes();
            switch( bytes[ 0 ] )
            {
                case 10:
                    return true;
                case 172:
                    return bytes[ 1 ] < 32 && bytes[ 1 ] >= 16;
                case 192:
                    return bytes[ 1 ] == 168;
                default:
                    return false;
            }
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };

        public static bool TryParseCookie<T>(this IRequestCookieCollection cookies, string cookieName, out T result)
        {
            result = default;
            if (!cookies.TryGetValue(cookieName, out var json)) return false;
            if (json == null)
                return false;

            result = JsonConvert.DeserializeObject<T>(json);

            return result != null;
        }

        public abstract class Logging
        {
            public const string LogTemplate = "[{Timestamp:HH:mm:ss} | {Level:u3}] | {SourceContext}] {Message:lj} {Exception:j}{NewLine}";

            public sealed class LoggingTheme : ConsoleTheme
            {
                public override bool CanBuffer => false;

                protected override int ResetCharCount => 0;

                public override void Reset(TextWriter output)
                {
                    Console.ResetColor();
                }

                public override int Set(TextWriter output, ConsoleThemeStyle style)
                {
                    (ConsoleColor foreground, ConsoleColor background) = style switch
                    {
                        ConsoleThemeStyle.Scalar => (ConsoleColor.Green, ConsoleColor.Black),
                        ConsoleThemeStyle.Number => (ConsoleColor.DarkGreen, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelDebug => (ConsoleColor.DarkMagenta, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelError => (ConsoleColor.Red, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelFatal => (ConsoleColor.DarkRed, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelVerbose => (ConsoleColor.Magenta, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelWarning => (ConsoleColor.Yellow, ConsoleColor.Black),
                        ConsoleThemeStyle.SecondaryText => (ConsoleColor.DarkBlue, ConsoleColor.Black),
                        ConsoleThemeStyle.LevelInformation => (ConsoleColor.DarkCyan, ConsoleColor.Black),
                        _ => (ConsoleColor.White, ConsoleColor.Black)
                    };
                    Console.ForegroundColor = foreground;
                    Console.BackgroundColor = background;
                    return 0;
                }
            }
        }
    }
}
