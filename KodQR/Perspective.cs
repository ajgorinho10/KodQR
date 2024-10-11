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
using Accord.Math;


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

        public static double distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public static Point PrzecięcieLin(Point point1_1, Point point1_2, Point point3_1, Point point3_2, Point point2_1)
        {
            if (Math.Abs(point2_1.X - point1_1.X) <= 2.0)
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



            if (Math.Abs(point1_1.X - point1_2.X) <= 1)
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

            double a1 = 0, a2 = 0;
            double x = 0, y = 0;
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

            if (mianownikA1 != 0 && mianownikA2 != 0)
            {
                x = (b2 - b1) / (a1 - a2);
                y = a1 * x + b1;
            }
            else
            {
                x = point1_1.X > point3_1.X ? point1_1.X : point3_1.X;
                y = point1_1.Y > point3_1.Y ? point1_1.Y : point3_1.Y;
            }

            return new Point((int)x, (int)y);
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

            Point p4_new = PrzecięcieLin(point1_1, point1_2, point3_1, point3_2, point2_1);
            Point p4_1 = betterP4(point1_1, point2_1, point3_2, p4_new);
            Point p4_2 = betterP4(point3_1, point2_1, point1_2, p4_new);
            p4_new = PrzecięcieLin(point1_1, p4_2, point3_1, p4_1, point2_1);
            //p4_new = p4_1;

            //point3_1 = betterP4(point2_1, point1_1, p4_new, point3_1);
            //point1_1 = betterP4(point2_1, point3_1, p4_new, point1_1);

            //Point point2_2 = betterP4(point3_1, p4_new, point1_1, point2_1);
            //Point point2_3 = betterP4(point1_1, p4_new, point3_1, point2_1);

            //point2_1 = PrzecięcieLin(point1_1, point2_2, point3_1, point2_3, p4_new);

            ShowPerspective(point1_1, point1_2, point3_1, point3_2, point2_1, p4_new, image, punkt2);


        }

        public void ShowPerspective(Point point1_1, Point point1_2, Point point3_1, Point point3_2, Point point2_1, Point p4, Image<Bgr, Byte> image,Punkt punkt2)
        {
            MCvScalar color = new MCvScalar(255, 0, 255);
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

            MCvScalar color7 = new MCvScalar(255, 0, 255);
            CvInvoke.Circle(
                image,
                new System.Drawing.Point(p4.X, p4.Y),
                5,
                color6,
                -1
            );

            CvInvoke.Line(
                image,
                point1_1,
                new System.Drawing.Point(p4.X, p4.Y),
                color,
                1
            );

            CvInvoke.Line(
                image,
                point3_1,
                new System.Drawing.Point(p4.X , p4.Y),
                color,
                1
            );

            Console.WriteLine($"p1:{point1_1.X},{point1_1.Y} p1_2:{point1_2.X},{point1_2.Y}");
            Console.WriteLine($"p3:{point3_1.X},{point3_1.Y} p3_2:{point3_2.X},{point3_2.Y}");
            Console.WriteLine($"p2:{point2_1.X},{point2_1.Y}");
            Console.WriteLine($"p4:{p4.X},{p4.Y}");
            image.Save("perspektywa.png");
            Process.Start(new ProcessStartInfo("perspektywa.png") { UseShellExecute = true });


            PointF[] srcPoints = new PointF[]
            {
            new PointF(point2_1.X, point2_1.Y),
            new PointF(point1_1.X, point1_1.Y),
            new PointF(p4.X, p4.Y),
            new PointF(point3_1.X, point3_1.Y)
            };

            double dis_X = punkt2.w * 7.0;
            double dis_Y = punkt2.w * 7.0;

            PointF[] dstPoints = new PointF[]
            {
            new PointF(0, 0),
            new PointF((float)dis_X, 0),
            new PointF((float)dis_X, (float)dis_Y),
            new PointF(0, (float)dis_Y)
            };

            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, dstPoints);

            Mat dstImage = new Mat();
            Size newSize = new Size((int)dis_X, (int)dis_Y);

            CvInvoke.WarpPerspective(this.img, dstImage, perspectiveMatrix, newSize, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(0, 0, 0));

            CvInvoke.Imshow("Transformed Image", dstImage);
            CvInvoke.WaitKey(0);
        }

        public Point betterP4(Point p1, Point p2, Point p3,Point p4)
        {
            Point p = new Point(p4.X, p4.Y);
            int[] line1 = Line(p3, p);

            p3 = nowyPunkt(p3, p2, true);
            p = nowyPunkt(p, p1, true);
            int[] line2 = Line(p3, p);

            if (line1.Sum() > 0 && line2.Sum() == 0)
            {
                Console.WriteLine("Opcja A");
                return p;
            }
            else
            {
                if(line1.Sum() > 0 && line2.Sum() > 0)
                {
                    Console.WriteLine("Start B");
                    Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    Console.WriteLine($"p: ({p.X},{p.Y})");
                    //p = nowyPunkt(p, p1, true);
                    Point p_old = new Point();
                    p_old.X = p.X; p_old.Y = p.Y;
                    while (line2[2] > 0 && line2[2] < 5)
                    {
                        p_old.X = p.X; p_old.Y = p.Y;
                        p = nowyPunkt(p, p1, true);
                        line2 = Line(p3, p);
                        if (line2[3] == -1)
                        {
                            break;
                        }
                        Console.Write("B__");
                        Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                        Console.WriteLine($"p: ({p.X},{p.Y})");
                    }
                    Console.WriteLine("Koniec B");
                    return p;
                }
                else if (line1.Sum() == 0 && line2.Sum() == 0)
                {
                    Console.WriteLine("Start C");
                    Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    Point p_old = new Point();
                    p_old.X = p.X; p_old.Y = p.Y;
                    while (line2[2] == 0)
                    {
                        p_old.X = p.X; p_old.Y = p.Y;
                        p = nowyPunkt(p, p1, false);
                        line2 = Line(p3, p);
                        if (line2[3] == -1)
                        {
                            break;
                        }
                        Console.Write("C__");
                        Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    }
                    Console.WriteLine("Koniec C");
                    return p;
                }

                Console.WriteLine("Brak Opcji");
                return p;
            }
        }

        public int[] Line(Point p1 , Point p3)
        {
            int[] black = new int[4];

            double czesc = 0;
            double ilosc = distance(p1,p3);

            foreach (Point p in QuietZone.BresenhamLine(p1, p3, 0.0))
            {
                if (p.X < 0 || p.X >= this.width)
                {
                    black[3] = -1;
                    continue;
                }

                if (p.Y < 0 || p.Y >= this.height)
                {
                    black[3] = -1;
                    continue;
                }

                czesc++;
                if (czesc <= (ilosc / 3.0))
                {
                    if (this.img.Data[p.Y, p.X, 0] == 0)
                    {
                        black[0]++;
                    }
                }
                else if (czesc <= ((ilosc * 2.0) / 3.0))
                {
                    if (this.img.Data[p.Y, p.X, 0] == 0)
                    {
                        black[1]++;
                    }
                }
                else if (czesc <= ilosc)
                {
                    if (this.img.Data[p.Y, p.X, 0] == 0)
                    {
                        black[2]++;
                    }
                }
            
            }

            return black;
        }

        public Point nowyPunkt(Point p1,Point p2,bool od)
        {
            double MW = 2.0;

            double vX = p2.X - p1.X;
            double vY = p2.Y - p1.Y;

            double v_length = Math.Sqrt(vX * vX + vY * vY);

            double v_unitX = vX / v_length;
            double v_unitY = vY / v_length;

            double v_scaledX = v_unitX * MW;
            double v_scaledY = v_unitY * MW;

            Point point_further = new Point();
            if(od == true)
            {
                point_further.X = (int)(p1.X - v_scaledX);
                point_further.Y = (int)(p1.Y - v_scaledY);
            }
            else
            {
                point_further.X = (int)(p1.X + v_scaledX);
                point_further.Y = (int)(p1.Y + v_scaledY);
            }

            return point_further;
        }

    }
}
