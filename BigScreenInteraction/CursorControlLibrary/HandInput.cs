using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CursorControlLibrary
{
    [Flags]
    public enum MouseEventDataXButtons : uint
    {
        Nothing = 0x00000000,
        XBUTTON1 = 0x00000001,
        XBUTTON2 = 0x00000002
    }

    [Flags]
    public enum MOUSEEVENTF : uint
    {
        ABSOLUTE = 0x8000,
        HWHEEL = 0x01000,
        MOVE = 0x0001,
        MOVE_NOCOALESCE = 0x2000,
        LEFTDOWN = 0x0002,
        LEFTUP = 0x0004,
        RIGHTDOWN = 0x0008,
        RIGHTUP = 0x0010,
        MIDDLEDOWN = 0x0020,
        MIDDLEUP = 0x0040,
        VIRTUALDESK = 0x4000,
        WHEEL = 0x0800,
        XDOWN = 0x0080,
        XUP = 0x0100
    }

    public class HandInput
    {
        public int _isWhich; //0 没有， 1左手，2右手，3双手
        public MOUSEEVENTF _dwFlags;
        public int _PrimaryHand; //哪只手控制光标 0 没有， 1左手，2右手
        public float _dx;
        public float _dy;
        public float _dx1;
        public float _dy1;
        public float _Scale;   //
        public float _LPressExtent;
        public float _RPressExtent;
        public long _time;
        public bool isLeftGrip;
        public bool isRightGrip;
    }
}
