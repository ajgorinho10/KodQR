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
using ZXing.PDF417.Internal;
using ImageProcessor.Core.Processors;
using System.Text.RegularExpressions;

namespace KodQR
{

    public class QuietZone
    {
        private Bitmap binaryImage;
        private double GMW;

        public QuietZone(Bitmap binaryImage)
        {
            this.binaryImage = binaryImage;
        }

        // Metoda sprawdzająca cichą strefę
        public bool VerifyQuietZone(RegionDescriptors TP1, RegionDescriptors TP2, RegionDescriptors TP3)
        {
            Point PS = new Point((int)((TP1.Centroid.X + TP3.Centroid.X) / 2.0), (int)((TP1.Centroid.Y + TP3.Centroid.Y) / 2.0));

            Point p1 = Calculate90Point(TP1, PS, TP1.BoundingBox.Height - TP1.BoundingBox.Y);
            Point p2 = Calculate90Point(TP2, PS, TP2.BoundingBox.Height - TP2.BoundingBox.Y);
            Point p3 = Calculate90Point(TP3, PS, TP3.BoundingBox.Height - TP3.BoundingBox.Y);

            this.GMW = (TP1.BoundingBox.Height - TP1.BoundingBox.Y) + (TP2.BoundingBox.Height - TP2.BoundingBox.Y) + (TP3.BoundingBox.Height - TP3.BoundingBox.Y);
            this.GMW = this.GMW / 3.0;

            //Console.WriteLine($"p1: {p1} p2: {p2} p3: {p3} ps: {PS}");
            DrawQuiteZone(p1, p2, p3); 


            return CheckQuietZoneLine(p1, p2) &&
                   CheckQuietZoneLine(p3, p2);
        }

        public void DrawQuiteZone(Point p1, Point p2, Point p3)
        {
            using (Graphics g = Graphics.FromImage(binaryImage))
            {

                g.DrawLine(Pens.Green, (PointF)p1, (PointF)p2);
                g.DrawLine(Pens.Green, (PointF)p2, (PointF)p3);
            }
        }

        public Point Calculate90Point(RegionDescriptors p2, Point ps, int Height)
        {
            Point x1 = p2.Centroid;
            double MW = Math.Sqrt((p2.BoundingBox.X - x1.X) * (p2.BoundingBox.X - x1.X) + (p2.BoundingBox.Y - x1.Y) * (p2.BoundingBox.Y - x1.Y)) + 5;
            //Console.WriteLine($"MW: {MW} Height: {Height}");
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

            Color color = binaryImage.GetPixel(point_further.X, point_further.Y);
            while (color.R == 255)
            {
                MW = MW - 1;
                v_scaledX = v_unitX * MW;
                v_scaledY = v_unitY * MW;
                point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
                point_further = isInBitmap(point_further);
                color = binaryImage.GetPixel(point_further.X, point_further.Y);
            }

            MW = MW + (Height / 7.0);
            v_scaledX = v_unitX * MW;
            v_scaledY = v_unitY * MW;
            point_further = new Point((int)(x1.X - v_scaledX), (int)(x1.Y - v_scaledY));
            point_further = isInBitmap(point_further);
            color = binaryImage.GetPixel(point_further.X, point_further.Y);



            // Wyświetlenie wyniku
            //Console.WriteLine("Punkt dalej od PS: ({0}, {1})", point_further.X, point_further.Y);

            return new Point((int)point_further.X, (int)point_further.Y);
        }

        public Point isInBitmap(Point punkt)
        {
            Point tmp = new Point(punkt.X, punkt.Y);

            if (punkt.X <= 0)
            {
                tmp.X = 1;
            }

            if (punkt.Y <= 0)
            {
                tmp.Y = 1;
            }

            if (punkt.X >= binaryImage.Width)
            {
                tmp.X = binaryImage.Width - 1;
            }

            if (punkt.Y >= binaryImage.Height)
            {
                tmp.Y = binaryImage.Height - 1;
            }

            return tmp;
        }

        private bool CheckQuietZoneLine(Point p1, Point p2)
        {
            //Console.WriteLine("NEXT");
            int black = 0;
            int white = 0;
            foreach (Point point in BresenhamLine(p1, p2,this.GMW))
            {
                //Console.WriteLine($"Point: ({point.X}, {point.Y})");
                if (point.X >= binaryImage.Width - 10 || point.Y >= binaryImage.Height - 10)
                {
                    continue;
                }
                if (point.X < 0 || point.Y < 0)
                {
                    continue;
                }

                Color pixelColor = binaryImage.GetPixel(point.X, point.Y);

                // Debugowanie: wyświetlanie kolorów pikseli
                //Console.WriteLine($"Point: ({point.X}, {point.Y}) - Color: {pixelColor.R}");
                if (pixelColor.R != 255)
                {
                    black++;
                }
                else
                {
                    white++;
                }
            }

            double avg = (double)(white) / (black + white);
            //Console.WriteLine(avg);
            if (avg >= 0.92)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Implementacja algorytmu Bresenhama do przeglądania Pointów na linii
        private static IEnumerable<Point> BresenhamLine(Point p1, Point p2,double GMW)
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
            if (points.Count > (GMW * 2))
            {
                for (int i = (int)GMW; i < points.Count - GMW; i++)
                {
                    yield return points[i];
                }
            }
        }

    }
}
