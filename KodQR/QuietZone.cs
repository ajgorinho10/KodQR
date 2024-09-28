using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;
using static QRCodeReader;
using Accord.Math;
using static FindPatterns;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Collections.Concurrent;

namespace KodQR
{

    public class QuietZone
    {
        private Image<Gray, Byte> binaryImage;
        private double GMW;
        private Point p1;
        public Point q1;
        public Point q2;
        public Point q3;


        public QuietZone(Image<Gray, Byte> binaryImage)
        {
            this.binaryImage = binaryImage;
        }

        // Metoda sprawdzająca cichą strefę
        public bool VerifyQuietZone(Punkt TP1, Punkt TP2, Punkt TP3)
        {
            this.GMW = (TP1.w) + (TP2.w) + (TP3.w);
            this.GMW = (this.GMW / 3.0);
            this.p1 = new System.Drawing.Point(TP1.X,TP1.Y);
            Point PS = new Point((int)((TP1.X + TP3.X) / 2.0), (int)((TP1.Y + TP3.Y) / 2.0));

            Point p1 = Calculate90Point(TP1, PS, TP1.w + 5);
            Point p2 = Calculate90Point(TP2, PS, TP2.w + 5);
            Point p3 = Calculate90Point(TP3, PS, TP3.w + 5);

            this.q1 = p1;
            this.q2 = p2;
            this.q3 = p3;


            //Console.WriteLine($"p1: {p1} p2: {p2} p3: {p3} ps: {PS} GMW:{this.GMW}");


            return CheckQuietZoneLine(p2, p1) &&
                   CheckQuietZoneLine(p2, p3);
        }

        public Point Calculate90Point(Punkt p2, Point ps, double Height)
        {

            Point x1 = new System.Drawing.Point(p2.X, p2.Y);
            double MW = Height / (Math.Sqrt(2.0)) + 3.4;

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

            int color = this.binaryImage.Data[point_further.Y, point_further.X, 0];
            while (color == 255)
            {
                MW = MW - 1;
                v_scaledX = v_unitX * MW;
                v_scaledY = v_unitY * MW;
                point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
                point_further = isInBitmap(point_further);
                color = this.binaryImage.Data[point_further.Y, point_further.X, 0];
            }

            MW = MW + (Height / 7.0);
            v_scaledX = v_unitX * MW;
            v_scaledY = v_unitY * MW;
            point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
            point_further = isInBitmap(point_further);
            color = this.binaryImage.Data[point_further.Y, point_further.X, 0];



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

            if (punkt.X >= binaryImage.Width)
            {
                tmp.X = binaryImage.Width ;
            }

            if (punkt.Y >= binaryImage.Height)
            {
                tmp.Y = binaryImage.Height ;
            }

            return tmp;
        }

        private bool CheckQuietZoneLine(Point p1, Point p2)
        {
            //Console.WriteLine("NEXT");
            int black = 0;
            int white = 0;
            foreach (Point point in BresenhamLine(p1, p2, this.GMW))
            {
                //Console.WriteLine($"Point: ({point.X}, {point.Y})");

                int pixelColor = binaryImage.Data[point.Y, point.X,0];

                // Debugowanie: wyświetlanie kolorów pikseli
                //Console.WriteLine($"Point: ({point.X}, {point.Y}) - Color: {pixelColor.R}");
                if (pixelColor == 0)
                {
                    black++;
                }
                else
                {
                    white++;
                }
            }

            double avg = (double)(white) / (black+white);
            //Console.WriteLine(avg);
            if(avg > 0.98)
            {
                return true;
            }
                return false;

        }

        // Implementacja algorytmu Bresenhama do przeglądania Pointów na linii
        private static IEnumerable<Point> BresenhamLine(Point p1, Point p2, double GMW)
        {
            List<Point> points = new List<Point>();

            int x1 = p1.X, y1 = p1.Y;
            int x2 = p2.X, y2 = p2.Y;

            int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
            int err = dx + dy, e2; // błąd pomiaru

            while (true)
            {
                points.Add(new Point(x1, y1));
                if (x1 == x2 && y1 == y2) break;
                e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x1 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }

            // Pomijanie pierwszego i ostatniego punktu
            if (points.Count > (GMW))
            {
                for (int i = 0; i < points.Count - (int)(GMW); i++)
                {
                    yield return points[i];
                }
            }
        }

    }
}
