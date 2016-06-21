using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;

namespace BigScreenInteraction
{
    public class DollarOneEventArgs : GestureEventArgs
    {

        private double m_x, m_y;

        public double X { get { return m_x; } }
        public double Y { get { return m_y; } }


        public DollarOneEventArgs()
            : base() { }

        public DollarOneEventArgs(string name, double x, double y, long time)
            : base(name, time)
        {
            m_x = x;
            m_y = y;
        }
    }
    public delegate void DollarOneEventHandler(object obj, DollarOneEventArgs args);

    public class DollarOneGRConfiguration : GRConfiguration
    {
        public const int NumPoints = 64;
        public const double DX = 250.0;
        public static readonly GRSize SquareSize = new GRSize(DX, DX);
        public static readonly float Diagonal = (float)Math.Sqrt(DX * DX + DX * DX);
        public static readonly float HalfDiagonal = (float)(0.5 * Diagonal);
        public static readonly GRPoint Origin = new GRPoint(0, 0);

        public DollarOneGRConfiguration()
            : base(false) { }
    }

    public class DollarOneGR : GestureRecognizer
    {
        #region copy cof
        public const int _NumPoints = 64;
        public const double _DX = 250.0;
        public static readonly GRSize _SquareSize = new GRSize(_DX, _DX);
        public static readonly float _Diagonal = (float)Math.Sqrt(_DX * _DX + _DX * _DX);
        public static readonly float _HalfDiagonal = (float)(0.5 * _Diagonal);
        public static readonly GRPoint _Origin = new GRPoint(0, 0);

        #endregion

        #region Private or internal members
        private static readonly float Phi = (float)(0.5 * (-1.0 + Math.Sqrt(5.0))); // Golden Ratio
        private const int NumRandomTests = 100;
        private Hashtable _GesturesLib = new Hashtable(256);

        private string[] _GRLibPath;    //手势库路径

        #endregion

        #region Public members
        public event DollarOneEventHandler TRIANGLEEventHandler;
        public event DollarOneEventHandler XEventHandler;
        public event DollarOneEventHandler RECTANGLEEventHandler;
        public event DollarOneEventHandler CIRCLEEventHandler;
        public event DollarOneEventHandler CHECKEventHandler;
        public event DollarOneEventHandler CARETEventHandler;
        public event DollarOneEventHandler QUESTION_MARKEventHandler;
        public event DollarOneEventHandler ARROWEventHandler;
        public event DollarOneEventHandler LEFT_SQ_BRACKETEventHandler;
        public event DollarOneEventHandler RIGHT_SQ_BRACKETEventHandler;
        public event DollarOneEventHandler VEventHandler;
        public event DollarOneEventHandler DELETEEventHandler;
        public event DollarOneEventHandler PIGTAILEventHandler;
        #endregion

        #region Constructor
        public DollarOneGR()
            : base(null)
        {
            Configuration = new DollarOneGRConfiguration();
            try
            {
                this._GRLibPath = Directory.GetFiles(@"gesturemap\", "*.xml");
                this.loadAllGestures(this._GRLibPath);
            }
            catch (IOException exception)
            {
                Console.WriteLine("Gesture directory not found!: " + exception);
            }
        }

        public DollarOneGR(GRConfiguration configuration)
            : base(configuration)
        {
            if (!(configuration is DollarOneGRConfiguration))
                Configuration = new DollarOneGRConfiguration();

            try
            {
                this._GRLibPath = Directory.GetFiles(@"gesturemap\", "*.xml");
                this.loadAllGestures(this._GRLibPath);
            }
            catch (IOException exception)
            {
                Console.WriteLine("Gesture directory not found!: " + exception);
            }
        }

        #endregion

        #region Private Load GR Lib
        private void loadAllGestures(string[] gesturesToLoad)
        {
            foreach (string str in gesturesToLoad)
            {
                if (!this.LoadGesture(str))
                {
                    Console.WriteLine("Error loading gesture: " + str);
                }
            }
        }

        public bool LoadGesture(string filename)
        {
            bool success = true;
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();

                Unistroke p = ReadGesture(reader);

                // remove any with the same name and add the prototype gesture
                if (_GesturesLib.ContainsKey(p.Name))
                    _GesturesLib.Remove(p.Name);
                _GesturesLib.Add(p.Name, p);
            }
            catch (XmlException xex)
            {
                Console.Write(xex.Message);
                success = false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                success = false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return success;
        }

        private Unistroke ReadGesture(XmlTextReader reader)
        {
            Debug.Assert(reader.LocalName == "Gesture");
            string name = reader.GetAttribute("Name");
           

            ArrayList points = new ArrayList(XmlConvert.ToInt32(reader.GetAttribute("NumPts")));

            reader.Read(); // advance to the first Point
            Debug.Assert(reader.LocalName == "Point");

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                GRPoint p = GRPoint.Empty;
                p._X = (float)XmlConvert.ToDouble(reader.GetAttribute("X"));
                p._Y = (float)XmlConvert.ToDouble(reader.GetAttribute("Y"));
                p._T = XmlConvert.ToInt32(reader.GetAttribute("T"));
                points.Add(p);
                reader.ReadStartElement("Point");
            }
            return new Unistroke(name, points);
        }
        #endregion

        #region GesturesRecognition

        protected override void GesturesRecognition()
        {
            NBestList list;
            double GestPosX = 0, GestPosY = 0;
            long time = 0;


            if (NumGestures > 0)
            {
                list = Recognize(_GRPoints);
                if (Math.Round(list.Score, 2) >= 0.8)
                {
                    //this._firstResult = list;
                   // Console.WriteLine("-----------{0} : {1} \n", list.Name, _GRPoints.Count);
                    
                    GestPosX = ((GRPoint)_GRPoints[0])._X;
                    GestPosY = ((GRPoint)_GRPoints[0])._Y;
                    time = ((GRPoint)_GRPoints[0])._T;                    

                    switch (list.Name)
                    {

                        case "CARET":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("CARET", GestPosX, GestPosY, time));
                            break;

                        case "CHECK":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("CHECK", GestPosX, GestPosY, time));
                            break;

                        case "CIRCLE":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("CIRCLE", GestPosX, GestPosY, time));
                            break;

                        case "DELETE":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("DELETE", GestPosX, GestPosY, time));
                            break;

                        case "LIGHTNING":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("LIGHTNING", GestPosX, GestPosY, time));
                            break;

                        case "PIGTAIL":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("PIGTAIL", GestPosX, GestPosY, time));
                            break;

                        case "QUESTION_MARK":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("QUESTION_MARK", GestPosX, GestPosY, time));
                            break;

                        case "RECTANGLE":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("RECTANGLE", GestPosX, GestPosY, time));
                            break;

                        case "TRIANGLE":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("TRIANGLE", GestPosX, GestPosY, time));
                            break;

                        case "X":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("X", GestPosX, GestPosY, time));
                            break;

                        //////not used////////////////////////////////////////////////
                        case "LEFT_SQ_BRACKET":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("LEFT_SQ_BRACKET", GestPosX, GestPosY, time));
                            break;
                        case "RIGHT_SQ_BRACKET":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("RIGHT_SQ_BRACKET", GestPosX, GestPosY, time));
                            break;
                        case "V":
                            AppendEvent(CHECKEventHandler, new DollarOneEventArgs("V", GestPosX, GestPosY, time));
                            break;
                      
                        default:
                            break;
                    }
                    
                }
            }
        }

        public NBestList Recognize(ArrayList points) // candidate points
        {
            points = Utils.Resample(points, _NumPoints);
            double radians = Utils.AngleInRadians(Utils.Centroid(points), (GRPoint)points[0], false);
            points = Utils.RotateByRadians(points, -radians);
            points = Utils.ScaleTo(points, _SquareSize);
            points = Utils.TranslateCentroidTo(points, _Origin);

            NBestList nbest = new NBestList();
            foreach (Unistroke u in _GesturesLib.Values)
            {
                double[] best = GoldenSectionSearch(
                    points,                 // to rotate
                    u.Points,               // to match
                    Utils.Deg2Rad(-45.0),   // lbound
                    Utils.Deg2Rad(+45.0),   // ubound
                    Utils.Deg2Rad(2.0)      // threshold
                );

                double score = 1.0 - best[0] / _HalfDiagonal;
                nbest.AddResult(u.Name, score, best[0], best[1]); // name, score, distance, angle
            }
            nbest.SortDescending(); // sort so that nbest[0] is best result
            return nbest;
        }

        // From http://www.math.uic.edu/~jan/mcs471/Lec9/gss.pdf
        private double[] GoldenSectionSearch(ArrayList pts1, ArrayList pts2, double a, double b, double threshold)
        {
            double x1 = Phi * a + (1 - Phi) * b;
            ArrayList newPoints = Utils.RotateByRadians(pts1, x1);
            double fx1 = Utils.PathDistance(newPoints, pts2);

            double x2 = (1 - Phi) * a + Phi * b;
            newPoints = Utils.RotateByRadians(pts1, x2);
            double fx2 = Utils.PathDistance(newPoints, pts2);

            double i = 2.0; // calls
            while (Math.Abs(b - a) > threshold)
            {
                if (fx1 < fx2)
                {
                    b = x2;
                    x2 = x1;
                    fx2 = fx1;
                    x1 = Phi * a + (1 - Phi) * b;
                    newPoints = Utils.RotateByRadians(pts1, x1);
                    fx1 = Utils.PathDistance(newPoints, pts2);
                }
                else
                {
                    a = x1;
                    x1 = x2;
                    fx1 = fx2;
                    x2 = (1 - Phi) * a + Phi * b;
                    newPoints = Utils.RotateByRadians(pts1, x2);
                    fx2 = Utils.PathDistance(newPoints, pts2);
                }
                i++;
            }
            return new double[3] { Math.Min(fx1, fx2), Utils.Rad2Deg((b + a) / 2.0), i }; // distance, angle, calls to pathdist
        }

        // continues to rotate 'pts1' by 'step' degrees as long as points become ever-closer 
        // in path-distance to pts2. the initial distance is given by D. the best distance
        // is returned in array[0], while the angle at which it was achieved is in array[1].
        // array[3] contains the number of calls to PathDistance.
        private double[] HillClimbSearch(ArrayList pts1, ArrayList pts2, double D, double step)
        {
            double i = 0.0;
            double theta = 0.0;
            double d = D;
            do
            {
                D = d; // the last angle tried was better still
                theta += step;
                ArrayList newPoints = Utils.RotateByDegrees(pts1, theta);
                d = Utils.PathDistance(newPoints, pts2);
                i++;
            }
            while (d <= D);
            return new double[3] { D, theta - step, i }; // distance, angle, calls to pathdist
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private double[] FullSearch(ArrayList pts1, ArrayList pts2, StreamWriter writer)
        {
            double bestA = 0d;
            double bestD = Utils.PathDistance(pts1, pts2);

            for (int i = -180; i <= +180; i++)
            {
                ArrayList newPoints = Utils.RotateByDegrees(pts1, i);
                double d = Utils.PathDistance(newPoints, pts2);
                if (writer != null)
                {
                    writer.WriteLine("{0}\t{1:F3}", i, Math.Round(d, 3));
                }
                if (d < bestD)
                {
                    bestD = d;
                    bestA = i;
                }
            }
            writer.WriteLine("\nFull Search (360 rotations)\n{0:F2}{1}\t{2:F3} px", Math.Round(bestA, 2), (char)176, Math.Round(bestD, 3)); // calls, angle, distance
            return new double[3] { bestD, bestA, 360.0 }; // distance, angle, calls to pathdist
        }

        #endregion

        #region Public Fun

        public int NumGestures
        {
            get
            {
                return _GesturesLib.Count;
            }
        }

        public ArrayList Gestures
        {
            get
            {
                ArrayList list = new ArrayList(_GesturesLib.Values);
                list.Sort();
                return list;
            }
        }

        public void ClearGestures()
        {
            _GesturesLib.Clear();
        }

        #endregion

        protected void AppendEvent(DollarOneEventHandler ev, DollarOneEventArgs args)
        {

            // Console.Write(args.EventId+" ");
            if (ev != null)
            {
                ev(this, args);
            }
        }
    }
}