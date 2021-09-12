using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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

        public static bool TryParseCache<T>(this IMemoryCache cache, string cacheKey, out T result)
        {
            result = default;
            if (!cache.TryGetValue(cacheKey, out result)) return false;

            return result != null;
        }

        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
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

        public class Knn
        {
            public class PointF
            {
                public double X { get; set; }
                public double Y { get; set; }
                public PointF(double x,double y)
                {
                    X = x;
                    Y = y;
                }

            }

            public class KDNode
            {
                public KDNode Left { get; set; }
                public KDNode Right { get; set; }
                public KDNode Parent { get; set; }
                public PointF Value { get; set; }
            }

            public class KDTree
            {
                public KDNode Root { get; set; } = null;
                public int K { get; set; }
                public KDTree(int k=2)
                {
                    K = k;
                }
                public double Distance(PointF a,PointF b)
                {
                    double dx = a.X - b.X;
                    double dy = a.Y - b.Y;

                    return Math.Sqrt(dx * dx + dy * dy);

                }

                public void BuildKDTree(List<PointF> list)
                {
                    Root=CreateTree(Root,list);
                }

                private KDNode CreateTree(KDNode parent,List<PointF> list,int depth=0)
                {
                    if (list.Count == 0)
                        return null;

                    int axis = depth % K;
                    var sorted = list.OrderBy(i => axis == 0 ? i.X : i.Y).ToList();
                    int mid =sorted.Count/2;
                    var lower=SplitList(sorted, 0, mid - 1);
                    var upper = SplitList(sorted, mid + 1,list.Count-1);


                    KDNode node = new KDNode();
                    node.Value = list[mid];
                    node.Parent = parent;
                    node.Left = CreateTree(node, lower, depth+1);
                    node.Right = CreateTree(node, upper, depth + 1);

                    return node;
                }

                public PointF NearestPoint(PointF point)
                {
                    return GetNearestPoint(point,Root);
                }

                private PointF GetNearestPoint(PointF point, KDNode parent, int depth = 0)
                {
                    if (parent == null)
                        return null;
                    int axis = depth % K;

                    KDNode nextBranch, oppositeBranch;

                    if (axis == 0)
                    {
                        if (point.X < parent.Value.X)
                        {
                            nextBranch = parent.Left;
                            oppositeBranch = parent.Right;
                        }
                        else
                        {
                            nextBranch = parent.Right;
                            oppositeBranch = parent.Left;
                        }
                    }
                    else
                    {
                        if (point.Y < parent.Value.Y)
                        {
                            nextBranch = parent.Left;
                            oppositeBranch = parent.Right;
                        }
                        else
                        {
                            nextBranch = parent.Right;
                            oppositeBranch = parent.Left;
                        }
                    }

                    PointF br=GetNearestPoint(point, nextBranch, depth + 1);
                    PointF best = CloserDistance(point, br, parent.Value);

                    var distancePlane=0.0;
                    if(axis==0)
                        distancePlane=Math.Abs(point.X - parent.Value.X);
                    else
                        distancePlane = Math.Abs(point.Y - parent.Value.Y);

                    if(Distance(point,best)>distancePlane)
                    {
                        PointF opposite = GetNearestPoint(point, oppositeBranch, depth + 1);
                        best = CloserDistance(point, opposite, best);
                    }

                    return best;
                }

                PointF CloserDistance(PointF pivot,PointF p1,PointF p2)
                {
                    if (p1 == null)
                        return p2;
                    if (p2 == null)
                        return p1;


                    if (Distance(pivot , p2)<Distance(pivot,p1))
                        return p2;
                    else
                        return p1;
                }

                private static List<PointF> SplitList(List<PointF> list,int start,int end)
                {
                    List<PointF> points = new List<PointF>();
                    for (int i = start; i <= end; i++)
                    {
                        points.Add(list[i]);
                    }
                    return points;
                }
            }
        }
    }
}
