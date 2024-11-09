using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dai;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FindPatterns;
using ZXing.Windows.Compatibility;
using ImageProcessor.Common.Extensions;

namespace KodQR
{
    public class Decode
    {
        public Image<Gray, Byte> img;
        PointF[] points;
        Point[] pointsTopatterns;
        double moduleS;
        int QrS;

        List<int> wysokosc;
        List<int> szerokosc;

        public Decode() { }
        public Decode(Image<Gray,Byte> img, PointF[] pointsNew) {
            this.img = img;  
            this.points = pointsNew;
        }



        public void fromImgToArray()
        {
            cleanImg();
            pattern();
            Qrsize_Vertical();
            Qrsize_horizontal();

            Console.WriteLine($"ModuleSize:{this.moduleS}");
            Console.WriteLine($"Qrsize:{this.QrS}");
            Console.WriteLine($"width:{this.img.Width}");
            double s = this.QrS;
            double mw = this.img.Width / s;
            //mw = Math.Round( mw );

            Console.WriteLine($"Obliczony inaczej ModuleSize:{mw}");
            QrToBitmap(mw);
        }

        public void QrToBitmap(double mw)
        {
            Image<Bgr, Byte> t = this.img.Convert<Bgr, Byte>();
            MCvScalar color = new MCvScalar(255, 0, 255);

            Bitmap map = new Bitmap(this.QrS, this.QrS);
            for (int y = 0; y < this.QrS; y++)
            {
                for(int x=0;x < this.QrS; x++)
                {
                    int startX = (int)((x+0.55) * mw);
                    int startY = (int)((y+0.55) * mw);

                    if (x > this.QrS / 2) {
                        startX = (int)((x + 0.65) * mw);
                    }

                    if (y > this.QrS / 2)
                    {
                        startY = (int)((y + 0.65) * mw);
                    }

                    CvInvoke.Circle(t, new Point(startX, startY), 1, color);
                    int col = this.img.Data[startY,startX,0];
                    Color c = col == 255 ? Color.White : Color.Black;
                    map.SetPixel(x, y, c);

                    Console.Write($"{(col == 255 ? "-":"@")} ");
                }
                Console.WriteLine();
            }


            var decoder = new BarcodeReader();
            var result = decoder.Decode(map);

            if (result != null)
            {
                Console.WriteLine($"Decoded:");
                Console.WriteLine($"{result.Text}");
            }
            else
            {
                Console.WriteLine($"Nie udalo sie");
            }

            CvInvoke.Imshow("xd123", t);
            CvInvoke.WaitKey(0);
        }

        public double Qrsize_horizontal()
        {
            int size = 14;
            Point p1 = new Point(this.pointsTopatterns[0].X, this.pointsTopatterns[0].Y);
            Point p2 = new Point(this.pointsTopatterns[1].X, this.pointsTopatterns[1].Y);
            Point p3 = new Point(this.pointsTopatterns[2].X, this.pointsTopatterns[2].Y);

            while (this.img.Data[p2.Y, p2.X, 0] == 0)
            {
                p2.X++;
            }
            p2.X++;

            int ilosc_zmian = 1;
            int color = this.img.Data[p2.Y, p2.X, 0];
            int lenght = 0;
            List<int> szerokosci = new List<int>();
            szerokosci.Add(p2.X);
            while (p2.X < this.img.Width)
            {
                lenght++;
                if ((Perspective.distance(p2, p3) < this.moduleS) && (this.img.Data[p2.Y, p2.X, 0] == 0))
                {
                    break;
                }

                if ((this.img.Data[p2.Y, p2.X, 0] != color) && (lenght > this.moduleS / 2))
                {
                    ilosc_zmian++;
                    color = color == 0 ? 255 : 0;
                    lenght = 0;
                    szerokosci.Add(p2.X);
                }

                p2.X += 1;
            }
            szerokosci.Add(p2.X);
            size += ilosc_zmian;
            this.QrS = size;

            this.szerokosc = new List<int>();
            for (int i = 0; i < szerokosci.Count - 1; i++)
            {
                int avg = (szerokosci[i] + szerokosci[i + 1]) / 2;
                szerokosc.Add(avg);
            }

            return size;
        }

        public void Qrsize_Vertical()
        {
            int size = 14;
            Point p1 = new Point(this.pointsTopatterns[0].X, this.pointsTopatterns[0].Y);
            Point p2 = new Point(this.pointsTopatterns[1].X, this.pointsTopatterns[1].Y);
            Point p3 = new Point(this.pointsTopatterns[2].X, this.pointsTopatterns[2].Y);

            while (this.img.Data[p2.Y,p2.X,0] == 0)
            {
                p2.Y++;
            }
            p2.Y++;

            int ilosc_zmian = 1;
            int color = this.img.Data[p2.Y, p2.X, 0];
            int lenght = 0;
            List<int> wysokosci = new List<int>();
            wysokosci.Add(p2.Y);
            while ( p2.Y < this.img.Height)
            {
                lenght++;
                if (Perspective.distance(p2, p1) < this.moduleS && this.img.Data[p2.Y, p2.X, 0] == 0)
                {
                    break;
                }

                if (this.img.Data[p2.Y,p2.X,0] != color && lenght > this.moduleS/2)
                {
                    ilosc_zmian++;
                    color = color == 0 ? 255 : 0;
                    lenght = 0;
                    wysokosci.Add(p2.Y);
                }

                p2.Y+=1;
            }
            wysokosci.Add(p2.Y);
            size += ilosc_zmian;
            this.QrS = size;

            this.wysokosc = new List<int>();
            for(int i = 0; i < wysokosci.Count-1; i++)
            {
                int avg = (wysokosci[i] + wysokosci[i+1])/2;
                wysokosc.Add(avg);
            }
        }

        public void pattern()
        {
            MCvScalar color2 = new MCvScalar(255, 0, 255);
            Image<Bgr, Byte> t = img.Convert<Bgr, Byte>();

            foreach(PointF p in this.points)
            {
                CvInvoke.Circle(t, new Point((int)p.X,(int)p.Y), 2, color2);
            }

            Punkt p1 = new Punkt();
            Punkt p2 = new Punkt();
            Punkt p3 = new Punkt();

            p1.X = (int)this.points[0].X;
            p1.Y = (int)this.points[0].Y;

            p2.X = (int)this.points[1].X;
            p2.Y = (int)this.points[1].Y;

            p3.X = (int)this.points[2].X;
            p3.Y = (int)this.points[2].Y;

            RegionDescriptors area1 = FloodFill(p1,true);
            RegionDescriptors area2 = FloodFill(p2,true);
            RegionDescriptors area3 = FloodFill(p3,true);

            Rectangle r1 = new Rectangle();
            r1.X = area1.BoundingBox.X;
            r1.Y = area1.BoundingBox.Y;
            r1.Width = area1.BoundingBox.Width- area1.BoundingBox.X;
            r1.Height = area1.BoundingBox.Height- area1.BoundingBox.Y;

            Rectangle r2 = new Rectangle();
            r2.X = area2.BoundingBox.X;
            r2.Y = area2.BoundingBox.Y;
            r2.Width = area2.BoundingBox.Width - area2.BoundingBox.X;
            r2.Height = area2.BoundingBox.Height - area2.BoundingBox.Y;

            Rectangle r3 = new Rectangle();
            r3.X = area3.BoundingBox.X;
            r3.Y = area3.BoundingBox.Y;
            r3.Width = area3.BoundingBox.Width - area3.BoundingBox.X;
            r3.Height = area3.BoundingBox.Height - area3.BoundingBox.Y;

            CvInvoke.Rectangle(t,r1,color2);
            CvInvoke.Rectangle(t,r2,color2);
            CvInvoke.Rectangle(t,r3,color2);

            double moduleAvg = (((r1.Width+r1.Height)/2.0) + ((r2.Width + r2.Height) / 2.0) + ((r3.Width + r3.Height) / 2.0))/3.0;
            moduleAvg /= 3.0;
            //Console.WriteLine($"ModulSize:{moduleAvg}");
            Point FP1 = area1.Centroid;
            Point FP2 = area2.Centroid;
            Point FP3 = area3.Centroid;
            Point[] toPattern = PatternsToPatterns(moduleAvg,FP1,FP2,FP3,t);
            this.pointsTopatterns = toPattern;
            this.moduleS = moduleAvg;

            //CvInvoke.Imshow("aha", t);
            //CvInvoke.WaitKey(0);
        }

        public Point[] PatternsToPatterns(double modulSize,Point FP1, Point FP2, Point FP3,Image<Bgr,Byte> t)
        {
            Point p3 = new Point();
            p3.X = (int)(FP3.X - ((FP3.X - FP1.X) / (Perspective.distance(FP1, FP3))) * modulSize * Math.Sqrt(18));
            p3.Y = (int)(FP3.Y - ((FP3.Y - FP1.Y) / (Perspective.distance(FP1, FP3))) * modulSize * Math.Sqrt(18));

            Point p1 = new Point();
            p1.X = (int)(FP1.X - ((FP1.X - FP3.X) / (Perspective.distance(FP1, FP3))) * modulSize * Math.Sqrt(18));
            p1.Y = (int)(FP1.Y - ((FP1.Y - FP3.Y) / (Perspective.distance(FP1, FP3))) * modulSize * Math.Sqrt(18));

            Point ps = new Point();
            ps.X = (FP1.X + FP3.X)/2;
            ps.Y = (FP1.Y + FP3.Y)/2;

            Point p2 = new Point();
            p2.X = (int)(FP2.X - ((FP2.X - ps.X) / (Perspective.distance(ps, FP2))) * modulSize * Math.Sqrt(18));
            p2.Y = (int)(FP2.Y - ((FP2.Y - ps.Y) / (Perspective.distance(ps, FP2))) * modulSize * Math.Sqrt(18));

            //Point p2_2 = BetterPunkt2(p2);
            //p2.X = (p2.X + p2_2.X)/ 2;
            //p2.Y = (p2.Y + p2_2.Y)/ 2;

            //Point p3_2 = BetterPunkt1and3(p3);
            //p3.X = (p3.X + p3_2.X) / 2;
            //p3.Y = (p3.Y + p3_2.Y) / 2;

            //Point p1_2 = BetterPunkt1and3(p1);
            //p1.X = (p1.X + p1_2.X) / 2;
            //p1.Y = (p1.Y + p1_2.Y) / 2;

            CvInvoke.Circle(t,p3,2, new MCvScalar(255, 0, 255));
            CvInvoke.Circle(t,p2,2, new MCvScalar(255, 0, 255));
            CvInvoke.Circle(t,p1,2, new MCvScalar(255, 0, 255));

            if(p1.Y > p3.Y)
            {
                return new Point[3] { p1, p2, p3 };
            }
            else
            {
                return new Point[3] { p3, p2, p1 };
            }

        }

        public Point BetterPunkt2(Point p)
        {
            while (this.img.Data[p.Y+1,p.X+1,0] == 0)
            {
                p.X++;
                p.Y++;
            }

            return p;
        }

        public Point BetterPunkt1and3(Point p)
        {
            if(p.Y > this.img.Height / 2)
            {
                while (this.img.Data[p.Y, p.X, 0] == 0)
                {
                    p.X++;
                    //p.Y--;
                }

                return p;
            }
            else
            {
                while (this.img.Data[p.Y, p.X, 0] == 0)
                {
                    //p.X--;
                    p.Y++;
                }

                return p;
            }
        }

        public RegionDescriptors FloodFill(Punkt punkt, bool black)
        {
            int Width = this.img.Width;
            int Height = this.img.Height;

            Stack<System.Drawing.Point> pixels = new Stack<System.Drawing.Point>();
            RegionDescriptors descriptors = new RegionDescriptors();

            pixels.Push(new System.Drawing.Point(punkt.X, punkt.Y));
            descriptors.BoundingBox = new Rectangle(Width, Height, 1, 1);
            bool[,] visited = new bool[Height, Width];
            int expexted_color = black ? 0 : 255;

            System.Drawing.Point p1, p2, p3, p4;


            double maxArea = punkt.w * punkt.w;

            while (pixels.Count > 0)
            {
                System.Drawing.Point p = pixels.Pop();

                visited[p.Y, p.X] = true;

                byte color = this.img.Data[p.Y, p.X, 0];

                if (color == expexted_color)
                {
                    descriptors.Area += 1;
                    descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X + p.X, descriptors.Centroid.Y + p.Y);
                    if (descriptors.BoundingBox.X > p.X) descriptors.BoundingBox.X = p.X;
                    if (descriptors.BoundingBox.Y > p.Y) descriptors.BoundingBox.Y = p.Y;
                    if (descriptors.BoundingBox.Width < p.X) descriptors.BoundingBox.Width = p.X;
                    if (descriptors.BoundingBox.Height < p.Y) descriptors.BoundingBox.Height = p.Y;

                    p1 = new System.Drawing.Point(p.X + 1, p.Y);
                    p2 = new System.Drawing.Point(p.X - 1, p.Y);
                    p3 = new System.Drawing.Point(p.X, p.Y + 1);
                    p4 = new System.Drawing.Point(p.X, p.Y - 1);

                    if (!checkConditions(p1, visited)) pixels.Push(p1);
                    if (!checkConditions(p2, visited)) pixels.Push(p2);
                    if (!checkConditions(p3, visited)) pixels.Push(p3);
                    if (!checkConditions(p4, visited)) pixels.Push(p4);
                }

            }

            if (descriptors.Area > 0)
            {
                descriptors.Centroid = new System.Drawing.Point(descriptors.Centroid.X / descriptors.Area, descriptors.Centroid.Y / descriptors.Area);
            }
            pixels.Clear();
            return descriptors;
        }

        public bool checkConditions(System.Drawing.Point p, bool[,] visited)
        {
            if (p.X < 0 || p.X >= this.img.Width || p.Y < 0 || p.Y >= this.img.Height || visited[p.Y, p.X]) { return true; }
            return false;
        }

        public void cleanImg()
        {
            img = img.SmoothGaussian(1, 1, 34.0, 34.0);
            //CvInvoke.AdaptiveThreshold(img, img, 255.0, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 201, 2.0);
            CvInvoke.Threshold(img,img,128.0,255.0,ThresholdType.Binary);
        }

    }
}
