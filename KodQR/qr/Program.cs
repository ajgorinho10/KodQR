using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using KodQR.qr;
using KodQR.bar;
using System.Threading;
using System.Threading.Tasks;

public class QRCodeReader
{
    static void Main(string[] args)
    {
        //KODY QR
        //-----------------------------------------------------------
        //string filePath = "kodyQRzdjecia//zyczenia-sensonauka.png";
        //string filePath = "kodyQRzdjecia//qr-code-21x21.png";
        //string filePath = "kodyQRzdjecia//qr7.png";
        //string filePath = "kodyQRzdjecia//qr6.png";
        //string filePath = "kodyQRzdjecia//rq3.png";
        //string filePath = "kodyQRzdjecia//qr-1.png";
        //string filePath = "kodyQRzdjecia//megaqr.png";
        //string filePath = "kodyQRzdjecia//qrmax.png";
        //string filePath = "kodyQRzdjecia//qrkat.png";
        //string filePath = "kodyQRzdjecia//qrmid.png";
        //string filePath = "kodyQRzdjecia//qr1_2.png";
        //string filePath = "kodyQRzdjecia//qrmoj2.jpg";
        //string filePath = "kodyQRzdjecia//qr1_2.png";
        //string filePath = "kodyQRzdjecia//qr1_4.png";
        //string filePath = "kodyQRzdjecia//qrkuba.jpg";
        //string filePath = "kodyQRzdjecia//qrciekawy.png";
        //string filePath = "kodyQRzdjecia//qrehh.jpg";
        //string filePath = "kodyQRzdjecia//test.png";
        //string filePath = "kodyQRzdjecia//test1.png";
        //string filePath = "kodyQRzdjecia//qr_moj.png";
        //string filePath = "kodyQRzdjecia//qr12.jpg";
        //string filePath = "kodyQRzdjecia//qr10.jpg";
        //string filePath = "kodyQRzdjecia//qrdziwne2.png";
        //string filePath = "kodyQRzdjecia//qrnie.png";
        //-----------------------------------------------------------



        //KODY BAR
        //-------------------------------------------------------------------
        //string filePath = "kodyBarZdjecia//bar2_1.jpg"; //-1
        //string filePath = "kodyBarZdjecia//bar2_2.jpg"; //-1
        //string filePath = "kodyBarZdjecia//bar2_3.jpg";
        //string filePath = "kodyBarZdjecia//bar2_4.jpg"; 
        //string filePath = "kodyBarZdjecia//bar2_5.jpg";
        //string filePath = "kodyBarZdjecia//bar2_6.jpg"; 
        //string filePath = "kodyBarZdjecia//bar2_7.jpg";
        //string filePath = "kodyBarZdjecia//bar2_8.jpg";
        //string filePath = "kodyBarZdjecia//bar2_9.jpg"; // -1
        //-------------------------------------------------------------------

        string filePath = "kodyBarZdjecia//qr_bar.jpg";

        DateTime startTime = DateTime.Now;

        Detection(filePath);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;
        Console.WriteLine($"Czas wykonania: {duration.TotalMilliseconds} ms");
    }

    public static void Detection(string filePath)
    {
        void QR()
        {
            Image<Gray, Byte> img = Binarization.Binarize(filePath);
            qrDetection qr = new qrDetection();
            List<Tuple<Punkt, Punkt, Punkt, Punkt, String>> qr_info = qr.qrDetect(img);

            foreach (var q in qr_info)
            {
                if(q.Item5 == "")
                {
                    continue;
                }
                Console.WriteLine("QR:" + q.Item5);
            }
        }

        void BAR()
        {
            Mat image = CvInvoke.Imread(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
            barDetection bar = new barDetection(image.ToImage<Bgr, Byte>());
            List<String> bar_info = bar.detectBAR();

            foreach (var q in bar_info)
            {
                Console.WriteLine("EAN-13:" + q);
            }
        }

        Thread thread_qr = new Thread(new ThreadStart(QR));
        thread_qr.Name = "QR";
        thread_qr.Start();       

        Thread thread_bar = new Thread(new ThreadStart(BAR));
        thread_bar.Name = "BAR";
        thread_bar.Start();

        thread_qr.Join();
        thread_bar.Join();
    }
}