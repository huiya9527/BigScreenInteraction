using System;

namespace BigScreenInteraction
{
    public struct GRPoint
    {
        public static readonly GRPoint Empty;  //这个empty究竟是干嘛用的。。。。      
        public float _X, _Y;
        public long _T;

        public GRPoint(double x, double y) : this(x, y, 0)
        {
        }

        public GRPoint(double x, double y, long t)
        {
            _X = (float)x;
            _Y = (float)y;
            _T = t;
        }
        // copy constructor
        public GRPoint(GRPoint p)
        {
            _X = p._X;
            _Y = p._Y;
            _T = p._T;
        }

        public static bool operator ==(GRPoint p1, GRPoint p2)
        {
            return (p1._X == p2._X && p1._Y == p2._Y);
        }

        public static bool operator !=(GRPoint p1, GRPoint p2)
        {
            return (p1._X != p2._X || p1._Y != p2._Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is GRPoint)
            {
                GRPoint p = (GRPoint)obj;
                return (_X == p._X && _Y == p._Y);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

        public float Distance(GRPoint pt)
        {
            return (float)Math.Sqrt((_X - pt._X) * (_X - pt._X) + (_Y - pt._Y) * (_Y - pt._Y));
        }
    }

    public struct GRSize
    {
        public static readonly GRSize Empty;
        private float _Cx;
        private float _Cy;
        public float Width
        {
            get
            {
                return _Cx;
            }
            set
            {
                _Cx = value;
            }
        }

        public float Height
        {
            get
            {
                return _Cy;
            }
            set
            {
                _Cy = value;
            }
        }

        public GRSize(float cx, float cy)
        {
            _Cx = cx;
            _Cy = cy;
        }

        public GRSize(double cx, double cy)
        {
            _Cx = (float)cx;
            _Cy = (float)cy;
        }

        // copy constructor
        public GRSize(GRSize sz)
        {
            _Cx = sz.Width;
            _Cy = sz.Height;
        }

        //public static explicit operator SizeF(SizeR sz)
        //{
        //    return new SizeF((float)sz.Width, (float)sz.Height);
        //}

        public static bool operator ==(GRSize sz1, GRSize sz2)
        {
            return (sz1.Width == sz2.Width && sz1.Height == sz2.Height);
        }

        public static bool operator !=(GRSize sz1, GRSize sz2)
        {
            return (sz1.Width != sz2.Width || sz1.Height != sz2.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj is GRSize)
            {
                GRSize sz = (GRSize)obj;
                return (Width == sz.Width && Height == sz.Height);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ((GRSize)this).GetHashCode();
        }
    }

    public struct GRRect
    {
        private const int _Digits = 4;
        private float _x;
        private float _y;
        private float _width;
        private float _height;
        public static readonly GRRect Empty = new GRRect();

        public GRRect(float x, float y, float width, float height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public GRRect(double x, double y, double width, double height)
        {
            _x = (float)x;
            _y = (float)y;
            _width = (float)width;
            _height = (float)height;
        }

        // copy constructor
        public GRRect(GRRect r)
        {
            _x = r.X;
            _y = r.Y;
            _width = r.Width;
            _height = r.Height;
        }

        public float X
        {
            get
            {
                return (float)Math.Round((double)_x, _Digits);
            }
            set
            {
                _x = value;
            }
        }

        public float Y
        {
            get
            {
                return (float)Math.Round((double)_y, _Digits);
            }
            set
            {
                _y = value;
            }
        }

        public float Width
        {
            get
            {
                return (float)Math.Round(_width, _Digits);
            }
            set
            {
                _width = (float)value;
            }
        }

        public float Height
        {
            get
            {
                return (float)Math.Round(_height, _Digits);
            }
            set
            {
                _height = value;
            }
        }

        public GRPoint TopLeft
        {
            get
            {
                return new GRPoint(X, Y);
            }
        }

        public GRPoint BottomRight
        {
            get
            {
                return new GRPoint(X + Width, Y + Height);
            }
        }

        public GRPoint Center
        {
            get
            {
                return new GRPoint(X + Width / 2d, Y + Height / 2d);
            }
        }

        public float MaxSide
        {
            get
            {
                return Math.Max(_width, _height);
            }
        }

        public float MinSide
        {
            get
            {
                return Math.Min(_width, _height);
            }
        }

        public double Diagonal
        {
            get
            {
                return Utils.Distance(TopLeft, BottomRight);
            }
        }

        //public static explicit operator RectangleF(GRRect r)
        //{
        //    return new RectangleF((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        //}

        public override bool Equals(object obj)
        {
            if (obj is GRRect)
            {
                GRRect r = (GRRect)obj;
                return (X == r.X && Y == r.Y && Width == r.Width && Height == r.Height);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

    }
}
