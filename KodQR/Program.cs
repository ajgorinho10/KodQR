using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Emgu.CV.Structure;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using ImageProcessor.Core.Processors;
using KodQR;
using Emgu.CV.Stitching;
using static FindPatterns;

public class QRCodeReader
{
    public struct Punkt
    {
        public Punkt(int x, int y, double w,double mw)
        {
            this.X = x;
            this.Y = y;
            this.w = w;
            this.MW = mw;
        }
        public int X;
        public int Y;
        public double w;
        public double MW;

        public override string ToString()
        {
            return $"(X == {X}, Y == {Y} W == {w})";
        }

        public static implicit operator System.Drawing.Point(Punkt p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator System.Drawing.PointF(Punkt p)
        {
            return new PointF(p.X, p.Y);
        }
    }

    static void Main(string[] args)
    {
        //string filePath = "zyczenia-sensonauka.png";
        //string filePath = "qr-code-21x21.png";
        //string filePath = "qr7.png";
        //string filePath = "qr6.png";
        //string filePath = "rq3.png";
        //string filePath = "qr-1.png";
        //string filePath = "megaqr.png";
        //string filePath = "qrmax.png";
        //string filePath = "qrkat.png";
        //string filePath = "qrmid.png";
        //string filePath = "qrtest1.png";
        //string filePath = "qr1_2.png";
        //string filePath = "qr1_3.png";
        string filePath = "qr1_4.png";
        //string filePath = "qr1_5.png";
        //string filePath = "qrtest2.png";
        //string filePath = "qrciekawy.png"; //wazne
        //string filePath = "dziwne.png";
        //string filePath = "test.png";
        //
        //string outputFilePath = "output.png";

        Bitmap binary = Binarization.Binarize(filePath);
        List<RegionDescriptors> finderPatterns = FindPatterns.FindFinderPatterns(binary);
        List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>> Grouped = Grouping.FindQRCodeCandidates(binary,finderPatterns, 0, binary.Width*2, 45,65);
        Console.WriteLine("Grouped Count:"+Grouped.Count);
        Grouped = QRCodeReader.checkGrouped(Grouped, binary);

        QRCodeReader.PrintandDraw(finderPatterns, binary);
        QRCodeReader.PrintandDrawGrouped(Grouped, binary);

        binary.Save("output.png", ImageFormat.Png);

        Process.Start(new ProcessStartInfo("output.png") { UseShellExecute = true });
    }

    static List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>> checkGrouped(List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>> Grouped,Bitmap binary)
    {
        List < Tuple <RegionDescriptors, RegionDescriptors, RegionDescriptors>> validGrouped = new List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>>();
        foreach (var p in Grouped)
        {
            QuietZone verifier = new QuietZone(binary);
            bool quietZoneValid = verifier.VerifyQuietZone(p.Item1, p.Item2, p.Item3);
            //Console.WriteLine($"{quietZoneValid} {timingPatternValid}");
            if(quietZoneValid)
            {
                validGrouped.Add(p);
            }
        }

        return validGrouped;
    }

    static void PrintandDraw(List<RegionDescriptors> finderPatterns,Bitmap binary)
    {
        using (Graphics g = Graphics.FromImage(binary))
        {
            int j = 1;
            foreach (var p in finderPatterns)
            {
                g.DrawRectangle(Pens.Blue, p.BoundingBox.X, p.BoundingBox.Y , p.BoundingBox.Width - p.BoundingBox.X, p.BoundingBox.Height - p.BoundingBox.Y);
                Console.WriteLine($"{j} Finder Pattern at: {p.Centroid.ToString()}");
                j++;
            }
        }
        Console.WriteLine();
    }

    static void PrintandDrawGrouped(List<Tuple<RegionDescriptors, RegionDescriptors, RegionDescriptors>> Grouped,Bitmap binary)
    {
        using (Graphics g = Graphics.FromImage(binary))
        {
            int j = 1;
            foreach (var p in Grouped)
            {
                g.DrawLine(Pens.Red, (PointF)p.Item1.Centroid, (PointF)p.Item2.Centroid);
                g.DrawLine(Pens.Red, (PointF)p.Item1.Centroid, (PointF)p.Item3.Centroid);
                g.DrawLine(Pens.Red, (PointF)p.Item2.Centroid, (PointF)p.Item3.Centroid);
                Console.WriteLine($" Pattern NR:{j}");
                Console.WriteLine($"P NR:{j} Finder Pattern at: {p.Item1.Centroid.ToString()}");
                Console.WriteLine($"P NR:{j} Finder Pattern at: {p.Item2.Centroid.ToString()}");
                Console.WriteLine($"P NR:{j} Finder Pattern at: {p.Item3.Centroid.ToString()}");
                j++;
            }
        }

    }
}