
using System;
using System.Collections;

namespace BigScreenInteraction
{
    public class Unistroke : IComparable
    {
        public string Name;
        public ArrayList RawPoints; // raw points (for drawing) -- read in from XML
        public ArrayList Points;    // pre-processed points (for matching) -- created when loaded

        public Unistroke()
        {
            this.Name = String.Empty;
            this.RawPoints = null;
            this.Points = null;
        }

        /// <summary>
        /// Constructor of a unistroke gesture. A unistroke is comprised of a set of points drawn
        /// out over time in a sequence.
        /// </summary>
        /// <param name="name">The name of the unistroke gesture.</param>
        /// <param name="points">The array of points supplied for this unistroke.</param>
        public Unistroke(string name, ArrayList points)
        {
            this.Name = name;
            this.RawPoints = new ArrayList(points); // copy (saved for drawing)

            this.Points = Utils.Resample(points, DollarOneGRConfiguration.NumPoints);
            double radians = Utils.AngleInRadians(Utils.Centroid(this.Points), (GRPoint)this.Points[0], false);
            this.Points = Utils.RotateByRadians(this.Points, -radians);
            this.Points = Utils.ScaleTo(this.Points, DollarOneGRConfiguration.SquareSize);
            this.Points = Utils.TranslateCentroidTo(this.Points, DollarOneGRConfiguration.Origin);
        }

        /// <summary>
        /// 
        /// </summary>
        public long Duration
        {
            get
            {
                if (RawPoints.Count >= 2)
                {
                    GRPoint p0 = (GRPoint)RawPoints[0];
                    GRPoint pn = (GRPoint)RawPoints[RawPoints.Count - 1];
                    return pn._T - p0._T;
                }
                else
                {
                    return 0;
                }
            }
        }

        // sorts in descending order of Score
        public int CompareTo(object obj)
        {
            if (obj is Unistroke)
            {
                Unistroke g = (Unistroke)obj;
                return this.Name.CompareTo(g.Name);
            }
            else throw new ArgumentException("object is not a Gesture");
        }

        /// <summary>
        /// Pulls the gesture name from the file name, e.g., "circle03" from "C:\gestures\circles\circle03.xml".
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ParseName(string filename)
        {
            int start = filename.LastIndexOf('\\');
            int end = filename.LastIndexOf('.');
            return filename.Substring(start + 1, end - start - 1);
        }
    }

    public class Utils
    {
        #region Constants

        private static readonly Random _rand = new Random();

        #endregion

        #region Lengths and Rects

        public static GRRect FindBox(ArrayList points)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (GRPoint p in points)
            {
                if (p._X < minX)
                    minX = p._X;
                if (p._X > maxX)
                    maxX = p._X;

                if (p._Y < minY)
                    minY = p._Y;
                if (p._Y > maxY)
                    maxY = p._Y;
            }

            return new GRRect(minX, minY, maxX - minX, maxY - minY);
        }

        public static float Distance(GRPoint p1, GRPoint p2)
        {
            double dx = p2._X - p1._X;
            double dy = p2._Y - p1._Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // compute the centroid of the points given
        public static GRPoint Centroid(ArrayList points)
        {
            double xsum = 0.0;
            double ysum = 0.0;

            foreach (GRPoint p in points)
            {
                xsum += p._X;
                ysum += p._Y;
            }
            return new GRPoint(xsum / points.Count, ysum / points.Count);
        }

        public static double PathLength(ArrayList points)
        {
            double length = 0;
            for (int i = 1; i < points.Count; i++)
            {
                length += Distance((GRPoint)points[i - 1], (GRPoint)points[i]);
            }
            return length;
        }

        #endregion

        #region Angles and Rotations

        // determines the angle, in degrees, between two points. the angle is defined 
        // by the circle centered on the start point with a radius to the end point, 
        // where 0 degrees is straight right from start (+x-axis) and 90 degrees is
        // straight down (+y-axis).
        public static double AngleInDegrees(GRPoint start, GRPoint end, bool positiveOnly)
        {
            double radians = AngleInRadians(start, end, positiveOnly);
            return Rad2Deg(radians);
        }

        // determines the angle, in radians, between two points. the angle is defined 
        // by the circle centered on the start point with a radius to the end point, 
        // where 0 radians is straight right from start (+x-axis) and PI/2 radians is
        // straight down (+y-axis).
        public static double AngleInRadians(GRPoint start, GRPoint end, bool positiveOnly)
        {
            double radians = 0.0;
            if (start._X != end._X)
            {
                radians = Math.Atan2(end._Y - start._Y, end._X - start._X);
            }
            else // pure vertical movement
            {
                if (end._Y < start._Y)
                    radians = -Math.PI / 2.0; // -90 degrees is straight up
                else if (end._Y > start._Y)
                    radians = Math.PI / 2.0; // 90 degrees is straight down
            }
            if (positiveOnly && radians < 0.0)
            {
                radians += Math.PI * 2.0;
            }
            return radians;
        }

        public static double Rad2Deg(double rad)
        {
            return (rad * 180d / Math.PI);
        }

        public static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180d);
        }

        // rotate the points by the given degrees about their centroid
        public static ArrayList RotateByDegrees(ArrayList points, double degrees)
        {
            double radians = Deg2Rad(degrees);
            return RotateByRadians(points, radians);
        }

        // rotate the points by the given radians about their centroid
        public static ArrayList RotateByRadians(ArrayList points, double radians)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRPoint c = Centroid(points);

            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double cx = c._X;
            double cy = c._Y;

            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];

                double dx = p._X - cx;
                double dy = p._Y - cy;

                GRPoint q = GRPoint.Empty;
                q._X = (float)(dx * cos - dy * sin + cx);
                q._Y = (float)(dx * sin + dy * cos + cy);

                newPoints.Add(q);
            }
            return newPoints;
        }

        // Rotate a point 'p' around a point 'c' by the given radians.
        // Rotation (around the origin) amounts to a 2x2 matrix of the form:
        //
        //		[ cos A		-sin A	] [ p.x ]
        //		[ sin A		cos A	] [ p.y ]
        //
        // Note that the C# Math coordinate system has +x-axis stright right and
        // +y-axis straight down. Rotation is clockwise such that from +x-axis to
        // +y-axis is +90 degrees, from +x-axis to -x-axis is +180 degrees, and 
        // from +x-axis to -y-axis is -90 degrees.
        public static GRPoint RotatePoint(GRPoint p, GRPoint c, double radians)
        {
            GRPoint q = GRPoint.Empty;
            q._X = (float)((p._X - c._X) * Math.Cos(radians) - (p._Y - c._Y) * Math.Sin(radians) + c._X);
            q._Y = (float)((p._X - c._X) * Math.Sin(radians) + (p._Y - c._Y) * Math.Cos(radians) + c._Y);
            return q;
        }

        #endregion

        #region Translations

        // translates the points so that the upper-left corner of their bounding box lies at 'toPt'
        public static ArrayList TranslateBBoxTo(ArrayList points, GRPoint toPt)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRRect r = Utils.FindBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X += (toPt._X - r.X);
                p._Y += (toPt._Y - r.Y);
                newPoints.Add(p);
            }
            return newPoints;
        }

        // translates the points so that their centroid lies at 'toPt'
        public static ArrayList TranslateCentroidTo(ArrayList points, GRPoint toPt)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRPoint centroid = Centroid(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X += (toPt._X - centroid._X);
                p._Y += (toPt._Y - centroid._Y);
                newPoints.Add(p);
            }
            return newPoints;
        }

        // translates the points by the given delta amounts
        public static ArrayList TranslateBy(ArrayList points, GRSize sz)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X += sz.Width;
                p._Y += sz.Height;
                newPoints.Add(p);
            }
            return newPoints;
        }

        #endregion

        #region Scaling

        /// <summary>
        /// Scales the bounding box defined by a set of points after first rotating that set
        /// such that the angle from centroid-to-first-point is zero degrees. After scaling,
        /// the points are rotated back by the same angle.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="sz"></param>
        /// <returns></returns>
        public static ArrayList ScaleOrientedTo(ArrayList points, GRSize sz)
        {
            double radians = AngleInRadians(Centroid(points), (GRPoint)points[0], false); // indicative angle
            ArrayList newpoints = RotateByRadians(points, -radians); // rotate so that centroid-to-point[0] is 0 deg.
            newpoints = ScaleTo(newpoints, sz);
            newpoints = RotateByRadians(newpoints, +radians); // restore orientation
            return newpoints;
        }

        // scales the points so that they form the size given. does not restore the 
        // origin of the box.
        public static ArrayList ScaleTo(ArrayList points, GRSize sz)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRRect r = FindBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                if (r.Width != 0.0)
                    p._X *= (sz.Width / r.Width);
                if (r.Height != 0.0)
                    p._Y *= (sz.Height / r.Height);
                newPoints.Add(p);
            }
            return newPoints;
        }

        // scales by the percentages contained in the 'sz' parameter. values of 1.0 would result in the
        // identity scale (that is, no change).
        public static ArrayList ScaleBy(ArrayList points, GRSize sz)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRRect r = FindBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X *= sz.Width;
                p._Y *= sz.Height;
                newPoints.Add(p);
            }
            return newPoints;
        }

        // scales the points so that the length of their longer side
        // matches the length of the longer side of the given box.
        // thus, both dimensions are warped proportionally, rather than
        // independently, like in the function ScaleTo.
        public static ArrayList ScaleToMax(ArrayList points, GRRect box)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRRect r = FindBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X *= (box.MaxSide / r.MaxSide);
                p._Y *= (box.MaxSide / r.MaxSide);
                newPoints.Add(p);
            }
            return newPoints;
        }

        // scales the points so that the length of their shorter side
        // matches the length of the shorter side of the given box.
        // thus, both dimensions are warped proportionally, rather than
        // independently, like in the function ScaleTo.
        public static ArrayList ScaleToMin(ArrayList points, GRRect box)
        {
            ArrayList newPoints = new ArrayList(points.Count);
            GRRect r = FindBox(points);
            for (int i = 0; i < points.Count; i++)
            {
                GRPoint p = (GRPoint)points[i];
                p._X *= (box.MinSide / r.MinSide);
                p._Y *= (box.MinSide / r.MinSide);
                newPoints.Add(p);
            }
            return newPoints;
        }

        #endregion

        #region Path Sampling and Distance

        public static ArrayList Resample(ArrayList points, int n)
        {
            double I = PathLength(points) / (n - 1); // interval length
            double D = 0.0;
            ArrayList srcPts = new ArrayList(points);
            ArrayList dstPts = new ArrayList(n);
            dstPts.Add(srcPts[0]);
            for (int i = 1; i < srcPts.Count; i++)
            {
                GRPoint pt1 = (GRPoint)srcPts[i - 1];
                GRPoint pt2 = (GRPoint)srcPts[i];

                double d = Distance(pt1, pt2);
                if ((D + d) >= I)
                {
                    double qx = pt1._X + ((I - D) / d) * (pt2._X - pt1._X);
                    double qy = pt1._Y + ((I - D) / d) * (pt2._Y - pt1._Y);
                    GRPoint q = new GRPoint(qx, qy);
                    dstPts.Add(q); // append new point 'q'
                    srcPts.Insert(i, q); // insert 'q' at position i in points s.t. 'q' will be the next i
                    D = 0.0;
                }
                else
                {
                    D += d;
                }
            }
            // somtimes we fall a rounding-error short of adding the last point, so add it if so
            if (dstPts.Count == n - 1)
            {
                dstPts.Add(srcPts[srcPts.Count - 1]);
            }

            return dstPts;
        }

        // computes the 'distance' between two point paths by summing their corresponding point distances.
        // assumes that each path has been resampled to the same number of points at the same distance apart.
        public static double PathDistance(ArrayList path1, ArrayList path2)
        {
            double distance = 0;
            for (int i = 0; i < path1.Count; i++)
            {
                distance += Distance((GRPoint)path1[i], (GRPoint)path2[i]);
            }
            return distance / path1.Count;
        }

        #endregion

        #region Random Numbers

        /// <summary>
        /// Gets a random number between low and high, inclusive.
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static int Random(int low, int high)
        {
            return _rand.Next(low, high + 1);
        }

        /// <summary>
        /// Gets multiple random numbers between low and high, inclusive. The
        /// numbers are guaranteed to be distinct.
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int[] Random(int low, int high, int num)
        {
            int[] array = new int[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = _rand.Next(low, high + 1);
                for (int j = 0; j < i; j++)
                {
                    if (array[i] == array[j])
                    {
                        i--; // redo i
                        break;
                    }
                }
            }
            return array;
        }

        #endregion
    }
}
