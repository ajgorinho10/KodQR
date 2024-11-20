using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;  // Dodaj tę przestrzeń nazw
using System.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace KodQR.bar
{
    public class FindBar
    {
        public Image<Gray, Byte> img;

        public FindBar(Image<Gray, Byte> im) { this.img = im; }

        public void find()
        {
            Image<Bgr, Byte> im = this.img.Convert<Bgr,Byte>();
            Mat grayImg = this.img.Mat;

            // 3. Wykryj krawędzie za pomocą algorytmu Canny
            Mat edges = new Mat();
            CvInvoke.Canny(grayImg, edges, 50, 200);

            // 4. Wykryj linie za pomocą transformacji Hougha
            using (Mat lines = new Mat())
            {
                CvInvoke.HoughLines(edges, lines, 1, Math.PI / 180, 100);

                // 5. Iteruj przez wykryte linie i rysuj je na obrazie
                for (int i = 0; i < lines.Rows; i++)
                {
                    float[] lineData = lines.GetData() // Pobiera dane jako tablicę
                        .Cast<float>() // Rzutuje każdy element na `float`
                        .Skip(i * 2) // Pomija pierwsze `i * 2` elementów
                        .Take(2) // Pobiera kolejne 2 elementy
                        .ToArray(); // Konwertuje wynik na tablicę
                    double rho = lineData[0];   // Odległość od środka (r)
                    double theta = lineData[1]; // Kąt w radianach

                    // Przekształć (rho, theta) na punkty na linii
                    double a = Math.Cos(theta);
                    double b = Math.Sin(theta);
                    double x0 = a * rho;
                    double y0 = b * rho;

                    Point pt1 = new Point((int)(x0 + 1000 * (-b)), (int)(y0 + 1000 * a));
                    Point pt2 = new Point((int)(x0 - 1000 * (-b)), (int)(y0 - 1000 * a));

                    // Rysuj linię na obrazie
                    CvInvoke.Line(im, pt1, pt2, new MCvScalar(0, 0, 255), 2);
                }
            }

            // 6. Wyświetl wynik
            CvInvoke.Imshow("Hough Lines", im);
            CvInvoke.WaitKey(0);
        }
    }
}
