using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using static FindPatterns;

namespace KodQR
{
    public class Perspective
    {
        Image<Gray, Byte> img;
        public Image<Gray, Byte> img_perspective;
        int width;
        int height;

        public Punkt p2;
        public Punkt p1;
        public Punkt p3;

        public PointF[] pointsNew;

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

        public static Point PrzecięcieLin(Point point1_1, Point point1_2, Point point3_1, Point point3_2)
        {
            if(point1_1.X == point1_2.X)
            {
                return new Point(point1_1.X, point3_1.Y);
            }

            if (point1_1.Y == point1_2.Y)
            {
                return new Point(point3_1.X, point1_1.Y);
            }


            double a1 = 0, a2 = 0;
            double x = 0, y = 0;
            double b1 = 0, b2 = 0;
            double mianownikA1 = (double)(point1_1.X - point1_2.X);
            double mianownikA2 = (double)(point3_1.X - point3_2.X);
            double licznikA1 = (double)(point1_1.Y - point1_2.Y);
            double licznikA2 = (double)(point3_1.Y - point3_2.Y);

            if (mianownikA1 == 0)
            {
                a1 = 0;
                b1 = point1_2.Y;
            }
            else
            {
                a1 = (double)(licznikA1) / mianownikA1;
                b1 = point1_2.Y - (a1 * point1_2.X);
            }

            if (mianownikA2 == 0)
            {
                a2 = 0;
                b1 = point1_2.Y;
            }
            else
            {
                a2 = (double)(licznikA2) / mianownikA2;
                b2 = point3_2.Y - (a2 * point3_2.X);
            }

            //Console.WriteLine($"a1: {a1}, a2: {a2}");
            //Console.WriteLine($"b1: {b1}, b2: {b2}");

            if ((mianownikA1 != 0) && (mianownikA2 != 0))
            {
                x = (b2 - b1) / (a1 - a2);
                y = a1 * x + b1;
            }
            else
            {
                x = point1_1.X > point3_1.X ? point1_1.X : point3_1.X;
                y = point1_1.Y > point3_1.Y ? point1_1.Y : point3_1.Y;
            }

            //Console.WriteLine($"Koniec : x:{x} y:{y}");
            return new Point((int)x, (int)y);
        }


        public Point Calculate90Point(Punkt p2, Punkt ps, double Height)
        {

            Point x1 = new System.Drawing.Point(p2.X, p2.Y);
            double MW = Height / (Math.Sqrt(2.0)) - 1.0;

            double vX = ps.X - x1.X;
            double vY = ps.Y - x1.Y;

            double v_length = Math.Sqrt(vX * vX + vY * vY);

            double v_unitX = vX / v_length;
            double v_unitY = vY / v_length;

            double v_scaledX = v_unitX * MW;
            double v_scaledY = v_unitY * MW;

            //Console.WriteLine($"MW: {MW} v_scaledX: {v_scaledX} v_scaledY: {v_scaledY}");

            Point point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
            point_further = isInBitmap(point_further);

            int color = this.img.Data[point_further.Y, point_further.X, 0];
            while (color == 255)
            {
                MW = MW - 1;
                v_scaledX = v_unitX * MW;
                v_scaledY = v_unitY * MW;
                point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
                point_further = isInBitmap(point_further);
                color = this.img.Data[point_further.Y, point_further.X, 0];
            }

            // Wyświetlenie wyniku
            //Console.WriteLine("Punkt dalej od PS: ({0}, {1})", point_further.X, point_further.Y);

            return new Point((int)point_further.X, (int)point_further.Y);
        }

        public Point isInBitmap(Point punkt)
        {
            Point tmp = new Point(punkt.X, punkt.Y);

            if (punkt.X <= 0)
            {
                tmp.X = 0;
            }

            if (punkt.Y <= 0)
            {
                tmp.Y = 0;
            }

            if (punkt.X >= img.Width)
            {
                tmp.X = img.Width - 1;
            }

            if (punkt.Y >= img.Height)
            {
                tmp.Y = img.Height - 1;
            }

            return tmp;
        }

        public Point FloodFill(Punkt punkt, bool black, Point ps)
        {

            Point krawendz = new Point(punkt.X, punkt.Y);
            while (this.img.Data[krawendz.Y, krawendz.X, 0] == 0)
            {
                //Console.WriteLine($"");
                krawendz.Y -= 1;
            }

            while (this.img.Data[krawendz.Y, krawendz.X, 0] == 255)
            {
                krawendz.Y -= 1;
            }
            punkt.X = krawendz.X;
            punkt.Y = krawendz.Y;

            int Width = this.width;
            int Height = this.height;

            Stack<System.Drawing.Point> pixels = new Stack<System.Drawing.Point>();
            Point point = new Point(punkt.X, punkt.Y);

            pixels.Push(new System.Drawing.Point(punkt.X, punkt.Y));
            bool[,] visited = new bool[Height, Width];
            int expexted_color = black ? 0 : 255;

            System.Drawing.Point p1, p2, p3, p4;


            double maxArea = punkt.w * punkt.w;

            while (pixels.Count > 0)
            {
                System.Drawing.Point p = pixels.Pop();

                visited[p.Y, p.X] = true;

                byte color = this.img.Data[p.Y, p.X, 0];

                if (color == 0)
                {
                    double dis1 = distance(p, new Point(ps.X,ps.Y));
                    double dis2 = distance(point, new Point(ps.X, ps.Y));
                    if (dis1 >= dis2 && black == false)
                    {
                        point = p;
                    }

                    if (dis1 <= dis2 && black == true)
                    {
                        point = p;
                    }

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

            pixels.Clear();
            return point;
        }

        public bool checkConditions(System.Drawing.Point p, bool[,] visited)
        {
            if (p.X < 0 || p.X >= this.img || p.Y < 0 || p.Y >= this.img || visited[p.Y, p.X]) { return true; }
            return false;
        }

        public void SetUpPerspective(Point p1, Point p2, Point p3,Punkt punkt1, Punkt punkt2, Punkt punkt3)
        {
            Console.WriteLine("Utworzenie perspektywy");


            this.p1 = new Punkt(punkt1.X, punkt1.Y);
            this.p2 = new Punkt(punkt2.X, punkt2.Y);
            this.p3 = new Punkt(punkt3.X, punkt3.Y);

            Punkt ps = new Punkt();
            ps.X = (int)((punkt1.X + punkt3.X) / 2.0);
            ps.Y = (int)((punkt1.Y + punkt3.Y) / 2.0);


            Point point3_1 = FloodFill(punkt3, true, p3);
            Point point3_2 = new Point();

            Point point2_1 = FloodFill(punkt2, true, p2);

            Point point1_1 = FloodFill(punkt1, true, p1);
            Point point1_2 = new Point();

            //Console.WriteLine($"dlugosc p2w:{punkt2.w}, dlugosc p2w*3.5:{punkt2.w*3.5} dlugosc p2 - p1:{distance(point1_1,point2_1)}");

            Point p4_new = Calculate90Point(ps, punkt2, distance(point1_1, point2_1));
            point3_2 = betterP4(point1_1, point2_1, point3_1, p4_new, true);
            point1_2 = betterP4(point3_1, point2_1, point1_1, p4_new, true);
            p4_new = PrzecięcieLin(point1_1, point1_2, point3_1, point3_2);
            
            Image<Bgr, byte> image = this.img.Convert<Bgr, Byte>();

            //this.p1 = punkt1;
            //this.p2 = punkt2;
            //this.p3 = punkt3;
            ShowPerspective(point1_1, point1_2, point3_1, point3_2, point2_1, p4_new, image, punkt2);
        }

        public void ShowPerspective(Point point1_1, Point point1_2, Point point3_1, Point point3_2, Point point2_1, Point p4, Image<Bgr, Byte> image,Punkt punkt2)
        {
            MCvScalar color = new MCvScalar(255, 0, 255);
            MCvScalar color2 = new MCvScalar(0, 255, 0);
            int grubosc = 1;
            CvInvoke.Circle(
                image,
                new Point(this.p1.X,this.p1.Y),
                grubosc,
                color2,
                -1
            );

            CvInvoke.Circle(
                image,
                new Point(this.p2.X, this.p2.Y),
                grubosc,
                color2,
                -1
            );

            CvInvoke.Circle(
                image,
                new Point(this.p3.X, this.p3.Y),
                grubosc,
                color2,
                -1
            );



            //Console.WriteLine($"p1:{point1_1.X},{point1_1.Y} p1_2:{point1_2.X},{point1_2.Y}");
            //Console.WriteLine($"p3:{point3_1.X},{point3_1.Y} p3_2:{point3_2.X},{point3_2.Y}");
            //Console.WriteLine($"p2:{point2_1.X},{point2_1.Y}");
            //Console.WriteLine($"p4:{p4.X},{p4.Y}");
            //image.Save("perspektywa.png");
            //Process.Start(new ProcessStartInfo("perspektywa.png") { UseShellExecute = true });

            Point tmp = new Point();

            if (point3_1.X < point2_1.X)
            {
                tmp.X = point3_1.X;
                tmp.Y = point3_1.Y;

                point3_1.X = point1_1.X; point3_1.Y = point1_1.Y;

                point1_1 = tmp;
            }


            PointF[] srcPoints = new PointF[]
            {
            new PointF(point2_1.X, point2_1.Y),
            new PointF(point1_1.X, point1_1.Y),
            new PointF(p4.X, p4.Y),
            new PointF(point3_1.X, point3_1.Y)
            };

            double dis_X = distance(point2_1, point3_1);
            double dis_Y = distance(point2_1, point1_1);

            dis_X = dis_X >= dis_Y ? dis_X : dis_Y;
            dis_Y = dis_X;

            PointF[] dstPoints = new PointF[]
            {
            new PointF(0, 0),
            new PointF((float)dis_X, 0),
            new PointF((float)dis_X, (float)dis_Y),
            new PointF(0, (float)dis_Y)
            };

            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, dstPoints);

            Image<Gray, Byte> dstImage = new Image<Gray, Byte>((int)dis_X, (int)dis_Y);
            Size newSize = new Size((int)dis_X, (int)dis_Y);

            CvInvoke.WarpPerspective(this.img, dstImage, perspectiveMatrix, newSize, Inter.Linear, Warp.Default, BorderType.Default, new MCvScalar(0, 0, 0));
            
            this.img_perspective = dstImage;

            PointF[] srcPointsArray = new PointF[] {
                new PointF(this.p1.X, this.p1.Y),
                new PointF(this.p2.X, this.p2.Y),
                new PointF(this.p3.X, this.p3.Y),
            };

            PointF[] dstPointsArray = new PointF[3];

            this.pointsNew = CvInvoke.PerspectiveTransform(srcPointsArray, perspectiveMatrix);

            //CvInvoke.Imshow("Transformed Image", dstImage);
            //CvInvoke.WaitKey(0);
        }

        public Point betterP4(Point p1, Point p2, Point p3,Point p4,bool czy4)
        {
            Point p = new Point(p4.X, p4.Y);
            int[] line1 = Line(p3, p);

            p3 = nowyPunkt(p3, p2, p3,true, 2.0);
            p = nowyPunkt(p3, p2, p,true, 2.0);
            int[] line2 = Line(p3, p);
            //Console.WriteLine($"line1[0]:{line1[0]} line1[1]:{line1[1]} line1[2]:{line1[2]}");
            //Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");

            if ((line1[0] + line1[1] + line1[2] > 0) && (line2[2] + line2[1] + line2[0] == 0))
            {
                //Console.WriteLine("Opcja A");
                return p;
            }
            else
            {
                if ((line1[0] + line1[1] + line1[2] > 0) && (line2[2] > 0 || line2[1] > 0))
                {
                    //Console.WriteLine("Start B");
                    //Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    //Console.WriteLine($"p: ({p.X},{p.Y})");
                    //p = nowyPunkt(p, p1, true);
                    Point p_old = new Point();
                    p_old.X = p.X; p_old.Y = p.Y;
                    double d = distance(p3, p);
                    double ratio = (line2[2]) / (d/3.0);
                    while (ratio > 0.1)
                    {
                        p_old.X = p.X; p_old.Y = p.Y;
                        p = nowyPunkt(p3, p2, p, true, 1.5);
                        line2 = Line(p3, p);
                        if (line2[3] == -1)
                        {
                            //Console.WriteLine($"Poza QR");
                            break;
                        }
                        ratio = (line2[2]) / (d/ 3.0);
                        //Console.Write("B__");
                        //Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                        //Console.WriteLine($"p: ({p.X},{p.Y})");
                    }
                    //Console.WriteLine("Koniec B");
                    if(czy4) return p;
                    else return p_old;
                }
                else if ((line1[0] + line1[1] + line1[2] == 0) && (line2[2] == 0 || line2[1] == 0))
                {
                    //Console.WriteLine("Start C");
                    //Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    Point p_old = new Point();
                    p_old.X = p.X; p_old.Y = p.Y;
                    double d = distance(p3, p);
                    double ratio = (line2[2]) / (d / 3.0);
                    while (ratio < 0.1)
                    {
                        p_old.X = p.X; p_old.Y = p.Y;
                        p = nowyPunkt(p3, p2, p, false,1.3);
                        line2 = Line(p3, p);
                        if (line2[3] == -1)
                        {
                            break;
                        }
                        ratio = (line2[2]) / (d / 3.0);
                        //Console.Write("C__");
                        //Console.WriteLine($"line2[0]:{line2[0]} line2[1]:{line2[1]} line2[2]:{line2[2]}");
                    }
                    //Console.WriteLine("Koniec C");
                    if (czy4) return p;
                    else return p_old;
                }

                //Console.WriteLine("Brak Opcji");
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

        public Point nowyPunkt(Point p1,Point p2,Point p3,bool od,double MW = 2.0)
        {

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
                point_further.X = (int)(p3.X - v_scaledX);
                point_further.Y = (int)(p3.Y - v_scaledY);
            }
            else
            {
                point_further.X = (int)(p3.X + v_scaledX);
                point_further.Y = (int)(p3.Y + v_scaledY);
            }

            return point_further;
        }

    }
}
