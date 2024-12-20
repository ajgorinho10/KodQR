﻿using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using static FindPatterns;
using System.Drawing;
using System.Collections.Concurrent;
using System.Diagnostics;
using KodQR.qr;
using KodQR.bar;

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
        string filePath = "kodyBarZdjecia//bar2_3.jpg";
        //string filePath = "kodyBarZdjecia//bar2_4.jpg"; // -1
        //string filePath = "kodyBarZdjecia//bar2_5.jpg";
        //string filePath = "kodyBarZdjecia//bar2_6.jpg"; //-1
        //string filePath = "kodyBarZdjecia//bar2_7.jpg";
        //string filePath = "kodyBarZdjecia//bar2_8.jpg";
        //string filePath = "kodyBarZdjecia//bar2_9.jpg";
        //-------------------------------------------------------------------

        //Image<Gray, Byte> img = Binarization.Binarize(filePath);

        //qrDetection qr = new qrDetection();
        //qr.qrDetect(img);

        Mat image = CvInvoke.Imread(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        barDetection bar = new barDetection(image.ToImage<Bgr,Byte>());
        bar.detectBAR();
    }
}