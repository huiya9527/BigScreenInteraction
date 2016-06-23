using System;
using System.Collections;

namespace Recognizer.Dollar
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

            this.Points = Utils.Resample(points, Recognizer.NumPoints);
            double radians = Utils.AngleInRadians(Utils.Centroid(this.Points), (PointR) this.Points[0], false);
            this.Points = Utils.RotateByRadians(this.Points, -radians);
            this.Points = Utils.ScaleTo(this.Points, Recognizer.SquareSize);
            this.Points = Utils.TranslateCentroidTo(this.Points, Recognizer.Origin);
		}

        /// <summary>
        /// 
        /// </summary>
        public int Duration
        {
            get
            {
                if (RawPoints.Count >= 2)
                {
                    PointR p0 = (PointR) RawPoints[0];
                    PointR pn = (PointR) RawPoints[RawPoints.Count - 1];
                    return pn.T - p0.T;
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
                Unistroke g = (Unistroke) obj;
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
}
