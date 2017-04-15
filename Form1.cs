using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assignment2
{
    public struct ImageBlock
    {
        public double[,] imgBlock;
    }

    public partial class Form1 : Form
    {
        Encoder encoder;
        Decoder decoder;

        Bitmap imgBitmap, _rightImgBitmat, _bttmImgBitmap;
        ImageBlock[,] YBlocks, CbBlocks, CrBlocks; 

        double[] yValues, cbValues, crValues;

        public static int[] luminanceTable = {
            16, 11, 10, 16, 24, 40, 51, 61,
            12, 12, 14, 19, 26, 58, 60, 55,
            14, 13, 16, 24, 40, 57, 69, 56,
            14, 17, 22, 29, 51, 87, 80, 62,
            18, 22, 37, 56, 68, 109, 103, 77,
            24, 35, 55, 64, 81, 104, 113, 92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103, 99
        };

        public static int[] chrominanceTable =
        {
            17, 18, 24, 47, 99, 99, 99, 99,
            18, 21, 26, 66, 99, 99, 99, 99,
            24, 26, 56, 99, 99, 99, 99, 99,
            47, 66, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        };

        public Form1()
        {
            InitializeComponent();
            encoder = new Encoder();
            decoder = new Decoder();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgBitmap = new Bitmap(dlg.FileName);
                    //showImgBlock();
                    LeftImage_Box.Image = imgBitmap;
                }
            }

        }

        private void CompressBtn_Click(object sender, EventArgs e)
        {
            /*
           int[,] tmp = new int[8, 8];
           double[,] tmp2 = new double[8,8];
           int[,] tmp3 = new int[8,8];
           int[] tmp4 = new int[64];
           int[,] tmp5 = new int[8, 8];
           double[,] tmp6 = new double[8, 8];
           double[,] tmp7 = new double[8, 8];
           int[] tmp8 = new int[64];
           int[] tmp9;


           //if (imgBitmap != null) { 
           //this.convertToYCbCr();
           //this.populateImgBlocks();

           this.meanReduce(testData2);
           tmp2 = this.dct(testData2);
           tmp3 = quantization(tmp2, luminanceTable);
           tmp4 = zigzag(tmp3);

           tmp5 = dzigzag(tmp4);
           tmp6 = dquantization(tmp5, luminanceTable);
           tmp7 = idct(tmp6);
           meanIncrease(tmp7);
           //tmp7 = zigzag(quantization(dct(testData), luminanceTable));
           //tmp8 = runLengthEncoding(tmp7);

           //tmp9 = runLengthDecode(tmp8, 64);

           //tmp5 = dquantization(tmp4, luminanceTable);
               //tmp6 = idct(tmp5);
               //meanIncrease(tmp6);
               int counter = 0;
               counter++;
           //}
           */
        }

        private void compressPFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _rightImgBitmat = new Bitmap(dlg.FileName);
                    
                }
            }

            SaveFileDialog saveFileDialogue_ = new SaveFileDialog();
            saveFileDialogue_.Title = "Save";
            saveFileDialogue_.Filter = "Andrei pFile|*.pandrei";
            saveFileDialogue_.ShowDialog();

            if (saveFileDialogue_.FileName != "")
            {
                //Console.Text += "The file name is: " + saveFileDialogue_.FileName + System.Environment.NewLine;
                //compressImg(saveFileDialogue_.FileName);
                if (imgBitmap != null)
                {
                    //encoder.compressImage(imgBitmap, saveFileDialogue_.FileName);
                    encoder.compressPFrame(_rightImgBitmat, saveFileDialogue_.FileName);
                    RightImg_Box.Image = _rightImgBitmat;
                    //printToFile();
                }
            }
           
        }

        private void decompressPFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Decompress";
            ofd.Filter = "Andrei pFile|*.pandrei";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _bttmImgBitmap = decoder.decompressPFrame(ofd.FileName);
                BttmImg_Box.Image = _bttmImgBitmap;
                //printToFile2();
            }
        }

        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            Bitmap newImage = new Bitmap(imgBitmap.Width, imgBitmap.Height);
            Bitmap oldImage = new Bitmap(imgBitmap.Width, imgBitmap.Height);

            int newRed, newGreen, newBlue;

            for (int y = 0, k = 0; y < imgBitmap.Height; y++)
            { 
                for (int x = 0; x < imgBitmap.Width; x++, k++)
                {

                    int RED = imgBitmap.GetPixel(x, y).R;
                    int GREEN = imgBitmap.GetPixel(x, y).G;
                    int BLUE = imgBitmap.GetPixel(x, y).B;

                    double yValue = 0.299 * RED + 0.587 * GREEN + 0.114 * BLUE;
                    double cb = 128 - (0.168736 * RED) - (0.331264 * GREEN) + (0.5 * BLUE);
                    double cr = 128 + (0.5 * RED) - (0.418688 * GREEN) - (0.081312 * BLUE);

                    Color newColor = Color.FromArgb((int)yValue, (int)cb, (int)cr);

                    newImage.SetPixel(x, y, newColor);
                    /*
                    newRed = getRValue(yValue, cb, cr);
                    newGreen = getGValue(yValue, cb, cr);
                    newBlue = getBValue(yValue, cb, cr);

                    oldImage.SetPixel(x, y, Color.FromArgb(newRed, newGreen, newBlue));
                    */
                }
            }

            LeftImage_Box.Image = newImage;
            RightImg_Box.Image = oldImage;
        }

        private void compressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialogue_ = new SaveFileDialog();
            saveFileDialogue_.Title = "Save";
            saveFileDialogue_.Filter = "Andrei File|*.andrei";
            saveFileDialogue_.ShowDialog();

            if (saveFileDialogue_.FileName != "")
            {
                //Console.Text += "The file name is: " + saveFileDialogue_.FileName + System.Environment.NewLine;
                //compressImg(saveFileDialogue_.FileName);
                if ( imgBitmap != null) { 
                    encoder.compressImage(imgBitmap, saveFileDialogue_.FileName);
                    //encoder.compressPFrame(imgBitmap, saveFileDialogue_.FileName);
                }
            }
        }

        private void decompressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Decompress";
            ofd.Filter = "Andrei File|*.andrei";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _rightImgBitmat = decoder.decompressImage(ofd.FileName);
                RightImg_Box.Image = _rightImgBitmat;
            }
        }

        private void printToFile()
        {
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\yVal1.txt", encoder.getYValues().OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\cbVal1.txt", encoder.getCbValues().OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\crVal1.txt", encoder.getCrValues().OfType<double>().Select(o => o.ToString()).ToArray());
        }

        private void printToFile2()
        {
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\yVal2.txt", decoder.getYValues().OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\cbVal2.txt", decoder.getCbValues().OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\crVal2.txt", decoder.getCrValues().OfType<double>().Select(o => o.ToString()).ToArray());
        }

        private void showImgBlock()
        {
            if ( imgBitmap != null )
            {
                for ( int i = 0; i < imgBitmap.Height; i++)
                {
                    for ( int j = 0; j < imgBitmap.Width; j+= 8)
                    {
                        imgBitmap.SetPixel(i, j, Color.Red);
                    }
                }

                for (int i = 0; i < imgBitmap.Height; i++)
                {
                    for (int j = 0; j < imgBitmap.Width; j += 8)
                    {
                        imgBitmap.SetPixel(j, i, Color.Red);
                    }
                }
            }
            return;
        }
    }
}
