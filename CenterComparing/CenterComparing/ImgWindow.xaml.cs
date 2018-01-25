using System;
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

        Point prePosition;
        Rectangle currentRect;


        public ImgWindow()
        {
            InitializeComponent();
        }

        private void cvsMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prePosition = e.GetPosition(cvsMain);
            cvsMain.CaptureMouse();
            if (currentRect == null)
                CreatRectangle(prePosition.X ,prePosition.Y);
            

        }

        private void cvsMain_MouseMove(object sender, MouseEventArgs e)
        {
            Point posnow = e.GetPosition(cvsMain);
            evtCurrentPos(string.Format("X : {0} , Y : {1}", posnow.X, posnow.Y));

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                if (currentRect != null)
                {
                    double left = prePosition.X;
                    double top = prePosition.Y;

                    if (prePosition.Y > posnow.Y) // 마우스를 처음위치보다 위로 움직일떄. 처음 포지션은 좌하단이 된다.
                        top = posnow.Y;
                    if (prePosition.X > posnow.X)
                        left = posnow.X;


                    currentRect.Width = Math.Abs(prePosition.X - posnow.X);
                    currentRect.Height = Math.Abs(prePosition.Y - posnow.Y);


                    Canvas.SetLeft(currentRect, left);
                    Canvas.SetTop(currentRect, top);
                    cvsMain.Children.Add(currentRect);


                }


            }
        }

        private void cvsMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cvsMain.Children.Remove(currentRect);
            cvsMain.ReleaseMouseCapture();
            SetRecrangleProperty();
            currentRect = null;
        }

        void SetRecrangleProperty()
        {
            currentRect.Opacity = 1;
            currentRect.Fill = new SolidColorBrush(Colors.Transparent);
            currentRect.StrokeDashArray = new DoubleCollection();
            currentRect.Stroke = new SolidColorBrush(Colors.IndianRed);
        }

        void CreatRectangle(double l , double t)
        {
            currentRect = new Rectangle();
            currentRect.Stroke = new SolidColorBrush(Colors.Red);
            currentRect.StrokeThickness = 2;
            currentRect.Opacity = 0.7;
            DoubleCollection dashSize = new DoubleCollection();
            dashSize.Add(1);
            dashSize.Add(1);
            currentRect.StrokeDashArray = dashSize;
            currentRect.StrokeDashOffset = 0;
            currentRect.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            currentRect.VerticalAlignment = System.Windows.VerticalAlignment.Top;
          

        }

    }
}
