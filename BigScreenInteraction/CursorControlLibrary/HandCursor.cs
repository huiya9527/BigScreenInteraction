using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CursorControlLibrary
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CursorControlLibrary"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CursorControlLibrary;assembly=CursorControlLibrary"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    /// 
    /// <summary>
    /// Visualization for a Kinect cursor
    /// </summary>
    /// 

    internal class HandCursor : Control
    {
        // Rough bounding square around the open/closed hand models
        public const double ArtworkSize = 80.0;

        // Maximum Cursor Scale
        public const double MaximumCursorScale = 1.0;

        // Minimum Cursor Scale
        public const double MinimumCursorScale = 0.8;

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen",
            typeof(bool),
            typeof(HandCursor),
            new UIPropertyMetadata(true, (o, args) => ((HandCursor)o).EnsureVisualState()));

        public static readonly DependencyProperty IsHoveringProperty = DependencyProperty.Register(
            "IsHovering",
            typeof(bool),
            typeof(HandCursor),
            new UIPropertyMetadata(false, (o, args) => ((HandCursor)o).EnsureVisualState()));

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(HandCursor),
            new UIPropertyMetadata(false, (o, args) => ((HandCursor)o).OnIsPressedChanged()));

        public static readonly DependencyProperty PressExtentProperty = DependencyProperty.Register(
            "PressExtent",
            typeof(double),
            typeof(HandCursor),
            new UIPropertyMetadata(0.0, (o, args) => ((HandCursor)o).OnPressExtentChanged()));

        public static readonly DependencyProperty CursorPressingColorProperty = HandCursorVisualizer.CursorPressingColorProperty.AddOwner(typeof(HandCursor));

        public static readonly DependencyProperty CursorExtendedColor1Property = HandCursorVisualizer.CursorExtendedColor1Property.AddOwner(typeof(HandCursor));

        public static readonly DependencyProperty CursorExtendedColor2Property = HandCursorVisualizer.CursorExtendedColor2Property.AddOwner(typeof(HandCursor));

        public static readonly DependencyProperty CursorGrippedColor1Property = HandCursorVisualizer.CursorGrippedColor1Property.AddOwner(typeof(HandCursor));

        public static readonly DependencyProperty CursorGrippedColor2Property = HandCursorVisualizer.CursorGrippedColor2Property.AddOwner(typeof(HandCursor));


        private FrameworkElement pressStoryboardTarget;

        private Storyboard pressStoryboard;

        private string currentVisualState;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to OverrideMetadata in the static constructor")]
        static HandCursor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HandCursor), new FrameworkPropertyMetadata(typeof(HandCursor)));
        }

        public HandCursor()
        {
            this.Width = ArtworkSize;
            this.Height = ArtworkSize;

            this.Loaded += this.HandCursorLoaded;
        }

        public bool IsOpen
        {
            get
            {
                return (bool)this.GetValue(IsOpenProperty);
            }

            set
            {
                this.SetValue(IsOpenProperty, value);
            }
        }

        public bool IsHovering
        {
            get
            {
                return (bool)GetValue(IsHoveringProperty);
            }

            set
            {
                this.SetValue(IsHoveringProperty, value);
            }
        }

        public bool IsPressed
        {
            get
            {
                return (bool)GetValue(IsPressedProperty);
            }

            set
            {
                this.SetValue(IsPressedProperty, value);
            }
        }

        public double PressExtent
        {
            get
            {
                return (double)this.GetValue(PressExtentProperty);
            }

            set
            {
                this.SetValue(PressExtentProperty, value);
            }
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

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);

            if (this.pressStoryboard != null)
            {
                this.pressStoryboard.Stop(this.pressStoryboardTarget);
                this.pressStoryboard = null;
            }

            this.currentVisualState = null;
            this.OnIsPressedChanged();
            this.OnPressExtentChanged();
        }

        private void HandCursorLoaded(object sender, RoutedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(this) > 0)
            {
                this.pressStoryboardTarget = VisualTreeHelper.GetChild(this, 0) as Grid;
                if (this.pressStoryboardTarget != null)
                {
                    this.pressStoryboard = this.pressStoryboardTarget.TryFindResource("CursorPress") as Storyboard;
                }

                this.currentVisualState = null;
                this.OnIsPressedChanged();
                this.OnPressExtentChanged();
            }
        }

        private void OnIsPressedChanged()
        {
            if (this.pressStoryboard != null)
            {
                if (this.IsPressed)
                {
                    this.pressStoryboard.Remove();
                }
                else
                {
                    this.RestartPressStoryboard();
                }
            }

            this.EnsureVisualState();
        }

        private void EnsureVisualState()
        {
            if (!this.IsOpen)
            {
                this.GoToState("HandClosed");
            }
            else if (this.IsPressed)
            {
                this.GoToState("Pressed");
            }
            else if (this.IsHovering)
            {
                this.GoToState("Hover");
            }
            else
            {
                this.GoToState("Idle");
            }
        }

        private void RestartPressStoryboard()
        {
            if (this.pressStoryboard != null)
            {
                // Put the storyboard in a state where it can Seek
                // arbitrarily
                this.pressStoryboard.Begin(this.pressStoryboardTarget, true);
                this.pressStoryboard.Pause(this.pressStoryboardTarget);
            }
        }

        private void OnPressExtentChanged()
        {
            if (this.pressStoryboard != null)
            {
                this.pressStoryboard.Seek(this.pressStoryboardTarget, TimeSpan.FromSeconds(this.PressExtent), TimeSeekOrigin.BeginTime);
            }
        }

        private void GoToState(string newState)
        {
            if (this.currentVisualState != newState)
            {
                this.currentVisualState = newState;
                VisualStateManager.GoToState(this, newState, true);
            }
        }
    }
}
