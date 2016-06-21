using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace BigScreenInteraction
{
    public delegate void GestureEventHandler(object obj, GestureEventArgs args);

    public class GestureEventArgs : EventArgs
    {

        private string _EventName;
        private long _Timestamp;

        public string EventName { get { return _EventName; } }
        public GestureEventArgs()
        {
            _EventName = "";
        }
        public GestureEventArgs(string eventId, long time)
        {
            _EventName = eventId;
            _Timestamp = time;
        }
    }

    public class GRConfiguration
    {
        protected readonly bool _Exclusive;

        // If the following is set to true, a successful recognition of the Process function will block
        // the callings to successive GRs (in the calling order).
        // On the other hand, GRs that are not exclusive they allow successive GRs to process data and
        // to send events as well.
        //exclusive指，如果为真，则一旦识别出一个手势，就不再继续识别可能的手势，如果为假，则继续识别，可发出多个手势事件
        public bool Exclusive { get { return _Exclusive; } }
        public GRConfiguration() : this(false) { }
        public GRConfiguration(bool exclusive)
        {
            _Exclusive = exclusive;
        }
    }


    public abstract class GestureRecognizer
    {
        public event Action<string> OnGestureDetected;
        public int _MinimalPeriodBetweenGestures { get; set; }
        DateTime _LastGestureDate = DateTime.Now;
        readonly int _TrajectorySize; // Number of recorded positions

        #region Private or internal members
        internal static readonly GRConfiguration DefaultConfiguration = new GRConfiguration();

        private GRConfiguration _Configuration;
        private int _PriorityNumber;
        private bool _Armed;
        private List<GestureEventHandler> _BufferedHandlers;
        private List<GestureEventArgs> _BufferedArgs;

        private int _debug_NProcessCalls = 0;

        // The recognition state, in base of which the GRs are coordinated by the group GR manager,
        // is composed by the following four parameters.
        /// <summary>
        /// Still attempting to recognize the gesture.
        /// </summary>
        private bool _Recognizing;
        /// <summary>
        /// Gesture successfully recognized.
        /// </summary>
        private bool _Successful;
        /// <summary>
        /// Confidence of successful recognition (0 = plain failure, 1 = plain success).
        /// </summary>
        private float _Confidence;
        /// <summary>
        /// Will process further incoming input.
        /// </summary>
        private bool _Processing;

        // These are public but intended to be internal
        public bool Recognizing { get { return _Recognizing; } }
        public bool Successful { get { return _Successful; } }
        public float Confidence { get { return _Confidence; } }
        public bool Processing { get { return _Processing; } }

        internal int PriorityNumber { get { return _PriorityNumber; } set { _PriorityNumber = value; } }
        internal bool Armed { get { return _Armed; } set { _Armed = value; } }
        #endregion

        #region Public members
        /// <summary>
        /// Object that will be passed as parameter to the constructor. It can be used to configure
        /// the GR's behaviour and/or to give it access to some resources.
        /// It should be set once in the constructor.
        /// </summary>
        public GRConfiguration Configuration { get { return _Configuration; } protected set { _Configuration = value; } }
        #endregion

        #region Point pram
        protected ArrayList _GRPoints;
        bool _isFirst = true;
        bool _isStar = false;
        long _StarTime = 0;
        long _Pretime = 0;
        long _CurTime = 0;

        int _EndCounter = 0;
        private const float TOUCH_REGION = 0.10F;
        private const float SPEED_MIN = 5F; //10个像素?
        private const int RECOG_MIN = 33;   //轨迹最少点
        private const int END_MIN = 17;   //轨迹最少点
        //private const float SPEED_MAX = 0.10F;

        int _isWho = 0;    //1-left,2-right
        HandState _Grip = HandState.Unknown;

        #endregion

        #region Test Draw
        protected ArrayList _DrawEllipse;
        public Canvas DisplayCanvas
        {
            get;
            set;
        }
        public Color DisplayColor
        {
            get;
            set;
        }

        #endregion

        #region Constructor
        public GestureRecognizer(GRConfiguration configuration, int TraSize = 20)
        {
            _Configuration = configuration;
            _Armed = false;
            _BufferedHandlers = new List<GestureEventHandler>();
            _BufferedArgs = new List<GestureEventArgs>();

            _Recognizing = true;
            _Successful = false;
            _Confidence = 1;
            _Processing = true;
            _TrajectorySize = TraSize;
            _MinimalPeriodBetweenGestures = 0;

            _GRPoints = new ArrayList(0x100);
            _DrawEllipse = new ArrayList(0x100);

            DisplayColor = Colors.Red;

        }
        #endregion

        public int TrajectorySize
        {
            get { return _TrajectorySize; }
        }

        public virtual void Add(SkeletonData skdata)
        {
            bool isRec = false;
            GRPoint ValidPoint = CheckPoint(skdata);
            if (ValidPoint != GRPoint.Empty) //如果为真，必然找到一只手，并且处于握紧状态？？？
            {
                _GRPoints.Add(ValidPoint);

                #region draw the gesture trail???
                if (DisplayCanvas != null)
                {
                    Ellipse DisplayEllipse = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        StrokeThickness = 2.0,
                        Stroke = new SolidColorBrush(DisplayColor),
                        StrokeLineJoin = PenLineJoin.Round
                    };
                    float x = (float)(ValidPoint._X * 0.15625);
                    float y = (float)(ValidPoint._Y * 0.20833);

                    DisplayCanvas.Children.Add(DisplayEllipse);//here add the ellipse
                    Canvas.SetLeft(DisplayEllipse, x - DisplayEllipse.Width / 2);
                    Canvas.SetTop(DisplayEllipse, y - DisplayEllipse.Height / 2);
                    _DrawEllipse.Add(DisplayEllipse);
                }
                #endregion

                //-------------------------------------------------------               
                _Pretime = _CurTime;
                _CurTime = skdata._Timestamp;
                //处理开始的驻留,速度过小则不认为会是手势开始
                if (!_isStar && _GRPoints.Count > 1)
                {
                    GRPoint pt0, pt1;
                    pt0 = (GRPoint)_GRPoints[0];
                    pt1 = (GRPoint)_GRPoints[_GRPoints.Count - 1];
                    float dis = pt0.Distance(pt1);
                    if (dis > SPEED_MIN)
                    {
                        _isStar = true;
                        _GRPoints.RemoveRange(0, _GRPoints.Count - 1);
                        _StarTime = _CurTime;
                    }
                }
                //判断是否结束
                int num = _GRPoints.Count;
                if (_isStar && num > RECOG_MIN)
                {
                    GRPoint pt1, pt0;
                    pt1 = (GRPoint)_GRPoints[num - 1];
                    pt0 = (GRPoint)_GRPoints[num - 2];
                    float dis = pt1.Distance(pt0);
                    if (dis < SPEED_MIN)
                    {
                        _EndCounter++;
                    }
                    else
                    {
                        _EndCounter = 0;
                    }

                    if (_EndCounter > END_MIN)
                    {
                        Console.Write("--------------------\n");
                        _EndCounter = 0;
                        isRec = true;
                    }
                }
            }
            else
            {
                int num = _GRPoints.Count;
                if (_isStar && num >= RECOG_MIN)
                {
                    isRec = true; //可以识别
                }
                else
                {
                    if (!_isFirst)
                        Reset();
                }
            }

            if (isRec)
            {
                GesturesRecognition();
                Reset();
            }
        }

        public virtual GRPoint CheckPoint(SkeletonData skdata)
        {

            //#region(判断手的活动范围)
            Vector3? headPos = skdata._JointPositions[(int)JointType.Head];
            Vector3? leftHandPos = skdata._JointPositions[(int)JointType.HandLeft];
            Vector3? rightHandPos = skdata._JointPositions[(int)JointType.HandRight];
            //problem
            Vector3? hipPos = skdata._JointPositions[(int)JointType.SpineMid];

            float left = -(leftHandPos.Value.Z - hipPos.Value.Z);
            float right = -(rightHandPos.Value.Z - hipPos.Value.Z);

            HandState Grip;
            //手的跟踪状态
            uint leftTrace = skdata._JointPositionTrackingState[(int)JointType.HandLeft];
            uint rightTrace = skdata._JointPositionTrackingState[(int)JointType.HandRight];

            //哪只手要做手势
            int isWho = 0;
            GRPoint Point;

            if (left > TOUCH_REGION && right <= TOUCH_REGION)// && leftTrace == 2)
            {
                isWho = 1;
                Grip = skdata._isGripLeft;
                Point = new GRPoint(leftHandPos.Value.X, leftHandPos.Value.Y, skdata._Timestamp);

            }
            else if (left <= TOUCH_REGION && right > TOUCH_REGION)// && rightTrace==2)
            {
                isWho = 2;
                Grip = skdata._isGripRight;
                Point = new GRPoint(rightHandPos.Value.X, rightHandPos.Value.Y);
            }
            else
            {
                _isWho = 0;
                return GRPoint.Empty;
            }
            //同一只手
            if (_isFirst)
            {
                if (Grip == HandState.Closed)
                {
                    _isWho = isWho;
                    _Grip = Grip;
                    _StarTime = skdata._Timestamp;
                    _isFirst = false;
                    return Point;
                }
            }
            else
            {
                if (_isWho != 0 && _isWho == isWho && Grip == HandState.Closed)
                {
                    _isWho = isWho;
                    _Grip = Grip;
                    return Point;
                }
                else
                {
                    _Grip = Grip;
                    _isWho = isWho;
                }
            }
            return GRPoint.Empty;
        }

        private void Reset()
        {
            _isFirst = true;
            _isStar = false;
            _StarTime = 0;
            _Pretime = 0;
            _CurTime = 0;
            _EndCounter = 0;
            _GRPoints.RemoveRange(0, _GRPoints.Count);
            if (DisplayCanvas != null)
            {
                DisplayCanvas.Children.Clear();
                _DrawEllipse.RemoveRange(0, _DrawEllipse.Count);
            }
        }

        protected void AppendEvent(GestureEventHandler ev, GestureEventArgs args)
        {

            // Console.Write(args.EventId+" ");
            if (ev != null)
            {
                ev(this, args);
            }
        }

        protected abstract void GesturesRecognition();

        protected void RaiseGestureDetected(string gesture)
        {
            // Too close?
            if (DateTime.Now.Subtract(_LastGestureDate).TotalMilliseconds > _MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                    OnGestureDetected(gesture);
                _LastGestureDate = DateTime.Now;
            }
            //Entries.Clear();
        }
    }


}
