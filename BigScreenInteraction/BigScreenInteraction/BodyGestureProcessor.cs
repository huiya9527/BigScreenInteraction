
using Microsoft.Kinect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BigScreenInteraction
{
    //图像事件
    public class RefreshImageEventArgs : EventArgs
    {
        // TODO:        
        public ImageSource _Image;
        public RefreshImageEventArgs()
            : base() { }

        public RefreshImageEventArgs(ImageSource image)
        {
            _Image = image;
        }
    }
    public delegate void RefreshImageHandler(object sender, RefreshImageEventArgs obj);





    public class BodyGestureProcessor : IObserver
    {
        public event RefreshImageHandler RefreshImageEvent;
        private KinectDevice _KDevice = null;

        public Boolean TUIO_MOUSE = false;
        public Boolean mouseControl = true;

        #region 插值 interpolation
        private readonly ConcurrentQueue<SkeletonData> _SKinterpolationQueue;
        SkeletonData _PreSKData;
        private Thread _Consumer;
        #endregion

        #region 手势数据
        internal const long SKTRACE_PERIOD = 1200;   //记录轨迹最大200ms？
        private List<SkeletonData> _SKDataList;
        #endregion

        SKFilters _SKFilters;

        public DollarOneGR _KGr;
       // public Posture _Posture;

       

        public BodyGestureProcessor()
        {
            _SKFilters = new SKFilters();
            _SKDataList = new List<SkeletonData>();
            //////////////////////////////////////////////////////tuio

            _KGr = new DollarOneGR();
            //_Posture = new Posture();
            //_KGr = Canvas;

            #region 插值
            _SKinterpolationQueue = new ConcurrentQueue<SkeletonData>();
            _Consumer = new Thread(Consumer) { IsBackground = true };
            _Consumer.Start();
            #endregion
        }

        public void Update(Body body)
        {

            HandState HandGrip;

            Skeleton ClosestSkeleton;
            SkeletonData skdata;

            if (true)
            {
                //ClosestSkeleton = _KDevice.GetClosestSkeleton();
                //HandGrip = _KDevice.GetHandGripState();


                if (ClosestSkeleton != null && HandGrip != null)
                {
                    if (ClosestSkeleton.TrackingState != SkeletonTrackingState.Tracked)
                        return;

                    #region //将当前选定用户的数据放置到_SKData中
                    skdata = new SkeletonData();
                    skdata._TrackingId = ClosestSkeleton.TrackingId;
                    skdata._Position.X = ClosestSkeleton.Position.X;
                    skdata._Position.Y = ClosestSkeleton.Position.Y;
                    skdata._Position.Z = ClosestSkeleton.Position.Z;

                    for (int i = 0; i < ClosestSkeleton.Joints.Count; i++)
                    {
                        //变到全屏
                        Joint joint = ClosestSkeleton.Joints[(JointType)i].ScaleTo((int)(System.Windows.SystemParameters.PrimaryScreenWidth), (int)(System.Windows.SystemParameters.PrimaryScreenHeight), .35f, 0.35f);
                        skdata._JointPositions[i].X = joint.Position.X;
                        skdata._JointPositions[i].Y = joint.Position.Y;
                        skdata._JointPositions[i].Z = joint.Position.Z;
                        skdata._JointPositionTrackingState[i] = (uint)joint.TrackingState;
                    }


                    skdata._QualityFlags = (uint)ClosestSkeleton.ClippedEdges;
                    skdata._Timestamp = BodyFrame.;
                    #endregion
                    if (skdata._TrackingId == HandGrip._SkeletonTrackingId)
                    {
                        skdata._isGripLeft = HandGrip._isGripLeft;
                        skdata._isGripRight = HandGrip._isGripRight;
                    }
                    int count = _SKDataList.Count;
                    if (count == 0)
                    {
                        _SKDataList.Add(skdata);
                    }
                    else
                    {
                        SkeletonData skdata_first = _SKDataList[0];
                        SkeletonData skdata0;
                        if (skdata_first._TrackingId != skdata._TrackingId)
                        {
                            _SKDataList.Clear();
                            _SKDataList.Add(skdata);
                        }
                        else
                        {
                            _SKDataList.Add(skdata);
                            int expire = -1;
                            for (int i = 0; i < _SKDataList.Count; i++)
                            {
                                skdata0 = _SKDataList[i];
                                long Period = skdata._Timestamp - skdata0._Timestamp;
                                if (Period > SKTRACE_PERIOD)
                                {
                                    expire = i;
                                }
                                else
                                    break;
                                // if(i>0)
                            }
                            if (expire > 0)//移出过期的轨迹
                            {
                                _SKDataList.RemoveRange(0, expire + 1);
                            }
                        }
                    }

                    long l = skdata._Timestamp;

                    #region 插值
                    new Task(() =>
                    {
                        SkeletonData interpolation;
                        if (_SKinterpolationQueue.Count == 0)
                        {
                            _SKinterpolationQueue.Enqueue(skdata);
                            _PreSKData = skdata;
                        }
                        else
                        {
                            interpolation = skdata.Interpolation(_PreSKData);
                            _SKinterpolationQueue.Enqueue(interpolation);
                            _SKinterpolationQueue.Enqueue(skdata);
                            _PreSKData = skdata;
                        }
                    }).Start();

                    #endregion
                   
                }
            }
           
        }
        public void Update()
        {
            return;
        }

        void Consumer()
        {
            while (true)
            {
                SkeletonData skdata;
                if (_SKinterpolationQueue.TryDequeue(out skdata))
                {
                    _Frame._Parent.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // key code!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        if (_Frame._GestrureDisplayGrid.Visibility == System.Windows.Visibility.Collapsed) mouseControl = true;
                        else { mouseControl = false; }
                                                
                        _KGr.Add(skdata);//gesture
                        
                        _Posture.PostureRecognizer(skdata);//posture

                    }));
                }
                Thread.Sleep(10);
            }
        }
    }
}
