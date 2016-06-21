using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace BigScreenInteraction
{
    public class SkeletonData
    {
        #region Body Skeleton Data
        public int _TrackingId;
        public Vector3 _Position;
        public Vector3[] _JointPositions;//= new Vector3[20];
        public uint[] _JointPositionTrackingState;// = new uint[20];
        public uint _QualityFlags;      //ClippedEdges        
        public long _Timestamp { get; set; }

        public HandState _isGripLeft = HandState.Unknown;
        public HandState _isGripRight = HandState.Unknown;
        //public 
        //public uint EnrollmentIndex;
        //public uint UserIndex;
        #endregion       

        public SkeletonData()
        {
            _JointPositions = new Vector3[20];
            _JointPositionTrackingState = new uint[20];
        }

        //这个函数目测是用来插值的，先不管它！
        public SkeletonData Interpolation(SkeletonData preSkdata)
        {
            SkeletonData Inter = new SkeletonData();
            Inter._TrackingId = this._TrackingId;
            Inter._Position = (this._Position + preSkdata._Position) / 2;
            Inter._QualityFlags = preSkdata._QualityFlags;
            if (preSkdata._isGripLeft != HandState.Closed)
            {
                Inter._isGripLeft = preSkdata._isGripLeft;
            }
            else
            {
                Inter._isGripLeft = this._isGripLeft;
            }

            if (preSkdata._isGripRight != HandState.Open)
            {
                Inter._isGripRight = preSkdata._isGripRight;
            }
            else
            {
                Inter._isGripRight = this._isGripRight;
            }

            Inter._Timestamp = (preSkdata._Timestamp + this._Timestamp) / 2;

            for (int i = 0; i < 20; i++)
            {
                Inter._JointPositions[i] = (preSkdata._JointPositions[i] + this._JointPositions[i]) / 2;
                Inter._JointPositionTrackingState[i] = preSkdata._JointPositionTrackingState[i];
            }
            return Inter;
        }
    }



    public class SKFilters
    {
        #region(Filters)
        // Filter parameters
        public float _TrendSmoothingFactor { get; set; }
        public float _JitterRadius { get; set; }
        public float _DataSmoothingFactor { get; set; }
        public float _PredictionFactor { get; set; }
        public float _GlobalSmooth { get; set; }

        Vector3[] _FilteredJointPosition;
        Vector3[] _Trend;
        Vector3[] _BasePosition;
        int _FrameCount;

        #endregion

        public SKFilters()
        {
            _TrendSmoothingFactor = 0.25f;
            _JitterRadius = 0.05f;
            _DataSmoothingFactor = 0.5f;
            _PredictionFactor = 0.5f;
            _GlobalSmooth = 0.9f;

            _FrameCount = 0;
            _FilteredJointPosition = new Vector3[20];
            _Trend = new Vector3[20];
            _BasePosition = new Vector3[20];
        }

        public Vector3 FilterJointPosition(SkeletonData skdeta, int JointType)
        {
            Vector3 filteredJointPosition;
            Vector3 differenceVector;
            Vector3 currentTrend;
            float distance;

            Vector3 baseJointPosition = skdeta._JointPositions[JointType];
            Vector3 prevFilteredJointPosition = _FilteredJointPosition[JointType];
            Vector3 previousTrend = _Trend[JointType];
            Vector3 previousBaseJointPosition = _BasePosition[JointType];

            // Checking frames count
            switch (_FrameCount)
            {
                case 0:
                    filteredJointPosition = baseJointPosition;
                    currentTrend = Vector3.Zero;
                    break;
                case 1:
                    filteredJointPosition = (baseJointPosition + previousBaseJointPosition) * 0.5f;
                    differenceVector = filteredJointPosition - prevFilteredJointPosition;
                    currentTrend = differenceVector * _TrendSmoothingFactor + previousTrend * (1.0f - _TrendSmoothingFactor);
                    break;
                default:
                    // Jitter filter
                    differenceVector = baseJointPosition - prevFilteredJointPosition;
                    distance = Math.Abs(differenceVector.Length);

                    if (distance <= _JitterRadius)
                    {
                        filteredJointPosition = baseJointPosition * (distance / _JitterRadius) + prevFilteredJointPosition * (1.0f - (distance / _JitterRadius));
                    }
                    else
                    {
                        filteredJointPosition = baseJointPosition;
                    }

                    // Double exponential smoothing filter
                    filteredJointPosition = filteredJointPosition * (1.0f - _DataSmoothingFactor) + (prevFilteredJointPosition + previousTrend) * _DataSmoothingFactor;

                    differenceVector = filteredJointPosition - prevFilteredJointPosition;
                    currentTrend = differenceVector * _TrendSmoothingFactor + previousTrend * (1.0f - _TrendSmoothingFactor);
                    break;
            }

            // Compute potential new position
            Vector3 potentialNewPosition = filteredJointPosition + currentTrend * _PredictionFactor;

            // Cache current value
            _BasePosition[JointType] = baseJointPosition;
            _FilteredJointPosition[JointType] = filteredJointPosition;
            _Trend[JointType] = currentTrend;
            _FrameCount++;

            return potentialNewPosition;
        }
    }
}
