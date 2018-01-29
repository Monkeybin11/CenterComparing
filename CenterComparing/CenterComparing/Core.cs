using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
using SpeedyCoding;
using System.Windows.Forms;

namespace CenterComparing
{
    using Img = Image<Gray, byte>;
    using ColorImg = Image<Bgr, byte>;
    using ContourColorSet = Tuple<List<VectorOfPoint>, List<MCvScalar>>;

    public class Core
    {
        public event Action<BitmapSource> evtProcessedImg;
        public event Action<double> evtDistance;
        public event Action evtMultiStart;
        public event Action evtMultiEnd;
        Img BaseImg;
        ColorImg ClrImg;
        ColorImg ClrOriginalImg;

        MCvScalar Outercolor = new MCvScalar(20, 250, 20);
        MCvScalar Innercolor = new MCvScalar(14, 40, 240);

        double RatioW;
        double RatioH;

        public void SaveImg()
        {
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
            ofd.Filter = "Image Files (*.bmp,*.png,*.jpg,*.jpeg) | *.bmp;*.png;*.jpg;*.jpeg";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ClrImg.Save(ofd.FileName);
            }
        }

        public void StartProcessing(Config cfg)
        {
            try
            {
                ClrImg = ClrOriginalImg.Copy();
                if (BaseImg == null)
                {
                    return;
                }

                var img = BaseImg.ThresholdBinary(new Gray(cfg.Threshold), new Gray(255));
                                 //.SmoothMedian( (int)cfg.Resolution * 10 + 1 );
                var contours = new VectorOfVectorOfPoint();
                
                CvInvoke.FindContours(img, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);

                var cntrColorSet = CreateContour_ColorSet(Outercolor, Innercolor, cfg, contours);

                List<VectorOfPoint> cntrlist = cntrColorSet.Item1;
                List<MCvScalar> colorlist = cntrColorSet.Item2;


                var centers = FindCenter(cntrlist);
                var centerlist = new List<double[]>();
                for (int i = 0; i < centers.Count(); i++)
                {
                    CvInvoke.Circle(ClrImg, centers[i], 5, colorlist[i]);
                    var realx = (centers[i].X * RatioW * cfg.Resolution);
                    var realy = (centers[i].Y * RatioH * cfg.Resolution);

                    var x = realx.ToString();
                    var y = realy.ToString();
                    string xy = x + " , " + y + " (um)";

                    System.Drawing.Point textpos = new System.Drawing.Point(centers[i].X - 40 - (i*25), centers[i].Y - 10 - (i*25));
                    CvInvoke.PutText(ClrImg, xy, textpos, FontFace.HersheySimplex, 0.4, colorlist[i]);

                    centerlist.Add(new double[] { realx, realy });
                }
                double errorDistance = CalcDistance(centerlist);
                ClrImg = CenterDiffDraw(centers, ClrImg);

                System.Drawing.Point textdifpos = new System.Drawing.Point(centers[0].X + 40 , centers[0].Y + 10 );
                CvInvoke.PutText(ClrImg, "Error : "+ errorDistance.ToString("F4") + " (um)", textdifpos, FontFace.HersheySimplex, 0.4, new MCvScalar(153, 51, 153));

                var res = ToBitmapSource(ClrImg);
                evtProcessedImg(res);
                evtDistance(errorDistance);
            }
            catch (Exception er)
            {
                er.ToString().Print();
            }
        }


        public ContourColorSet CreateContour_ColorSet(MCvScalar outerColor, MCvScalar innerColor, Config cfg, VectorOfVectorOfPoint contours)
        {
            List<VectorOfPoint> cntrlist = new List<VectorOfPoint>();
            List<MCvScalar> colorlist = new List<MCvScalar>();

            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);

                // outer
                if (area < cfg.OuterUp.ToCircleArea()
                && area > cfg.OuterDw.ToCircleArea())
                {
                    CvInvoke.DrawContours(ClrImg, contours, i, Outercolor, thickness: 3);
                    var cntr = contours[i];
                    cntrlist.Add(cntr);
                    colorlist.Add(Outercolor);
                }

                if (area < cfg.InnerUp.ToCircleArea()
                   && area > cfg.InnerDw.ToCircleArea())
                {
                    CvInvoke.DrawContours(ClrImg, contours, i, Innercolor, thickness: 3);
                    var cntr = contours[i];
                    cntrlist.Add(cntr);
                    colorlist.Add(Innercolor);
                }
            }

            return Tuple.Create(cntrlist, colorlist);
        }

        public double CalcDistance(List<double[]> list)
        {
            if (list.Count != 2) return double.MaxValue;

            var f = list.First();
            var l = list.Last();

            var xerror = Math.Pow((f[0] - l[0]), 2);
            var yerror = Math.Pow((f[1] - l[1]), 2);
            var dis = Math.Sqrt(xerror + yerror);
            return dis;
        }

        public ColorImg CenterDiffDraw(System.Drawing.Point[] points, ColorImg img)
        {
            var line = new LineSegment2D( points[0], points[1]);

             img.Draw(line,new Bgr(153,51,153),1);
           
            return img;
        }


        public System.Drawing.Point[] FindCenter(List<VectorOfPoint> contours)
        {
            System.Drawing.Point[] centerpoins = new System.Drawing.Point[contours.Count];

            for (int i = 0; i < contours.Count; i++)
            {
                var moments = CvInvoke.Moments(contours[i], false);
                centerpoins[i] = new System.Drawing.Point((int)(moments.M10 / moments.M00)
                                            , (int)(moments.M01 / moments.M00));
            }

            return centerpoins;
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }


        #region Load
        public string LoadImage()
        {
            string res = "";
            evtMultiStart();
           
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                ofd.Filter = "Image Files (*.bmp,*.png,*.jpg,*.jpeg) | *.bmp;*.png;*.jpg;*.jpeg";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                res = Task<string>.Run(() =>
                {
                    var tempimg = new Img(ofd.FileName);

                    RatioW = tempimg.Width / 800;
                    RatioH = tempimg.Height / 600;

                    BaseImg = tempimg.Resize(800, 600, Inter.Area);
                    ClrOriginalImg = new ColorImg(ofd.FileName).Resize(800, 600, Inter.Area);
                    return ofd.FileName;
                }).Result;
            }
            else res = "NG";
          
            evtMultiEnd();
            return res;
        }

        public void LoadImageFromClipBoard()
        { }

        public string LoadImageFromDrop(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == ".bmp"
                || ext == ".png"
                || ext == ".jpg"
                || ext == ".jpeg")
            {
                BaseImg = null;

                var tempimg = new Img(path);

                RatioW = tempimg.Width / 800 ;
                RatioH = tempimg.Height / 600;

                BaseImg = tempimg.Resize(800, 600, Inter.Area);
                ClrOriginalImg = new ColorImg(path).Resize(800, 600, Inter.Area);
                return path;
            }
            return "NG";
        }

        #endregion

        #region MultiProcess
        public event Action<int, string> evtNumError;

        public List<MultiAnalysisDatacs> LoadImageMulti()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Image Files (*.bmp,*.png,*.jpg,*.jpeg) | *.bmp;*.png;*.jpg;*.jpeg";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var dirpath = Path.GetDirectoryName(ofd.FileName);
                var pathlist = Directory.GetFiles(dirpath, "*.bmp")
                                .Union(Directory.GetFiles(dirpath, "*.jpg"))
                                .Union(Directory.GetFiles(dirpath, "*.png"))
                                .Union(Directory.GetFiles(dirpath, "*.jpeg"))
                                .OrderBy(x => Path.GetFileName(x))
                                .ToList();

                var output = pathlist.Select((x, i) => new MultiAnalysisDatacs(i, Path.GetFileName(x) ,x)).ToList();
                return output;
            }
            return null;
        }

        // 각 이미지마다 해상도에 맞춰서 프로세싱 하자. 
        public async void StartMultiProcessing(List<MultiAnalysisDatacs> srclist , Config cfg)
        {
            if (cfg == null) System.Windows.MessageBox.Show(" Please Set Config First. ");

            if (srclist.Count < 1) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Choose folder and header name of Result";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string basepath = Path.GetDirectoryName(sfd.FileName);
                string header = Path.GetFileName( sfd.FileName);

                Console.WriteLine(header);
                evtMultiStart();
                await Task.Run(()=> {
                    for (int k = 0; k < srclist.Count; k++)
                    {
                        try
                        {
                            var path = srclist[k].fullname;
                            var tempimg = new Img(path);
                            RatioW = tempimg.Width / 800;
                            RatioH = tempimg.Height / 600;
                            BaseImg = tempimg.Resize(800, 600, Inter.Area);
                            ClrOriginalImg = new ColorImg(path).Resize(800, 600, Inter.Area);
                            ClrImg = ClrOriginalImg.Copy();

                            // Processing
                            var img = BaseImg.ThresholdBinary(new Gray(cfg.Threshold), new Gray(255))
                                     .SmoothMedian((int)cfg.Resolution * 10 + 1);
                            var contours = new VectorOfVectorOfPoint();

                            CvInvoke.FindContours(img, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);

                            var cntrColorSet = CreateContour_ColorSet(Outercolor, Innercolor, cfg, contours);

                            List<VectorOfPoint> cntrlist = cntrColorSet.Item1;
                            List<MCvScalar> colorlist = cntrColorSet.Item2;

                            // Center
                            var centers = FindCenter(cntrlist);
                            var centerlist = new List<double[]>();

                            //Error
                            for (int i = 0; i < centers.Count(); i++)
                            {
                                CvInvoke.Circle(ClrImg, centers[i], 5, colorlist[i]);
                                var realx = (centers[i].X * RatioW * cfg.Resolution);
                                var realy = (centers[i].Y * RatioH * cfg.Resolution);

                                var x = realx.ToString();
                                var y = realy.ToString();
                                string xy = x + " , " + y + " (um)";

                                System.Drawing.Point textpos = new System.Drawing.Point(centers[i].X - 40 - (i * 25), centers[i].Y - 10 - (i * 25));
                                CvInvoke.PutText(ClrImg, xy, textpos, FontFace.HersheySimplex, 0.4, colorlist[i]);

                                centerlist.Add(new double[] { realx, realy });
                            }
                            double errorDistance = CalcDistance(centerlist);
                            ClrImg = CenterDiffDraw(centers, ClrImg);

                            System.Drawing.Point textdifpos = new System.Drawing.Point(centers[0].X + 40, centers[0].Y + 10);
                            CvInvoke.PutText(ClrImg, "Error : " + errorDistance.ToString("F4") + " (um)", textdifpos, FontFace.HersheySimplex, 0.4, new MCvScalar(153, 51, 153));

                            var res = ToBitmapSource(ClrImg);

                            var error = errorDistance.ToString("F4");
                            evtNumError(k, error);
                            srclist[k].error = error;
                            //Save Result 

                            ClrImg.Save(basepath + "\\" + k.ToString() + "_" + header + "_" + srclist[k].name);

                        }
                        catch (Exception er)
                        {
                            er.ToString().Print();
                        }
                    }

                    try
                    {
                        StringBuilder stv = new StringBuilder();
                        stv.AppendLine("Nnmber,FileName,Error");
                        foreach (var item in srclist)
                        {
                            stv.AppendLine(item.no.ToString() + "," + item.name + "," + item.error);
                        }
                        File.WriteAllText(sfd.FileName + "_Result.csv", stv.ToString());
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Please check result file path or file is opened or not");
                    }

                });
                evtMultiEnd();
            }


        }

        #endregion

    }

    public static class Ext
    {
        public static double ToCircleArea(
            this double src)
            => (double)(Math.PI * Math.Pow(src/2, 2));
    }

}
