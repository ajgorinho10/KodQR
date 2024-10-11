using System;
using KodQR;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;

public class QRCodeReader
{

    static void Main(string[] args)
    {
        //string filePath = "zyczenia-sensonauka.png";
        string filePath = "qr-code-21x21.png";
        //string filePath = "C:\\Users\\kowal\\source\\repos\\KodQRBackUp\\KodQR\\bin\\Debug\\net8.0\\qr7.png";
        //string filePath = "qr6.png";
        //string filePath = "rq3.png";
        //string filePath = "qr-1.png";
        //string filePath = "megaqr.png";
        //string filePath = "qrmax.png";
        //string filePath = "qrkat.png";
        //string filePath = "qrmid.png";
        //string filePath = "qr1_2.png";
        //string filePath = "qrmoj2.jpg";
        //string filePath = "qrmoj3.jpg";
        //string filePath = "qr1_2.png";
        //string filePath = "qrciekawy.png";
        //
        //string filePath = "qrehh.jpg";
        //string filePath = "test.png";
        //string filePath = "test1.png";
        //string filePath = "C:\\Users\\kowal\\source\\repos\\KodQRBackUp\\KodQR\\bin\\Debug\\net8.0\\qr_moj.png";
        //string filePath = "C:\\Users\\kowal\\source\\repos\\KodQRBackUp\\KodQR\\bin\\Debug\\net8.0\\qr12.jpg";
        //string filePath = "C:\\Users\\kowal\\source\\repos\\KodQRBackUp\\KodQR\\bin\\Debug\\net8.0\\qr10.jpg";
        //string filePath = "C:\\Users\\kowal\\source\\repos\\KodQRBackUp\\KodQR\\bin\\Debug\\net8.0\\qrdziwne2.png";
        //string outputFilePath = "output.png";

        DateTime startTime = DateTime.Now;


        Image<Gray, Byte> img = Binarization.Binarize(filePath);
        FindPatterns findPatterns = new FindPatterns(img);

        List<Punkt> finderPatterns = findPatterns.FinderPatterns();
        //Console.WriteLine($"Ilosc punktow(bez grupowania): {finderPatterns.Count}");

        List<Tuple<Punkt, Punkt, Punkt>> grouped = Grouping.FindQRCodeCandidates(finderPatterns, img.Cols * 2);
        //Console.WriteLine($"Ilosc Grup(z grupowaniem): {grouped.Count}");

        List<Tuple<Punkt, Punkt, Punkt>> groupedQuiet = quietCheck(grouped,img);
        //Console.WriteLine($"Ilosc QRKodów(z QuietZone): {groupedQuiet.Count}");


        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;
        Console.WriteLine($"Czas wykonania: {duration.TotalMilliseconds} ms");

        //drawInfo(img, groupedQuiet, finderPatterns);
    }

    public static List<Tuple<Punkt, Punkt, Punkt>> quietCheck(List<Tuple<Punkt, Punkt, Punkt>> grouped, Image<Gray, Byte> image)
    {
        ConcurrentBag<Tuple<Punkt, Punkt, Punkt>> final = new ConcurrentBag<Tuple<Punkt, Punkt, Punkt>>();
        ConcurrentBag<Tuple<Point, Point, Point>> list = new ConcurrentBag<Tuple<Point, Point, Point>>();

         Parallel.ForEach(grouped, punkty => {
             QuietZone q = new QuietZone(image);
            if (q.VerifyQuietZone(punkty.Item1, punkty.Item2, punkty.Item3))
            {
                final.Add(punkty);
                list.Add(new Tuple<Point, Point, Point>(q.q1, q.q2, q.q3));

                 Perspective perspective = new Perspective(image);
                 perspective.SetUpPerspective(q.q1, q.q2, q.q3, punkty.Item1, punkty.Item2, punkty.Item3);
             }
             //list.Add(new Tuple<Point, Point, Point>(q.q1, q.q2, q.q3));
         });
        final = Grouping.Filtrowanie(final);
        if (false)
        {
            foreach (var punkty in list)
            {
                //Console.WriteLine($"Quite Zone: ({punkty.Item1.X}; {punkty.Item1.Y}), ({punkty.Item2.X}; {punkty.Item2.Y}), ({punkty.Item3.X}; {punkty.Item3.Y})");
                MCvScalar color = new MCvScalar(0, 255, 0);
                CvInvoke.Line(
                    image,
                    new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                    new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                    color,
                    1
                    );

                CvInvoke.Line(
                    image,
                    new System.Drawing.Point(punkty.Item2.X, punkty.Item2.Y),
                    new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                    color,
                    1
                    );

                CvInvoke.Line(
                    image,
                    new System.Drawing.Point(punkty.Item3.X, punkty.Item3.Y),
                    new System.Drawing.Point(punkty.Item1.X, punkty.Item1.Y),
                    color,
                    1
                    );
                //CvInvoke.Imshow("Obraz z punktem", image);
            }
        }

        return new List<Tuple<Punkt, Punkt, Punkt>>(final);
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
                new System.Drawing.Point(punkty.Item1.X,  punkty.Item1.Y),
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