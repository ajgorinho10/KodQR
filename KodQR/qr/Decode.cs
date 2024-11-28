using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using static FindPatterns;
using ZXing.Windows.Compatibility;

namespace KodQR.qr
{
    public class Decode
    {
        public Image<Gray, byte> img;
        public Image<Gray, byte> backup;
        PointF[] points;
        Point[] pointsTopatterns;
        double moduleS;
        int QrS;

        List<int> wysokosc;
        List<int> szerokosc;

        RegionDescriptors p2;
        RegionDescriptors p1;
        RegionDescriptors p3;

        Bitmap QRBitmap;

        public struct statusDecoded
        {
            public string Text;
            public bool Status;
        }

        public statusDecoded DecodedText;

        public Decode() { }
        public Decode(Image<Gray, byte> img, PointF[] pointsNew)
        {
            this.img = img;
            points = pointsNew;
        }



        public void fromImgToArray()
        {
            cleanImg();
            pattern();
            Qrsize_Vertical();
            Qrsize_horizontal();

            double s = QrS;
            double mw = img.Width / s;
            moduleS = mw;

            QrToBitmap(mw, true);
            if (DecodedText.Status == false)
            {
                Image<Gray, byte> im = backup.Copy();
                CvInvoke.CLAHE(im, 15, new Size(8, 8), im);
                CvInvoke.Normalize(im, im, 0, 255, NormType.MinMax, DepthType.Cv8U);
                im = im.SmoothBlur((int)moduleS / 2, (int)moduleS / 2);
                int blur = (int)(moduleS * QrS) / 2;
                if (blur % 2 == 0) blur++;
                CvInvoke.AdaptiveThreshold(im, im, 255.0, AdaptiveThresholdType.MeanC, ThresholdType.Binary, blur, moduleS);

                img = new Image<Gray, byte>(im.Width, im.Height);
                img = im.Copy();

                pattern();
                Qrsize_Vertical();
                //Qrsize_horizontal();
                s = QrS;
                mw = img.Width / s;
                moduleS = mw;
                QrToBitmap(moduleS, true);
                if (DecodedText.Status == true)
                {
                    //Console.WriteLine("dalo");
                }
                else
                {
                    PointF[] srcPoints = new PointF[]
                    {
                        new PointF(0, 0),
                        new PointF(0,img.Height),
                        new PointF(img.Width, img.Height),
                        new PointF(img.Width, 0)
                    };

                    PointF[] desPoints = new PointF[]
                    {
                        new PointF(0, 0),
                        new PointF(img.Width, 0),
                        new PointF(img.Width, img.Height),
                        new PointF(0,img.Height)
                    };
                    Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, desPoints);
                    Size newSize = new Size(img.Width, img.Height);
                    CvInvoke.WarpPerspective(img, img, perspectiveMatrix, newSize, Inter.Linear, Warp.Default, BorderType.Default, new MCvScalar(0, 0, 0));
                    //img = img.SmoothBlur((int)moduleS / 5, (int)moduleS / 5);
                    CvInvoke.AdaptiveThreshold(img, img, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, blur*2+1, moduleS*2);
                    //pattern();
                    //Qrsize_Vertical();
                    QrToBitmap(moduleS, true);
                    if (DecodedText.Status == true)
                    {
                        //Console.WriteLine("dalo2");
                    }
                    else
                    {
                        QrtoBitmap2();

                        if (DecodedText.Status == true)
                        {
                            //Console.WriteLine("dalo3");
                        }
                        else
                        {
                            blur /= 3;
                            if (blur % 2 == 0) { blur++; }
                            CvInvoke.AdaptiveThreshold(img, img, 255.0, AdaptiveThresholdType.MeanC, ThresholdType.Binary,31, 12.0);
                            pattern();
                            Qrsize_Vertical();
                            s = QrS;
                            mw = img.Width / s;
                            moduleS = mw;
                            QrtoBitmap2();
                            if (DecodedText.Status == true) {
                                //Console.WriteLine("dalo4");
                            }
                            else
                            {
                                //fixBitmap(1, 0);
                                //fixBitmap(1, this.QRBitmap.Height-8);
                                //PrintBitmap();
                            }
                        }
                    }
                }
            }
        }

        public void fixBitmap(int startX,int startY)
        {
            //Line 1
            this.QRBitmap.SetPixel(startX, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+1, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+2, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+3, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+4, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+5, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+6, startY, Color.Black);
            this.QRBitmap.SetPixel(startX+7, startY, Color.White);

            //Line 2
            this.QRBitmap.SetPixel(startX, startY + 1, Color.Black);
            this.QRBitmap.SetPixel(startX+1, startY + 1, Color.White);
            this.QRBitmap.SetPixel(startX+2, startY + 1, Color.White);
            this.QRBitmap.SetPixel(startX+3, startY + 1, Color.White);
            this.QRBitmap.SetPixel(startX+4, startY + 1, Color.White);
            this.QRBitmap.SetPixel(startX+5, startY + 1, Color.White);
            this.QRBitmap.SetPixel(startX+6, startY + 1, Color.Black);
            this.QRBitmap.SetPixel(startX+7, startY + 1, Color.White);

            //Line 3
            this.QRBitmap.SetPixel(startX, startY + 2, Color.Black);
            this.QRBitmap.SetPixel(startX + 1, startY + 2, Color.White);
            this.QRBitmap.SetPixel(startX + 2, startY + 2, Color.Black);
            this.QRBitmap.SetPixel(startX + 3, startY + 2, Color.Black);
            this.QRBitmap.SetPixel(startX + 4, startY + 2, Color.Black);
            this.QRBitmap.SetPixel(startX + 5, startY + 2, Color.White);
            this.QRBitmap.SetPixel(startX + 6, startY + 2, Color.Black);
            this.QRBitmap.SetPixel(startX + 7, startY + 2, Color.White);

            //Line 4
            this.QRBitmap.SetPixel(startX, startY + 3, Color.Black);
            this.QRBitmap.SetPixel(startX + 1, startY + 3, Color.White);
            this.QRBitmap.SetPixel(startX + 2, startY + 3, Color.Black);
            this.QRBitmap.SetPixel(startX + 3, startY + 3, Color.Black);
            this.QRBitmap.SetPixel(startX + 4, startY + 3, Color.Black);
            this.QRBitmap.SetPixel(startX + 5, startY + 3, Color.White);
            this.QRBitmap.SetPixel(startX + 6, startY + 3, Color.Black);
            this.QRBitmap.SetPixel(startX + 7, startY + 3, Color.White);

            //Line 5
            this.QRBitmap.SetPixel(startX, startY + 4, Color.Black);
            this.QRBitmap.SetPixel(startX + 1, startY + 4, Color.White);
            this.QRBitmap.SetPixel(startX + 2, startY + 4, Color.Black);
            this.QRBitmap.SetPixel(startX + 3, startY + 4, Color.Black);
            this.QRBitmap.SetPixel(startX + 4, startY + 4, Color.Black);
            this.QRBitmap.SetPixel(startX + 5, startY + 4, Color.White);
            this.QRBitmap.SetPixel(startX + 6, startY + 4, Color.Black);
            this.QRBitmap.SetPixel(startX + 7, startY + 4, Color.White);

            //Line 6
            this.QRBitmap.SetPixel(startX, startY + 5, Color.Black);
            this.QRBitmap.SetPixel(startX + 1, startY + 5, Color.White);
            this.QRBitmap.SetPixel(startX + 2, startY + 5, Color.White);
            this.QRBitmap.SetPixel(startX + 3, startY + 5, Color.White);
            this.QRBitmap.SetPixel(startX + 4, startY + 5, Color.White);
            this.QRBitmap.SetPixel(startX + 5, startY + 5, Color.White);
            this.QRBitmap.SetPixel(startX + 6, startY + 5, Color.Black);
            this.QRBitmap.SetPixel(startX + 7, startY + 5, Color.White);

            //Line 7
            this.QRBitmap.SetPixel(startX, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 1, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 2, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 3, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 4, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 5, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 6, startY+6, Color.Black);
            this.QRBitmap.SetPixel(startX + 7, startY+6, Color.White);

            //Line 8
            this.QRBitmap.SetPixel(startX, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 1, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 2, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 3, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 4, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 5, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 6, startY + 7, Color.White);
            this.QRBitmap.SetPixel(startX + 7, startY + 7, Color.White);
        }

        public void PrintBitmap()
        {
            for (int i = 0; i < QRBitmap.Height; i++)
            {
                for (int j = 0; j < QRBitmap.Width; j++) {
                    Color c = QRBitmap.GetPixel(i, j);
                    Console.Write($"{(c.R == 255 ? "@" : "-")} ");
                }
                Console.WriteLine();
            }

            CvInvoke.Imshow("XD", this.img);
            CvInvoke.WaitKey(0);
        }

        public void QrtoBitmap2()
        {
            Bitmap map = new Bitmap(QrS, QrS);
            MCvScalar color = new MCvScalar(255, 0, 255);
            Image<Bgr, byte> im = img.Convert<Bgr, byte>();
            for (int y = 0; y < QrS; y++)
            {
                for (int x = 0; x < QrS; x++)
                {
                    Mat m = img.Mat;
                    int startx = (int)((x + 0.60) * moduleS);
                    int starty = (int)((y + 0.60) * moduleS);
                    int endx = (int)(moduleS / 1.75);
                    int endy = (int)(moduleS / 1.75);

                    if (startx + endx >= img.Width)
                    {
                        endx = img.Width - startx-1;
                    }
                    if (starty + endy >= img.Height)
                    {
                        endy = img.Height - starty-1;
                    }

                    if(startx >= img.Width)
                    {
                        startx = img.Width - startx-1;
                    }

                    if(starty >= img.Height)
                    {
                        starty = img.Height - starty-1;
                    }

                    if(startx < 0)
                    {
                        startx = 0;
                    }
                    if (starty < 0) 
                    { 
                        starty = 0;
                    }

                    Rectangle squareRegion = new Rectangle(startx, starty, endx, endy);
                    Mat square = new Mat(m, squareRegion);
                    MCvScalar meanValue = CvInvoke.Mean(square);
                    Color c = meanValue.V0 < 128 ? Color.White : Color.Black;
                    map.SetPixel(x, y, c);
                    // Console.Write($"{(c.R != 255 ? "-" : "@")} ");
                    CvInvoke.Rectangle(im, squareRegion, color);
                }
                //Console.WriteLine();
            }

            //CvInvoke.Imshow("aha",im);
            //CvInvoke.WaitKey(0);
            QRBitmap = map;
            var decoder = new BarcodeReader();
            decoder.Options.TryInverted = true;
            var result = decoder.Decode(map);

            if (result != null)
            {
                DecodedText.Text = result.Text;
                DecodedText.Status = true;
            }
            else
            {
                DecodedText.Text = "";
                DecodedText.Status = false;
            }
        }

        public void QrToBitmap(double mw, bool czy2)
        {
            Image<Bgr, byte> t = img.Convert<Bgr, byte>();
            MCvScalar color = new MCvScalar(255, 0, 255);

            Bitmap map = new Bitmap(QrS, QrS);
            for (int y = 0; y < QrS; y++)
            {
                for (int x = 0; x < QrS; x++)
                {

                    int startX = (int)((x + 0.55) * mw);
                    int startY = (int)((y + 0.55) * mw);

                    if(startX > img.Width-1)
                    {
                        startX = img.Width-1;
                    }

                    if(startY > img.Height-1)
                    {
                        startY = img.Height-1;
                    }
                    int col = img.Data[startY, startX, 0];

                    if (czy2)
                    {
                        if (startX > img.Width / 2.0 && startY > img.Height / 2.0)
                        {
                            startX = (int)((x + 0.60) * mw);
                        }
                        if (startY > img.Height / 2.0)
                        {
                            startY = (int)((y + 0.65) * mw);
                        }
                    }
                    else
                    {
                        startX = (int)((x + 0.70) * mw);
                        startY = (int)((y + 0.72) * mw);
                    }

                    Color c = col == 255 ? Color.White : Color.Black;
                    map.SetPixel(x, y, c);
                }
            }

            QRBitmap = map;
            var decoder = new BarcodeReader();
            decoder.Options.TryInverted = true;
            var result = decoder.Decode(map);
            map = new Bitmap(QrS, QrS);

            if (result != null)
            {
                DecodedText.Text = result.Text;
                DecodedText.Status = true;
            }
            else
            {
                DecodedText.Text = "";
                DecodedText.Status = false;
            }
        }

        public double Qrsize_horizontal()
        {
            int size = 14;
            Point p1 = new Point(pointsTopatterns[0].X, pointsTopatterns[0].Y);
            Point p2 = new Point(pointsTopatterns[1].X, pointsTopatterns[1].Y);
            Point p3 = new Point(pointsTopatterns[2].X, pointsTopatterns[2].Y);

            while (img.Data[p2.Y, p2.X, 0] == 0)
            {
                p2.X++;
            }
            p2.X++;

            int ilosc_zmian = 1;
            int color = img.Data[p2.Y, p2.X, 0];
            int lenght = 0;
            List<int> szerokosci = new List<int>();
            szerokosci.Add(p2.X);
            while (p2.X < img.Width)
            {
                lenght++;
                if (Perspective.distance(p2, p3) < moduleS && img.Data[p2.Y, p2.X, 0] == 0)
                {
                    break;
                }

                if (img.Data[p2.Y, p2.X, 0] != color && lenght >= moduleS / 1.5)
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
            QrS = size;

            szerokosc = new List<int>();
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
            Point p1 = new Point(pointsTopatterns[0].X, pointsTopatterns[0].Y);
            Point p2 = new Point(pointsTopatterns[1].X, pointsTopatterns[1].Y);
            Point p3 = new Point(pointsTopatterns[2].X, pointsTopatterns[2].Y);

            while (p2.Y < img.Height && p2.X < img.Width && p2.Y > 0 && p2.X > 0 && img.Data[p2.Y, p2.X, 0] == 0)
            {
                p2.Y++;
            }
            p2.Y++;

            int ilosc_zmian = 1;
            if(p2.Y > img.Height || p2.X > img.Width || p2.Y < 0 || p2.X < 0)
            {
                return;
            }
            int color = img.Data[p2.Y, p2.X, 0];
            int lenght = 0;
            List<int> wysokosci = new List<int>();
            wysokosci.Add(p2.Y);
            while (p2.Y < img.Height)
            {
                lenght++;
                if (Perspective.distance(p2, p1) < moduleS && img.Data[p2.Y, p2.X, 0] == 0)
                {
                    break;
                }

                if (img.Data[p2.Y, p2.X, 0] != color && lenght > moduleS / 2)
                {
                    ilosc_zmian++;
                    color = color == 0 ? 255 : 0;
                    lenght = 0;
                    wysokosci.Add(p2.Y);
                }

                p2.Y += 1;
            }
            wysokosci.Add(p2.Y);
            size += ilosc_zmian;
            QrS = size;

            wysokosc = new List<int>();
            for (int i = 0; i < wysokosci.Count - 1; i++)
            {
                int avg = (wysokosci[i] + wysokosci[i + 1]) / 2;
                wysokosc.Add(avg);
            }
        }

        public void pattern()
        {
            MCvScalar color2 = new MCvScalar(255, 0, 255);
            Image<Bgr, byte> t = img.Convert<Bgr, byte>();

            foreach (PointF p in points)
            {
                CvInvoke.Circle(t, new Point((int)p.X, (int)p.Y), 2, color2);
            }

            Punkt p1 = new Punkt();
            Punkt p2 = new Punkt();
            Punkt p3 = new Punkt();

            p1.X = (int)points[0].X;
            p1.Y = (int)points[0].Y;

            p2.X = (int)points[1].X;
            p2.Y = (int)points[1].Y;

            p3.X = (int)points[2].X;
            p3.Y = (int)points[2].Y;

            RegionDescriptors area1 = FloodFill(p1, true);
            RegionDescriptors area2 = FloodFill(p2, true);
            RegionDescriptors area3 = FloodFill(p3, true);

            this.p1 = area1;
            this.p2 = area2;
            this.p3 = area3;

            Rectangle r1 = new Rectangle();
            r1.X = area1.BoundingBox.X;
            r1.Y = area1.BoundingBox.Y;
            r1.Width = area1.BoundingBox.Width - area1.BoundingBox.X;
            r1.Height = area1.BoundingBox.Height - area1.BoundingBox.Y;
            this.p1.BoundingBox = r1;

            Rectangle r2 = new Rectangle();
            r2.X = area2.BoundingBox.X;
            r2.Y = area2.BoundingBox.Y;
            r2.Width = area2.BoundingBox.Width - area2.BoundingBox.X;
            r2.Height = area2.BoundingBox.Height - area2.BoundingBox.Y;
            this.p2.BoundingBox = r2;

            Rectangle r3 = new Rectangle();
            r3.X = area3.BoundingBox.X;
            r3.Y = area3.BoundingBox.Y;
            r3.Width = area3.BoundingBox.Width - area3.BoundingBox.X;
            r3.Height = area3.BoundingBox.Height - area3.BoundingBox.Y;
            this.p3.BoundingBox = r3;

            CvInvoke.Rectangle(t, r1, color2);
            CvInvoke.Rectangle(t, r2, color2);
            CvInvoke.Rectangle(t, r3, color2);

            double moduleAvg = ((r1.Width + r1.Height) / 2.0 + (r2.Width + r2.Height) / 2.0 + (r3.Width + r3.Height) / 2.0) / 3.0;
            moduleAvg /= 3.0;
            //Console.WriteLine($"ModulSize:{moduleAvg}");
            Point FP1 = area1.Centroid;
            Point FP2 = area2.Centroid;
            Point FP3 = area3.Centroid;
            Point[] toPattern = PatternsToPatterns(moduleAvg, FP1, FP2, FP3, t);
            pointsTopatterns = toPattern;
            moduleS = moduleAvg;

            //CvInvoke.Imshow("aha", t);
            //CvInvoke.WaitKey(0);
        }

        public Point[] PatternsToPatterns(double modulSize, Point FP1, Point FP2, Point FP3, Image<Bgr, byte> t)
        {
            Point p3 = new Point();
            p3.X = (int)(FP3.X - (FP3.X - FP1.X) / Perspective.distance(FP1, FP3) * modulSize * Math.Sqrt(18));
            p3.Y = (int)(FP3.Y - (FP3.Y - FP1.Y) / Perspective.distance(FP1, FP3) * modulSize * Math.Sqrt(18));

            Point p1 = new Point();
            p1.X = (int)(FP1.X - (FP1.X - FP3.X) / Perspective.distance(FP1, FP3) * modulSize * Math.Sqrt(18));
            p1.Y = (int)(FP1.Y - (FP1.Y - FP3.Y) / Perspective.distance(FP1, FP3) * modulSize * Math.Sqrt(18));

            Point ps = new Point();
            ps.X = (FP1.X + FP3.X) / 2;
            ps.Y = (FP1.Y + FP3.Y) / 2;

            Point p2 = new Point();
            p2.X = (int)(FP2.X - (FP2.X - ps.X) / Perspective.distance(ps, FP2) * modulSize * Math.Sqrt(18));
            p2.Y = (int)(FP2.Y - (FP2.Y - ps.Y) / Perspective.distance(ps, FP2) * modulSize * Math.Sqrt(18));

            //Point p2_2 = BetterPunkt2(p2);
            //p2.X = (p2.X + p2_2.X)/ 2;
            //p2.Y = (p2.Y + p2_2.Y)/ 2;

            //Point p3_2 = BetterPunkt1and3(p3);
            //p3.X = (p3.X + p3_2.X) / 2;
            //p3.Y = (p3.Y + p3_2.Y) / 2;

            //Point p1_2 = BetterPunkt1and3(p1);
            //p1.X = (p1.X + p1_2.X) / 2;
            //p1.Y = (p1.Y + p1_2.Y) / 2;

            CvInvoke.Circle(t, p3, 2, new MCvScalar(255, 0, 255));
            CvInvoke.Circle(t, p2, 2, new MCvScalar(255, 0, 255));
            CvInvoke.Circle(t, p1, 2, new MCvScalar(255, 0, 255));

            if (p1.X < p3.X)
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
            while (img.Data[p.Y + 1, p.X + 1, 0] == 0)
            {
                p.X++;
                p.Y++;
            }

            return p;
        }

        public Point BetterPunkt1and3(Point p)
        {
            if (p.Y > img.Height / 2)
            {
                while (img.Data[p.Y, p.X, 0] == 0)
                {
                    p.X++;
                    //p.Y--;
                }

                return p;
            }
            else
            {
                while (img.Data[p.Y, p.X, 0] == 0)
                {
                    //p.X--;
                    p.Y++;
                }

                return p;
            }
        }

        public RegionDescriptors FloodFill(Punkt punkt, bool black)
        {
            int Width = img.Width;
            int Height = img.Height;

            Stack<Point> pixels = new Stack<Point>();
            RegionDescriptors descriptors = new RegionDescriptors();

            pixels.Push(new Point(punkt.X, punkt.Y));
            descriptors.BoundingBox = new Rectangle(Width, Height, 1, 1);
            bool[,] visited = new bool[Height, Width];
            int expexted_color = black ? 0 : 255;

            Point p1, p2, p3, p4;


            double maxArea = punkt.w * punkt.w;

            while (pixels.Count > 0)
            {
                Point p = pixels.Pop();

                visited[p.Y, p.X] = true;

                byte color = img.Data[p.Y, p.X, 0];

                if (color == expexted_color)
                {
                    descriptors.Area += 1;
                    descriptors.Centroid = new Point(descriptors.Centroid.X + p.X, descriptors.Centroid.Y + p.Y);
                    if (descriptors.BoundingBox.X > p.X) descriptors.BoundingBox.X = p.X;
                    if (descriptors.BoundingBox.Y > p.Y) descriptors.BoundingBox.Y = p.Y;
                    if (descriptors.BoundingBox.Width < p.X) descriptors.BoundingBox.Width = p.X;
                    if (descriptors.BoundingBox.Height < p.Y) descriptors.BoundingBox.Height = p.Y;

                    p1 = new Point(p.X + 1, p.Y);
                    p2 = new Point(p.X - 1, p.Y);
                    p3 = new Point(p.X, p.Y + 1);
                    p4 = new Point(p.X, p.Y - 1);

                    if (!checkConditions(p1, visited)) pixels.Push(p1);
                    if (!checkConditions(p2, visited)) pixels.Push(p2);
                    if (!checkConditions(p3, visited)) pixels.Push(p3);
                    if (!checkConditions(p4, visited)) pixels.Push(p4);
                }

            }

            if (descriptors.Area > 0)
            {
                descriptors.Centroid = new Point(descriptors.Centroid.X / descriptors.Area, descriptors.Centroid.Y / descriptors.Area);
            }
            pixels.Clear();
            return descriptors;
        }

        public bool checkConditions(Point p, bool[,] visited)
        {
            if (p.X < 0 || p.X >= img.Width || p.Y < 0 || p.Y >= img.Height || visited[p.Y, p.X]) { return true; }
            return false;
        }

        public void cleanImg()
        {
            backup = img.Copy();
            Image<Gray, byte> im = img.Convert<Gray, byte>();
            //im = im.SmoothMedian(1);
            im = im.SmoothMedian(1);
            CvInvoke.Threshold(im, im, 0, 255.0, ThresholdType.Binary | ThresholdType.Otsu);
            img = im.Convert<Gray, byte>();
        }

    }
}
