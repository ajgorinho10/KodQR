using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FindPatterns;

namespace KodQR.qr
{
    public class Grouping
    {
        public static List<Tuple<Punkt, Punkt, Punkt>> FindQRCodeCandidates(List<Punkt> patterns, double maxQRCodeSize)
        {
            var qrCodeCandidates = new ConcurrentBag<Tuple<Punkt, Punkt, Punkt>>();
            double minQRCodeSize = 0;
            double sizeTolerance = 150;
            double hypotenuseTolerance = 60;

            Parallel.For(0, patterns.Count, i =>
            {
                for (int j = i + 1; j < patterns.Count; j++)
                {
                    for (int k = j + 1; k < patterns.Count; k++)
                    {
                        double bok1 = ObliczOdleglosc(patterns[i], patterns[j]);
                        double bok2 = ObliczOdleglosc(patterns[i], patterns[k]);
                        double bok3 = ObliczOdleglosc(patterns[j], patterns[k]);

                        if (bok1 > bok2 && bok1 > bok3)
                        {
                            double hypotenuse = Math.Sqrt(Math.Pow(bok3, 2) + Math.Pow(bok2, 2));
                            minQRCodeSize = patterns[k].w / 7.0 * 15.0;
                            bool isQRCode = bok1 >= minQRCodeSize && bok1 <= maxQRCodeSize && Math.Abs(hypotenuse - bok1) <= hypotenuseTolerance && Math.Abs(bok3 - bok2) <= sizeTolerance;
                            if (isQRCode) qrCodeCandidates.Add(new Tuple<Punkt, Punkt, Punkt>(patterns[j], patterns[k], patterns[i]));
                        }
                        else if (bok2 > bok1 && bok2 > bok3)
                        {
                            double hypotenuse = Math.Sqrt(Math.Pow(bok3, 2) + Math.Pow(bok1, 2));
                            minQRCodeSize = patterns[j].w / 7.0 * 15.0;
                            bool isQRCode = bok2 >= minQRCodeSize && bok2 <= maxQRCodeSize && Math.Abs(hypotenuse - bok2) <= hypotenuseTolerance && Math.Abs(bok3 - bok1) <= sizeTolerance;
                            if (isQRCode) qrCodeCandidates.Add(new Tuple<Punkt, Punkt, Punkt>(patterns[i], patterns[j], patterns[k]));
                        }
                        else if (bok3 > bok1 && bok3 > bok2)
                        {
                            double hypotenuse = Math.Sqrt(Math.Pow(bok2, 2) + Math.Pow(bok1, 2));
                            minQRCodeSize = patterns[i].w / 7.0 * 15.0;
                            bool isQRCode = bok3 >= minQRCodeSize && bok3 <= maxQRCodeSize && Math.Abs(hypotenuse - bok3) <= hypotenuseTolerance && Math.Abs(bok2 - bok1) <= sizeTolerance;
                            if (isQRCode) qrCodeCandidates.Add(new Tuple<Punkt, Punkt, Punkt>(patterns[j], patterns[i], patterns[k]));
                        }
                    }
                }
            });

            return new List<Tuple<Punkt, Punkt, Punkt>>(qrCodeCandidates);
        }

        public static double ObliczOdleglosc(Punkt p1, Punkt p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static ConcurrentBag<Tuple<Punkt, Punkt, Punkt>> Filtrowanie(ConcurrentBag<Tuple<Punkt, Punkt, Punkt>> qrCodeCandidates)
        {
            List<Tuple<Punkt, Punkt, Punkt>> validGrouped = new List<Tuple<Punkt, Punkt, Punkt>>();
            foreach (var p in qrCodeCandidates)
            {
                var x = p.Item2;
                var y = p.Item3;
                var istniejący = validGrouped.Find(g => g.Item2.X == x.X && g.Item2.Y == x.Y);
                if (istniejący == null)
                {
                    validGrouped.Add(p);
                }
            }
            return new ConcurrentBag<Tuple<Punkt, Punkt, Punkt>>(validGrouped); //validGrouped;
        }
    }
}
