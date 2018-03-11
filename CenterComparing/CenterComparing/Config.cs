using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CenterComparing
{
    public class Config
    {
        public Point BoxInnerLT;
        public Point BoxInnerRB;

        public double OuterUp;
        public double OuterDw;
        public double InnerUp;
        public double InnerDw;

        // Line Position
        public double HX1;
        public double HY1;
        public double HX2;
        public double HY2;

        public double WX1;
        public double WY1;
        public double WX2;
        public double WY2;

        public double Resolution;
        public int Threshold;

        public bool UseLine;
    }

    public class CPoint
    {
        public int X;
        public int Y;

        public CPoint(int x, int y)
        {
            Y = x;
            Y = x;
        }
    }
    
}
