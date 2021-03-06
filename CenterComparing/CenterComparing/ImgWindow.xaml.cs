﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CenterComparing
{

    /// <summary>
    /// Interaction logic for ImgWindow.xaml
    /// </summary>
    public partial class ImgWindow : UserControl
    {
        public event Action<string> evtCurrentPos;
        public event Action<string> evtDropFile;
        public event Action<int, int> evtImgWidthHeight;
        Point prePosition;
        Point InnerRectFirstPos;
        Point InnerRectLastPos;
        public Rectangle OuterRect = null;
        public Rectangle OuterRect_in = null;
        public Rectangle InnerRect = null;
        public Rectangle InnerRect_in = null;
        public Ellipse InnerEllipse = null;
        public Ellipse OuterEllipse = null;
        BitmapImage OriginalImg;

        public Line HLine = null;
        public Line WLine = null;
        public Label HLabel = null;
        public Label WLabel = null;

        int Margin = 10;

        public bool UseLine = false;
        

        public ImgWindow()
        {
            InitializeComponent();
        }

        public void SetImage(string path)
        {
            if (path == "NG")
            {
                MessageBox.Show("Selected file is not image file. Please select other image file.");
                return;
            }
            ImgBack.Source = new BitmapImage(new Uri(path));
            OriginalImg = new BitmapImage(new Uri(path));
            evtImgWidthHeight((int)ImgBack.Source.Width, (int)ImgBack.Source.Height);
            
        }

        public void SetImage(BitmapSource src)
        {
            ImgBack.Source = src;
        }

        public void RemoveRect()
        {
            cvsMain.Children.Remove(OuterRect);
            cvsMain.Children.Remove(OuterRect_in);
            cvsMain.Children.Remove(InnerRect);
            cvsMain.Children.Remove(InnerRect_in);
        }

        public void RemoveLine()
        {
            cvsMain.Children.Remove(HLine);
            cvsMain.Children.Remove(WLine);
            cvsMain.Children.Remove(HLabel);
            cvsMain.Children.Remove(WLabel);
        }

        public void SetMargin(int num)
        {
            Margin = num;
        }

        public void Reset()
        {
            ImgBack.Source = OriginalImg;
        }

        public Config GetConfig(double resol , int thres , double wratio, double hratio ,bool useline)
        {
            if (useline
                && WLine != null
                && HLine != null
                && InnerRect != null
                && InnerRect_in != null)
            {
                var ltpos = new System.Drawing.Point((int)Math.Min(InnerRectFirstPos.X, InnerRectLastPos.X), (int)Math.Min(InnerRectFirstPos.Y, InnerRectLastPos.Y));
                var rbpos = new System.Drawing.Point((int)Math.Max(InnerRectFirstPos.X, InnerRectLastPos.X), (int)Math.Max(InnerRectFirstPos.Y, InnerRectLastPos.Y));

                var output = new Config()
                {
                    BoxInnerLT = ltpos,
                    BoxInnerRB = rbpos,
                    HX1 = HLine.X1,
                    HY1 = HLine.Y1,
                    HX2 = HLine.X2,
                    HY2 = HLine.Y2,
                    WX1 = WLine.X1,
                    WY1 = WLine.Y1,
                    WX2 = WLine.X2,
                    WY2 = WLine.Y2,
                    InnerUp = Math.Max(InnerRect.Width * wratio, InnerRect.Height * hratio),
                    InnerDw = Math.Min(InnerRect_in.Width * wratio, InnerRect_in.Height * hratio) - 5,
                    Resolution = resol,
                    Threshold = thres,
                    UseLine = true
                };
                return output;
            }
            else if (OuterRect != null
                     && OuterRect_in != null
                     && InnerRect != null
                     && InnerRect_in != null)
            {
                var output = new Config()
                {
                    OuterUp = Math.Max(OuterRect.Width * wratio, OuterRect.Height * hratio),
                    OuterDw = Math.Min(OuterRect_in.Width * wratio, OuterRect_in.Height * hratio) - 5,
                    InnerUp = Math.Max(InnerRect.Width * wratio, InnerRect.Height * hratio),
                    InnerDw = Math.Min(InnerRect_in.Width * wratio, InnerRect_in.Height * hratio) - 5,
                    Resolution = resol,
                    Threshold = thres,
                    UseLine = false
                };
                return output;
            }
            return null;
        }

        private void cvsMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prePosition = e.GetPosition(cvsMain);
            cvsMain.CaptureMouse();
            if (UseLine)
            {
                //Inner
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (HLine == null)
                    {
                        cvsMain.Children.Remove(WLabel);
                        CreateLine(true);
                        CreateLineText(true);
                    }

                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) // Outer
                {
                    if (WLine == null)
                    {
                        cvsMain.Children.Remove(HLabel);
                        CreateLine(false);
                        CreateLineText(false);
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (InnerRect == null)
                    {
                        CreatRectangle(prePosition.X, prePosition.Y, false);
                        InnerRectFirstPos = e.GetPosition(cvsMain);
                    }

                }
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (InnerRect == null)
                    {
                        //InnerEllipse = CreateCircle(prePosition.X, prePosition.Y, false);
                        CreatRectangle(prePosition.X, prePosition.Y, false);
                        InnerRectFirstPos = e.GetPosition(cvsMain);
                    }

                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) // Outer
                {
                    if (OuterRect == null)
                    {
                        //OuterEllipse = CreateCircle(prePosition.X, prePosition.Y, true);
                        CreatRectangle(prePosition.X, prePosition.Y, true);
                    }
                }
            }
        }

        private void cvsMain_MouseMove(object sender, MouseEventArgs e)
        {
            Point posnow = e.GetPosition(cvsMain);
            evtCurrentPos(string.Format("X : {0} , Y : {1}",  Math.Round(posnow.X) , Math.Round(posnow.Y)));

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                double left = prePosition.X;
                double top = prePosition.Y;

                if (prePosition.Y > posnow.Y)
                    top = posnow.Y;
                if (prePosition.X > posnow.X)
                    left = posnow.X;

                var dis = Distance(prePosition.X, posnow.X, prePosition.Y, posnow.Y);
                if (!UseLine)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (InnerRect != null)
                        {
                            cvsMain.Children.Remove(InnerRect);

                            InnerRect.Width = Math.Abs(prePosition.X - posnow.X);
                            InnerRect.Height = Math.Abs(prePosition.Y - posnow.Y);
                            Canvas.SetLeft(InnerRect, left);
                            Canvas.SetTop(InnerRect, top);
                            cvsMain.Children.Add(InnerRect);

                            var innermargin = Margin / 2;

                            cvsMain.Children.Remove(InnerRect_in);
                            var w = Math.Abs(prePosition.X - posnow.X) - innermargin * 2;
                            var h = Math.Abs(prePosition.Y - posnow.Y) - innermargin * 2;
                            InnerRect_in.Width = w <= innermargin * 2 ? 0 : w;
                            InnerRect_in.Height = h <= innermargin * 2 ? 0 : h;
                            Canvas.SetLeft(InnerRect_in, left + innermargin);
                            Canvas.SetTop(InnerRect_in, top + innermargin);
                            cvsMain.Children.Add(InnerRect_in);

                            InnerRectLastPos = posnow;

                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) // Outer
                    {
                        if (OuterRect != null)
                        {
                            cvsMain.Children.Remove(OuterRect);
                            OuterRect.Width = Math.Abs(prePosition.X - posnow.X);
                            OuterRect.Height = Math.Abs(prePosition.Y - posnow.Y);
                            Canvas.SetLeft(OuterRect, left);
                            Canvas.SetTop(OuterRect, top);
                            cvsMain.Children.Add(OuterRect);

                            cvsMain.Children.Remove(OuterRect_in);
                            var w = Math.Abs(prePosition.X - posnow.X) - Margin * 2;
                            var h = Math.Abs(prePosition.Y - posnow.Y) - Margin * 2;
                            OuterRect_in.Width = w <= Margin * 2 ? 0 : w;
                            OuterRect_in.Height = h <= Margin * 2 ? 0 : h;
                            Canvas.SetLeft(OuterRect_in, left + Margin);
                            Canvas.SetTop(OuterRect_in, top + Margin);
                            cvsMain.Children.Add(OuterRect_in);
                        }
                    }
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        if (HLine != null)
                        {
                            cvsMain.Children.Remove(HLine);

                            HLine.X1 = Math.Abs(prePosition.X);
                            HLine.Y1 = Math.Abs(prePosition.Y);
                            HLine.X2 = Math.Abs(posnow.X);
                            HLine.Y2 = Math.Abs(posnow.Y);
                            cvsMain.Children.Add(HLine);

                            cvsMain.Children.Remove(HLabel);
                            Canvas.SetLeft(HLabel, posnow.X + 6);
                            Canvas.SetTop(HLabel, posnow.Y );
                            cvsMain.Children.Add(HLabel);
                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) // Outer
                    {
                        if (WLine != null)
                        {
                            cvsMain.Children.Remove(WLine);
                            WLine.X1 = Math.Abs(prePosition.X);
                            WLine.Y1 = Math.Abs(prePosition.Y);
                            WLine.X2 = Math.Abs(posnow.X);
                            WLine.Y2 = Math.Abs(posnow.Y);
                            cvsMain.Children.Add(WLine);

                            cvsMain.Children.Remove(WLabel);
                            Canvas.SetLeft(WLabel, posnow.X + 6);
                            Canvas.SetTop(WLabel, posnow.Y );
                            cvsMain.Children.Add(WLabel);

                        }
                    }
                    else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        if (InnerRect != null)
                        {
                            cvsMain.Children.Remove(InnerRect);

                            InnerRect.Width = Math.Abs(prePosition.X - posnow.X);
                            InnerRect.Height = Math.Abs(prePosition.Y - posnow.Y);
                            Canvas.SetLeft(InnerRect, left);
                            Canvas.SetTop(InnerRect, top);
                            cvsMain.Children.Add(InnerRect);

                            var innermargin = Margin / 2;

                            cvsMain.Children.Remove(InnerRect_in);
                            var w = Math.Abs(prePosition.X - posnow.X) - innermargin * 2;
                            var h = Math.Abs(prePosition.Y - posnow.Y) - innermargin * 2;
                            InnerRect_in.Width = w <= innermargin * 2 ? 0 : w;
                            InnerRect_in.Height = h <= innermargin * 2 ? 0 : h;
                            Canvas.SetLeft(InnerRect_in, left + innermargin);
                            Canvas.SetTop(InnerRect_in, top + innermargin);
                            cvsMain.Children.Add(InnerRect_in);
                            InnerRectLastPos = posnow;
                        }
                    }

                }

              
            }
        }

        private void cvsMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //cvsMain.Children.Remove(currentRect);
            cvsMain.ReleaseMouseCapture();
            //SetRecrangleProperty();
            //currentRect = null;

            //cvsMain.Children.Remove(WLine);
            //cvsMain.Children.Remove(HLine);

        }

        void SetRecrangleProperty()
        {
            OuterRect.Opacity = 1;
            OuterRect.Fill = new SolidColorBrush(Colors.Transparent);
            OuterRect.StrokeDashArray = new DoubleCollection();
            OuterRect.Stroke = new SolidColorBrush(Colors.IndianRed);
        }

        void CreateLine(bool isHorizontal)
        {
            if (isHorizontal)
            {
                HLine = new Line();
                HLine.StrokeThickness = 3;
                HLine.Stroke = new SolidColorBrush(Colors.MediumPurple);
                HLine.Opacity = 0.7;
                DoubleCollection dashSize = new DoubleCollection();
                dashSize.Add(1);
                dashSize.Add(1);
                HLine.StrokeDashArray = dashSize;
                HLine.StrokeDashOffset = 0;
            }
            else
            {
                WLine = new Line();
                WLine.StrokeThickness = 3;
                WLine.Stroke = new SolidColorBrush(Colors.Orange);
                WLine.Opacity = 0.7;
                DoubleCollection dashSize = new DoubleCollection();
                dashSize.Add(1);
                dashSize.Add(1);
                WLine.StrokeDashArray = dashSize;
                WLine.StrokeDashOffset = 0;
            }
          

        }

        void CreateLineText(bool isHorizontal)
        {
            if (isHorizontal)
            {
                HLabel = new Label();
                HLabel.FontWeight = FontWeights.Bold;
                HLabel.Content = "Vertical";
                HLabel.Foreground = Brushes.Green;

            }
            else
            {
                WLabel = new Label();
                WLabel.FontWeight = FontWeights.Bold;
                WLabel.Content = "Horizontal";
                WLabel.Foreground = Brushes.Green;
            }
        }
        Ellipse CreateCircle(double l, double t, bool isouter)
        {
            Ellipse output = new Ellipse();
            if (isouter)
            {
                output.Stroke = new SolidColorBrush(Colors.ForestGreen);
            }
            else
            {
                output.Stroke = new SolidColorBrush(Colors.OrangeRed);
            }
            output.StrokeThickness = 2;
            output.Opacity = 0.7;
            DoubleCollection dashSize = new DoubleCollection();
            dashSize.Add(1);
            dashSize.Add(1);
            output.StrokeDashArray = dashSize;
            output.StrokeDashOffset = 0;
            output.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            output.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            return output;
        }

        void CreatRectangle(double l , double t , bool isOuter)
        {

            DoubleCollection dashSize = new DoubleCollection();
            dashSize.Add(1);
            dashSize.Add(1);

            if (isOuter)
            {
                OuterRect = new Rectangle();
                OuterRect.Stroke = new SolidColorBrush(Colors.ForestGreen);
                OuterRect.StrokeThickness = 2;
                OuterRect.Opacity = 0.8;

                OuterRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                OuterRect.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                OuterRect_in = new Rectangle();
                OuterRect_in.Stroke = new SolidColorBrush(Colors.ForestGreen);
                OuterRect_in.StrokeThickness = 2;
                OuterRect_in.Opacity = 0.5;

                OuterRect_in.StrokeDashArray = dashSize;
                OuterRect_in.StrokeDashOffset = 0;
                OuterRect_in.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                OuterRect_in.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            }
            else
            {
                InnerRect = new Rectangle();
                InnerRect.Stroke = new SolidColorBrush(Colors.OrangeRed);
                InnerRect.StrokeThickness = 2;
                InnerRect.Opacity = 0.8;

                InnerRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                InnerRect.VerticalAlignment = System.Windows.VerticalAlignment.Top;

                InnerRect_in = new Rectangle();
                InnerRect_in.Stroke = new SolidColorBrush(Colors.OrangeRed);
                InnerRect_in.StrokeThickness = 2;
                InnerRect_in.Opacity = 0.5;

                InnerRect_in.StrokeDashArray = dashSize;
                InnerRect_in.StrokeDashOffset = 0;
                InnerRect_in.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                InnerRect_in.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            }
        }

        private void cvsMain_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            evtDropFile(files.First());

        }

        double Distance(double x0, double y0, double x1, double y1)
        {
            var disdouble = Math.Pow((x0 - x1), 2) + Math.Pow((y0 - y1), 2);
            var dis = Math.Sqrt(disdouble);
            return dis;

        }

    }


       

}
