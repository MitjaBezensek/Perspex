﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Perspex.Controls.Platform;
using Perspex.Input;
using Perspex.Input.Raw;
using Perspex.Media;
using Perspex.Platform;
using Perspex.Skia.iOS;
using UIKit;

namespace Perspex.iOS
{
    class PerspexView : SkiaView, IWindowImpl
    {
        private readonly UIWindow _window;
        private readonly UIViewController _controller;
        private IInputRoot _inputRoot;

        public PerspexView(UIWindow window, UIViewController controller) : base(onFrame => PlatformThreadingInterface.Instance.Render = onFrame)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            _window = window;
            _controller = controller;
            AutoresizingMask = UIViewAutoresizing.All;
            AutoFit();
            UIApplication.Notifications.ObserveDidChangeStatusBarOrientation(delegate { AutoFit(); });
            UIApplication.Notifications.ObserveDidChangeStatusBarFrame(delegate { AutoFit(); });
        }


        void AutoFit()
        {
            var needFlip = !UIDevice.CurrentDevice.CheckSystemVersion(8, 0) &&
                           (_controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft
                            || _controller.InterfaceOrientation == UIInterfaceOrientation.LandscapeRight);

            var frame = UIScreen.MainScreen.Bounds;
            if (needFlip)
                Frame = new CGRect(frame.Y, frame.X, frame.Height, frame.Width);
            else
                Frame = frame;
        }

        public Action Activated { get; set; }
        public Action Closed { get; set; }
        public Action Deactivated { get; set; }
        public Action<RawInputEventArgs> Input { get; set; }
        public Action<Rect> Paint { get; set; }
        public Action<Size> Resized { get; set; }


        public IPlatformHandle Handle => PerspexPlatformHandle;


        public override void LayoutSubviews() => Resized?.Invoke(ClientSize);

        public Size ClientSize
        {
            get { return Bounds.Size.ToPerspex(); }
            set { Resized?.Invoke(ClientSize); }
        }

        public void Activate()
        {
        }

        protected override void Draw()
        {
            Paint?.Invoke(new Rect(new Point(), ClientSize));
        }

        public void Invalidate(Rect rect) => DrawOnNextFrame();

        public void SetInputRoot(IInputRoot inputRoot) => _inputRoot = inputRoot;

        public Point PointToScreen(Point point) => point;

        public void SetCursor(IPlatformHandle cursor)
        {
            //Not supported
        }

        public void Show()
        {
        }

        public Size MaxClientSize => Bounds.Size.ToPerspex();
        public void SetTitle(string title)
        {
            //Not supported
        }

        public IDisposable ShowDialog()
        {
            //Not supported
            return Disposable.Empty;
        }

        public void Hide()
        {
            //Not supported
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var location = touch.LocationInView(this).ToPerspex();

                Input?.Invoke(new RawMouseEventArgs(
                    PerspexAppDelegate.MouseDevice,
                    (uint) touch.Timestamp,
                    _inputRoot,
                    RawMouseEventType.LeftButtonUp,
                    location,
                    InputModifiers.None));
            }
        }

        Point _touchLastPoint;
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var location = touch.LocationInView(this).ToPerspex();
                _touchLastPoint = location;
                Input?.Invoke(new RawMouseEventArgs(PerspexAppDelegate.MouseDevice, (uint) touch.Timestamp, _inputRoot,
                    RawMouseEventType.Move, location, InputModifiers.None));

                Input?.Invoke(new RawMouseEventArgs(PerspexAppDelegate.MouseDevice, (uint) touch.Timestamp, _inputRoot,
                    RawMouseEventType.LeftButtonDown, location, InputModifiers.None));
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                var location = touch.LocationInView(this).ToPerspex();
                if (PerspexAppDelegate.MouseDevice.Captured != null)
                    Input?.Invoke(new RawMouseEventArgs(PerspexAppDelegate.MouseDevice, (uint) touch.Timestamp, _inputRoot,
                        RawMouseEventType.Move, location, InputModifiers.LeftMouseButton));
                else
                {
                    Input?.Invoke(new RawMouseWheelEventArgs(PerspexAppDelegate.MouseDevice, (uint) touch.Timestamp,
                        _inputRoot, location, location - _touchLastPoint, InputModifiers.LeftMouseButton));
                }
                _touchLastPoint = location;
            }
        }


    }

    class PerspexViewController : UIViewController
    {
        public PerspexView PerspexView { get; }

        public PerspexViewController(UIWindow window)
        {
            PerspexView = new PerspexView(window, this);
        }

        public override void LoadView()
        {
            View = PerspexView;
        }
    }
}
