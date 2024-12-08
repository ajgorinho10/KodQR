using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodQR.bar
{
    public class Decoding
    {
        public int[] barTab;
        public Image<Gray, Byte> img;
        public int y_f;
        public Decoding(int[] tab, Image<Gray, Byte> im,int y) { this.barTab = tab; this.img = im; this.y_f = y; }

        public void decode()
        {
            
        }
    }
}
