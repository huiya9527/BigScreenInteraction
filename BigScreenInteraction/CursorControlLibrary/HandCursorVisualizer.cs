using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace CursorControlLibrary
{
    /// <summary>
    /// Simple WPF Adorner to display Kinect HandPointers
    /// </summary>
   
    public class HandCursorVisualizer : Canvas
    {
        bool test = false;
        [DllImport("user32.dll")]
        static extern int SetCursorPos(int X, int Y);

        public static readonly DependencyProperty CursorPressingColorProperty = DependencyProperty.Register(
                "CursorPressingColor", typeof(Color), typeof(HandCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 102, 48, 133)));

        public static readonly DependencyProperty CursorExtendedColor1Property = DependencyProperty.Register(
            "CursorExtendedColor1", typeof(Color), typeof(HandCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 1, 179, 255)));

        public static readonly DependencyProperty CursorExtendedColor2Property = DependencyProperty.Register(
            "CursorExtendedColor2", typeof(Color), typeof(HandCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 04, 229, 255)));

        public static readonly DependencyProperty CursorGrippedColor1Property = DependencyProperty.Register(
            "CursorGrippedColor1", typeof(Color), typeof(HandCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 1, 179, 255)));

        public static readonly DependencyProperty CursorGrippedColor2Property = DependencyProperty.Register(
            "CursorGrippedColor2", typeof(Color), typeof(HandCursorVisualizer), new PropertyMetadata(Color.FromArgb(255, 04, 229, 255)));

        private const double CursorBoundsMargin = 20.0;

        // check for design mode
        private static readonly bool IsInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        //private readonly KinectRegionBinder kinectRegionBinder;

        //private readonly Dictionary<HandPointer, HandCursor> pointerCursorMap;
        private HandCursor _LCursor = new HandCursor();
        private HandCursor _RCursor = new HandCursor();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static HandCursorVisualizer()
        {
            // Set default style key to be this control type
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HandCursorVisualizer), new FrameworkPropertyMetadata(typeof(HandCursorVisualizer)));

            // Set default style to have FlowDirection be LeftToRight
            var style = new Style(typeof(HandCursorVisualizer), null);
            style.Setters.Add(new Setter(FlowDirectionProperty, FlowDirection.LeftToRight));
            style.Seal();
            StyleProperty.OverrideMetadata(typeof(HandCursorVisualizer), new FrameworkPropertyMetadata(style));
        }

        public HandCursorVisualizer()
        {
            //this.kinectRegionBinder = new KinectRegionBinder(this);
            //this.kinectRegionBinder.OnKinectRegionChanged += this.OnKinectRegionChanged;

            // This makes the adorner ignore all mouse input
            this.IsHitTestVisible = false;

            _LCursor.SetBinding(HandCursor.CursorPressingColorProperty, new Binding("CursorPressingColor") { Source = this });
            _LCursor.SetBinding(HandCursor.CursorExtendedColor1Property, new Binding("CursorExtendedColor1") { Source = this });
            _LCursor.SetBinding(HandCursor.CursorExtendedColor2Property, new Binding("CursorExtendedColor2") { Source = this });
            _LCursor.SetBinding(HandCursor.CursorGrippedColor1Property, new Binding("CursorGrippedColor1") { Source = this });
            _LCursor.SetBinding(HandCursor.CursorGrippedColor2Property, new Binding("CursorGrippedColor2") { Source = this });

            _RCursor.SetBinding(HandCursor.CursorPressingColorProperty, new Binding("CursorPressingColor") { Source = this });
            _RCursor.SetBinding(HandCursor.CursorExtendedColor1Property, new Binding("CursorExtendedColor1") { Source = this });
            _RCursor.SetBinding(HandCursor.CursorExtendedColor2Property, new Binding("CursorExtendedColor2") { Source = this });
            _RCursor.SetBinding(HandCursor.CursorGrippedColor1Property, new Binding("CursorGrippedColor1") { Source = this });
            _RCursor.SetBinding(HandCursor.CursorGrippedColor2Property, new Binding("CursorGrippedColor2") { Source = this });

            this.Children.Add(_LCursor);
            this.Children.Add(_RCursor);

            //this.pointerCursorMap = new Dictionary<HandPointer, HandCursor>();
            
        }

        public Color CursorPressingColor
        {
            get
            {
                return (Color)this.GetValue(CursorPressingColorProperty);
            }

            set
            {
                this.SetValue(CursorPressingColorProperty, value);
            }
        }

        public Color CursorExtendedColor1
        {
            get
            {
                return (Color)this.GetValue(CursorExtendedColor1Property);
            }

            set
            {
                this.SetValue(CursorExtendedColor1Property, value);
            }
        }

        public Color CursorExtendedColor2
        {
            get
            {
                return (Color)this.GetValue(CursorExtendedColor2Property);
            }

            set
            {
                this.SetValue(CursorExtendedColor2Property, value);
            }
        }

        public Color CursorGrippedColor1
        {
            get
            {
                return (Color)this.GetValue(CursorGrippedColor1Property);
            }

            set
            {
                this.SetValue(CursorGrippedColor1Property, value);
            }
        }

        public Color CursorGrippedColor2
        {
            get
            {
                return (Color)this.GetValue(CursorGrippedColor2Property);
            }

            set
            {
                this.SetValue(CursorGrippedColor2Property, value);
            }
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        /// <summary>
        /// Update any cursors we are displaying
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="args">Event arguments</param>
        public void OnHandPointersUpdated(object sender, EventArgs args)
        {
    
        }

        public void UpdateHandCursor(HandInput handinput)
        {
            if (handinput == null)
                return;
            #region (Cursor in Canvas?)
            switch (handinput._isWhich)
            {
                case 0:
                    if (this.Children.Contains(_LCursor))
                    {
                        this.Children.Remove(_LCursor);
                    }
                    if (this.Children.Contains(_RCursor))
                    {
                        this.Children.Remove(_RCursor);
                    }
                    return;
                case 1:
                    if (!this.Children.Contains(_LCursor))
                    {
                        this.Children.Add(_LCursor);
                    }
                    if (this.Children.Contains(_RCursor))
                    {
                        this.Children.Remove(_RCursor);
                    }
                    LeftHand(handinput);
                    break;
                case 2:
                    if (this.Children.Contains(_LCursor))
                    {
                        this.Children.Remove(_LCursor);
                    }
                    if (!this.Children.Contains(_RCursor))
                    {
                        this.Children.Add(_RCursor);
                    }
                    RightHand(handinput);
                    break;
                case 3:
                    if (!this.Children.Contains(_LCursor))
                    {
                        this.Children.Add(_LCursor);
                    }
                    if (!this.Children.Contains(_RCursor))
                    {
                        this.Children.Add(_RCursor);
                    }
                    LeftHand(handinput);
                    RightHand(handinput);
                    break;
                default:
                    return;
            }  
            #endregion
        }

        private void LeftHand(HandInput handinput)
        {
            _LCursor.IsOpen = !handinput.isLeftGrip;
            _LCursor.IsHovering = true;
            _LCursor.IsPressed = false;
            _LCursor.PressExtent = handinput._LPressExtent;
            double LeftadjustedPressExtent = _LCursor.PressExtent;
            double LeftfinalRadius = HandCursor.ArtworkSize * (1.0 - (LeftadjustedPressExtent * ((HandCursor.MaximumCursorScale - HandCursor.MinimumCursorScale) / 2.0)));
            // Flip hand for Left           
            double LeftscaleX = -LeftfinalRadius / HandCursor.ArtworkSize;
            double LeftscaleY = LeftfinalRadius / HandCursor.ArtworkSize;

            var LefthandScale = new ScaleTransform(LeftscaleX, LeftscaleY);
            _LCursor.RenderTransform = LefthandScale;
            _LCursor.Opacity = 0.5;

            Canvas.SetLeft(_LCursor, handinput._dx);
            Canvas.SetTop(_LCursor, handinput._dy);

        }

        private void RightHand(HandInput handinput)
        {
            _RCursor.IsOpen = !handinput.isRightGrip;
            _RCursor.IsHovering = true;
            _RCursor.IsPressed = false;
            _RCursor.PressExtent = handinput._RPressExtent;
            _RCursor.Opacity = 0.5;

            Canvas.SetLeft(_RCursor, handinput._dx1);
            Canvas.SetTop(_RCursor, handinput._dy1);

        }

        private void TwoHand(HandInput handinput)
        {
            _LCursor.IsOpen = !handinput.isLeftGrip;
            _LCursor.IsHovering = true;
            _LCursor.IsPressed = false;
            _LCursor.PressExtent = handinput._LPressExtent;
            double LeftadjustedPressExtent = _LCursor.PressExtent;
            double LeftfinalRadius = HandCursor.ArtworkSize * (1.0 - (LeftadjustedPressExtent * ((HandCursor.MaximumCursorScale - HandCursor.MinimumCursorScale) / 2.0)));
            // Flip hand for Left           
            double LeftscaleX = -LeftfinalRadius / HandCursor.ArtworkSize;
            double LeftscaleY = LeftfinalRadius / HandCursor.ArtworkSize;

            var LefthandScale = new ScaleTransform(LeftscaleX, LeftscaleY);
            _LCursor.RenderTransform = LefthandScale;
            _LCursor.Opacity = 0.5;

            _RCursor.IsOpen = !handinput.isRightGrip;
            _RCursor.IsHovering = true;
            _RCursor.IsPressed = false;
            _RCursor.PressExtent = handinput._RPressExtent;
            _RCursor.Opacity = 0.5;
            
            Canvas.SetLeft(_LCursor, handinput._dx);
            Canvas.SetTop(_LCursor, handinput._dy);
            Canvas.SetLeft(_RCursor, handinput._dx1);
            Canvas.SetTop(_RCursor, handinput._dy1);
        }
    }
}
