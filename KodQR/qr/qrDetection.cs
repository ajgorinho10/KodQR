using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using KodQR.qr;

namespace KodQR.qr
{
    public class qrDetection
    {

        public List<Tuple<Punkt, Punkt, Punkt, Punkt, String>> qrDetect(Image<Gray, Byte> img)
        {
            //DateTime startTime = DateTime.Now;
            FindPatterns findPatterns = new FindPatterns(img);

            List<Punkt> finderPatterns = findPatterns.FinderPatterns();
            //Console.WriteLine($"Ilosc punktow(bez grupowania): {finderPatterns.Count}");

            List<Tuple<Punkt, Punkt, Punkt>> grouped = Grouping.FindQRCodeCandidates(finderPatterns, img.Cols * 2);
            //Console.WriteLine($"Ilosc Grup(z grupowaniem): {grouped.Count}");

            List<Tuple<Punkt, Punkt, Punkt, Punkt, String>> groupedQuiet = quietCheck(grouped, img);
            //Console.WriteLine($"Ilosc QRKodów(z QuietZone): {groupedQuiet.Count}");


            //DateTime endTime = DateTime.Now;
            //TimeSpan duration = endTime - startTime;
            //Console.WriteLine($"Czas wykonania: {duration.TotalMilliseconds} ms");

            //drawInfo(img, groupedQuiet, finderPatterns);
            return groupedQuiet;
        }

        public static List<Tuple<Punkt, Punkt, Punkt, Punkt, String>> quietCheck(List<Tuple<Punkt, Punkt, Punkt>> grouped, Image<Gray, Byte> image)
        {
            ConcurrentBag<Tuple<Punkt, Punkt, Punkt, Punkt, String>> final = new ConcurrentBag<Tuple<Punkt, Punkt, Punkt, Punkt, String>>();
            int ilosc = 0;
            int sukces = 0;

            foreach (var punkty in grouped)
            {
                QuietZone q = new QuietZone(image);
                if (q.VerifyQuietZone(punkty.Item1, punkty.Item2, punkty.Item3))
                {

                    Perspective perspective = new Perspective(image.Copy());
                    perspective.SetUpPerspective(q.q1, q.q2, q.q3, punkty.Item1, punkty.Item2, punkty.Item3);
                    Decode dec = new Decode(perspective.img_perspective, perspective.pointsNew);
                    dec.fromImgToArray();

                    if (dec.DecodedText.Status == true)
                    {
                        sukces += 1;
                    }
                    //Console.WriteLine($"Decoded Text:{dec.DecodedText.Text}");
                    Tuple<Punkt, Punkt, Punkt, Punkt, String> f = new Tuple<Punkt, Punkt, Punkt, Punkt, String>(perspective.p1, perspective.p2, perspective.p3, perspective.p4, dec.DecodedText.Text);
                    final.Add(f);
                    ilosc += 1;
                }
            };


            if (true)
            {
                Image<Bgr, Byte> im = image.Convert<Bgr, Byte>();
                foreach (var punkty in final)
                {
                    MCvScalar color = new MCvScalar(0, 255, 0);
                    CvInvoke.Line(
                        im,
                        new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                        new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                        color,
                        1
                        );

                    CvInvoke.Line(
                        im,
                        new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                        new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                        color,
                        1
                        );

                    CvInvoke.Line(
                        im,
                        new System.Drawing.Point(punkty.Item4.X, punkty.Item4.Y),
                        new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                        color,
                        1
                        );

                    CvInvoke.Line(
                        im,
                        new System.Drawing.Point(punkty.Item4.X, punkty.Item4.Y),
                        new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                        color,
                        1
                        );

                    CvInvoke.PutText(im, punkty.Item5, new Point(punkty.Item2.X, (int)((punkty.Item2.Y + punkty.Item1.Y) / 2.0)), FontFace.HersheyTriplex, 0.5, color);
                    //CvInvoke.Imshow("Obraz z punktem", image);
                }
                //CvInvoke.Imshow("Obraz z punktem", im);
                //CvInvoke.WaitKey(0);
                //Console.WriteLine($"ilosc:{ilosc} sukces:{sukces}");
                double sprawnosc = ((double)sukces / (double)ilosc) * 100;
                //Console.WriteLine($"Sprawnosc:{sprawnosc}%");
                //im.Save("perspektywa.png");
                //Process.Start(new ProcessStartInfo("perspektywa.png") { UseShellExecute = true });
            }

            return new List<Tuple<Punkt, Punkt, Punkt, Punkt, String>>(final);
        }

        public static void drawInfo(Image<Gray, Byte> image, List<Tuple<Punkt, Punkt, Punkt>> grouped, List<Punkt> finderPatterns)
        {
            Image<Bgr, Byte> img = image.Convert<Bgr, Byte>();
            DrawPatterns(img, finderPatterns);
            DrawGroupedPatterns(img, grouped);

            img.Save("output.png");
            Process.Start(new ProcessStartInfo("output.png") { UseShellExecute = true });
        }

        public static void DrawPatterns(Image<Bgr, Byte> img, List<Punkt> patterns)
        {
            MCvScalar color = new MCvScalar(255, 0, 255);
            foreach (Punkt punkt in patterns)
            {
                //Console.WriteLine($"Pattern: {punkt.X}, {punkt.Y}");
                CvInvoke.Circle(
                img,
                new System.Drawing.Point(punkt.X, punkt.Y),
                5,
                color,
                -1
                );
            }
        }

        public static void DrawGroupedPatterns(Image<Bgr, Byte> img, List<Tuple<Punkt, Punkt, Punkt>> grouped)
        {
            MCvScalar color = new MCvScalar(0, 0, 255);
            foreach (var punkty in grouped)
            {
                Console.WriteLine($"NEXT GROUP:");
                Console.WriteLine($"Pattern 1: {punkty.Item1.X}, {punkty.Item1.Y}");
                Console.WriteLine($"Pattern 2: {punkty.Item2.X}, {punkty.Item2.Y}");
                Console.WriteLine($"Pattern 3: {punkty.Item3.X}, {punkty.Item3.Y}");

                CvInvoke.Line(
                    img,
                    new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                    new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                    color,
                    1
                    );

                CvInvoke.Line(
                    img,
                    new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                    new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                    color,
                    1
                    );

                CvInvoke.Line(
                    img,
                    new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                    new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                    color,
                    1
                    );
            }
        }
    }
}
