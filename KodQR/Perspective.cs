using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using Accord.Math.Geometry;
using System.ComponentModel.Design;
using Accord.IO;
using Emgu.CV.Util;
using Accord.Imaging.Filters;
using Accord.Statistics.Kernels;
using static ZXing.Rendering.SvgRenderer;


namespace KodQR
{
    public class Perspective
    {
        Image<Gray, Byte> img;
        int width;
        int height;

        public Perspective(Image<Gray, Byte> image)
        {
            this.img = image;
            this.width = image.Cols;
            this.height = image.Rows;
        }

        public Perspective() { }

        public double distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public void SetUpPerspective(Point p1, Point p2, Point p3,Punkt punkt1, Punkt punkt2, Punkt punkt3)
        {
            Console.WriteLine("Utworzenie perspektywy");

            QuietZone q = new QuietZone(this.img);

            MCvScalar color4 = new MCvScalar(255, 0, 0);
            Image<Bgr, Byte> image = this.img.Convert<Bgr, Byte>();

            Mat cannyEdges = new Mat();
            CvInvoke.Canny(image, cannyEdges, 0.0,255.0);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(cannyEdges, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);

            MCvScalar color = new MCvScalar(255, 0, 255);
            CvInvoke.Circle(
                    image,
                    p3,
                    5,
                    color,
                    -1
                );

            CvInvoke.Circle(
                    image,
                    p1,
                    5,
                    color,
                    -1
                );

            Point point1_1 = new Point();
            Point point1_2 = new Point();

            Point point3_1 = new Point();
            Point point3_2 = new Point();

            Point point2_1 = p1;

            for (int i = 0; i < contours.Size; i++)
            {
                VectorOfPoint contour = contours[i];

                Point p = new Point();

                for (int j = 1; j < contour.Size; j++)
                {
                    var c = contour[j];

                    p.X = c.X;
                    p.Y = c.Y;
                    double dis = distance(p, p3);
                    if (dis < punkt3.w / 4.5)
                    {
                        CvInvoke.DrawContours(image, contours, i, new MCvScalar(255, 0, 0), 2);

                        double epsilon = 0.01 * CvInvoke.ArcLength(contour, true);
                        VectorOfPoint approx = new VectorOfPoint();
                        CvInvoke.ApproxPolyDP(contour, approx, epsilon, true);

                        bool czyObrecz = false;
                        for (int z = 0; z < approx.Size; z++)
                        {
                            var x1 = approx[z];
                            double distance1 = distance(x1, p3);
                            double distance2 = distance(point3_1, p3);

                            double distance3 = distance(x1, new System.Drawing.Point(punkt3.X, punkt3.Y));
                            double distance4 = distance(point3_1, new System.Drawing.Point(punkt3.X, punkt3.Y));
                            if ((distance1 < distance2)&&(distance1 < distance3) && (distance3<=(distance4*1.3)))
                            {
                                    point3_1 = x1;
                                    czyObrecz = true;
                            }
                        }

                        if (czyObrecz)
                        {
                            point3_2 = approx[0];
                            for (int z = 0; z < approx.Size; z++)
                            {
                                var x1 = approx[z];
                                double distance4 = distance(x1, p2);
                                double distance5 = distance(point3_2, p2);
                                if(distance4 > distance5)
                                {
                                    point3_2 = x1;
                                }
                            }
                        }
                    }

                    dis = distance(p, p1);
                    if (dis < punkt1.w / 4.5)
                    {
                        CvInvoke.DrawContours(image, contours, i, new MCvScalar(255, 0, 0), 2);

                        double epsilon = 0.01 * CvInvoke.ArcLength(contour, true);
                        VectorOfPoint approx = new VectorOfPoint();
                        CvInvoke.ApproxPolyDP(contour, approx, epsilon, true);

                        bool czyObrecz = false;
                        for (int z = 0; z < approx.Size; z++)
                        {
                            var x1 = approx[z];
                            double distance1 = distance(x1, p1);
                            double distance2 = distance(point1_1, p1);
                            if (distance1 < distance2)
                            {
                                double distance3 = distance(x1, new System.Drawing.Point(punkt1.X, punkt1.Y));
                                double distance4 = distance(point1_1, new System.Drawing.Point(punkt1.X, punkt1.Y));
                                if ((distance1 < distance3) && (distance3 <= (distance4 * 1.3)))
                                {
                                    point1_1 = x1;
                                    czyObrecz = true;
                                }
                                

                            }
                        }

                        if (czyObrecz)
                        {
                            point1_2 = approx[0];
                            for (int z = 0; z < approx.Size; z++)
                            {
                                var x1 = approx[z];
                                double distance4 = distance(x1, p2);
                                double distance5 = distance(point1_2, p2);
                                if (distance4 > distance5)
                                {
                                    point1_2 = x1;
                                }
                            }
                        }
                    }



                    dis = distance(p, p2);
                    if (dis < punkt2.w / 4.5)
                    {
                        CvInvoke.DrawContours(image, contours, i, new MCvScalar(255, 0, 0), 2);

                        double epsilon = 0.01 * CvInvoke.ArcLength(contour, false);
                        VectorOfPoint approx = new VectorOfPoint();
                        CvInvoke.ApproxPolyDP(contour, approx, epsilon, false);
                        for (int z = 0; z < approx.Size; z++)
                        {
                            var x1 = approx[z];
                            double distance1 = distance(x1, p2);
                            double distance2 = distance(point2_1, p2);
                            if (distance1 < distance2)
                            {
                                double distance3 = distance(x1, new System.Drawing.Point(punkt2.X, punkt2.Y)); ;
                                double distance4 = distance(point2_1, new System.Drawing.Point(punkt2.X, punkt2.Y));

                                if ((distance1 < distance3) && (distance3 <= (distance4 * 1.3)))
                                {
                                    point2_1 = x1;
                                }
                                
                            }
                        }
                    }
                }

            }

            MCvScalar color2 = new MCvScalar(0, 255, 0);
            CvInvoke.Circle(
                image,
                point3_1,
                5,
                color2,
                -1
            );

            MCvScalar color3 = new MCvScalar(255, 0, 0);
            CvInvoke.Circle(
                image,
                point1_1,
                5,
                color3,
                -1
            );

            MCvScalar color5 = new MCvScalar(0, 255, 255);
            CvInvoke.Circle(
                image,
                point3_2,
                5,
                color5,
                -1
            );

            MCvScalar color6 = new MCvScalar(0, 0, 255);
            CvInvoke.Circle(
                image,
                point1_2,
                5,
                color6,
                -1
            );

            CvInvoke.Circle(
                image,
                point2_1,
                5,
                color6,
                -1
            );

            if(Math.Abs(point2_1.X - point1_1.X) <= 2.0)
            {
                point1_1.X = point2_1.X;
            }

            if (Math.Abs(point2_1.Y - point1_1.Y) <= 2.0)
            {
                point1_1.Y = point2_1.Y;
            }

            if (Math.Abs(point2_1.X - point3_1.X) <= 2.0)
            {
                point3_1.X = point2_1.X;
            }

            if (Math.Abs(point2_1.Y - point3_1.Y) <= 2.0)
            {
                point3_1.Y = point2_1.Y;
            }



            if (Math.Abs(point1_1.X-point1_2.X) <= 1)
            {
                point1_2.X = point1_1.X;
            }

            if (Math.Abs(point1_1.Y - point1_2.Y) <= 1)
            {
                point1_2.Y = point1_1.Y;
            }

            if (Math.Abs(point3_1.Y - point3_2.Y) <= 1)
            {
                point3_2.Y = point3_1.Y;
            }

            if (Math.Abs(point3_1.X - point3_2.X) <= 1)
            {
                point3_2.X = point3_1.X;
            }

            double a1=0, a2=0;
            double x = 0,y=0;
            double mianownikA1 = (double)(point1_1.X - point1_2.X);
            double mianownikA2 = (double)(point3_1.X - point3_2.X);
            double licznikA1 = (double)(point1_1.Y - point1_2.Y);
            double licznikA2 = (double)(point3_1.Y - point3_2.Y);

            if (mianownikA1 == 0)
            {
                a1 = 0;
            }
            else
            {
                a1 = (double)(licznikA1) / mianownikA1;
            }

            if (mianownikA2 == 0)
            {
                a2 = 0;
            }
            else
            {
                a2 = (double)(licznikA2) / mianownikA2;
            }

            double b1 = point1_2.Y - (a1 * point1_2.X);
            double b2 = point3_2.Y - (a2 * point3_2.X);

            Console.WriteLine($"a1: {a1}, a2: {a2}");
            Console.WriteLine($"b1: {b1}, b2: {b2}");

            if (mianownikA1 !=0 && mianownikA2!=0)
            {
                x = (b2 - b1) / (a1 - a2);
                y = a1 * x + b1;
            }
            else
            {
                x = point1_1.X > point3_1.X ? point1_1.X : point3_1.X;
                y = point1_1.Y > point3_1.Y ? point1_1.Y : point3_1.Y;
            }

            MCvScalar color7 = new MCvScalar(255, 0, 255);
            CvInvoke.Circle(
                image,
                new System.Drawing.Point((int)x,(int)y),
                5,
                color6,
                -1
            );

            CvInvoke.Line(
                image,
                point1_1,
                new System.Drawing.Point((int)x, (int)y),
                color,
                1
            );

            CvInvoke.Line(
                image,
                point3_1,
                new System.Drawing.Point((int)x, (int)y),
                color,
                1
            );

            // Wyświetlenie obrazu z konturami i wierzchołkami
            Console.WriteLine($"p1:{point1_1.X},{point1_1.Y} p1_2:{point1_2.X},{point1_2.Y}");
            Console.WriteLine($"p3:{point3_1.X},{point3_1.Y} p3_2:{point3_2.X},{point3_2.Y}");
            Console.WriteLine($"p2:{point2_1.X},{point2_1.Y}");
            Console.WriteLine($"p4:{x},{y}");
            image.Save("perspektywa.png");
            Process.Start(new ProcessStartInfo("perspektywa.png") { UseShellExecute = true });

            
            PointF[] srcPoints = new PointF[]
            {
            new PointF(point2_1.X, point2_1.Y),
            new PointF(point1_1.X, point1_1.Y),
            new PointF((int)x, (int)y),
            new PointF(point3_1.X, point3_1.Y)
            };

            double dis_X = punkt2.w*7.0;
            double dis_Y = punkt2.w*7.0;

            PointF[] dstPoints = new PointF[]
            {
            new PointF(0, 0),
            new PointF((float)dis_X, 0),
            new PointF((float)dis_X, (float)dis_Y),
            new PointF(0, (float)dis_Y)
            };

            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, dstPoints);

            // Wynikowy obraz po transformacji
            Mat dstImage = new Mat();
            Size newSize = new Size((int)dis_X, (int)dis_Y); // docelowy rozmiar obrazu

            // Zastosowanie transformacji perspektywicznej
            CvInvoke.WarpPerspective(this.img, dstImage, perspectiveMatrix, newSize, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(0, 0, 0));

            // Wyświetlenie wynikowego obrazu
            CvInvoke.Imshow("Transformed Image", dstImage);
            CvInvoke.WaitKey(0);

        }

        private static double Angle(Point pt1, Point pt2, Point pt3)
        {
            double dx1 = pt1.X - pt2.X;
            double dy1 = pt1.Y - pt2.Y;
            double dx2 = pt3.X - pt2.X;
            double dy2 = pt3.Y - pt2.Y;

            double angle = Math.Atan2(dy1, dx1) - Math.Atan2(dy2, dx2);
            return angle * 180.0 / Math.PI;
        }
    }
}
