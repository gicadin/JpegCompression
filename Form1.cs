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

        Bitmap imgBitmap, _rightImgBitmat;
        ImageBlock[,] YBlocks, CbBlocks, CrBlocks; 

        double[] yValues, cbValues, crValues;

        List<int> validationTest = new List<int>();

        int size, imgWidth, imgHeight;

        const int NUM_PIXEL_BLOCK = 64;

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

        double[,] testData =
        {
            { 200, 202, 189, 188, 189, 175, 175, 175 },
            { 200, 203, 198, 188, 189, 182, 178, 175 },
            { 203, 200, 200, 195, 200, 187, 185, 175 },
            { 200, 200, 200, 200, 197, 187, 187, 187 },
            { 200, 205, 200, 200, 195, 188, 187, 175 },
            { 200, 200, 200, 200, 200, 190, 187, 175 },
            { 205, 200, 199, 200, 191, 187, 187, 175 },
            { 210, 200, 200, 200, 188, 185, 187, 186 }
        };

        double[,] testData2 =
        {
            { 70, 70, 100, 70, 87, 87, 150, 187 },
            { 85, 100, 96, 79, 87, 154, 87, 113 },
            { 100, 85, 116, 79, 70, 87, 86, 196 },
            { 136, 69, 87, 200, 79, 71, 117, 96 },
            { 161, 70, 87, 200, 103, 71, 96, 113 },
            { 161, 123, 147, 133, 113, 113, 85, 161 },
            { 146, 147, 175, 100, 103, 103, 163, 187 },
            { 156, 146, 189, 70, 113, 161, 163, 197 }
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
                //dlg.Filter = "bmp files (*.bmp)|*.bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgBitmap = new Bitmap(dlg.FileName);
                    LeftImage_Box.Image = imgBitmap;
                    imgWidth = imgBitmap.Width;
                    imgHeight = imgBitmap.Height;
                }
            }

        }

        private void CompressBtn_Click(object sender, EventArgs e)
        {

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

                    newRed = getRValue(yValue, cb, cr);
                    newGreen = getGValue(yValue, cb, cr);
                    newBlue = getBValue(yValue, cb, cr);

                    oldImage.SetPixel(x, y, Color.FromArgb(newRed, newGreen, newBlue));

                }
            }

            LeftImage_Box.Image = newImage;
            rightImg_box.Image = oldImage;
        }

        private void populateImgBlocks()
        {
            initImgBlocks();

            int rows = (int)Math.Ceiling((double)imgHeight / 8);
            int columns = (int)Math.Ceiling((double)imgWidth / 8);

            int index = 0;
            int x = 0, y = 0, block_x = 0, block_y = 0;
            
            for ( int row = 0; row < imgHeight; row++, x++)
            {
                if ( row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for ( int col = 0; col < imgWidth; col++, y++)
                {
                    if (col % 8 == 0 && col != 0) { 
                        block_y++;
                        y = 0;
                    }

                    if ( block_y == columns - 1 && y > imgWidth - (columns - 1) * 8 )
                        YBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if ( block_x == rows - 1 && x > imgHeight - (rows - 1) * 8 )
                        YBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else 
                        YBlocks[block_x, block_y].imgBlock[x, y] = yValues[index++];
                }
                block_y = 0;
                y = 0;
            }

            rows = (int)Math.Ceiling((double)imgHeight / 16);
            columns = (int)Math.Ceiling((double)imgWidth / 16);

            index = 0;
            x = 0; y = 0; block_x = 0; block_y = 0;

            for (int row = 0; row < imgHeight/2; row++, x++)
            {
                if (row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for (int col = 0; col < imgWidth/2; col++, y++)
                {
                    if (col % 8 == 0 && col != 0)
                    {
                        block_y++;
                        y = 0;
                    }

                    if (block_y == columns - 1 && y > imgWidth / 2 - (columns - 1) * 8 )
                        CbBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if (block_x == rows - 1 && x > imgHeight / 2 - (rows - 1) * 8 )
                        CbBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else
                        CbBlocks[block_x, block_y].imgBlock[x, y] = cbValues[index++];
                }
                block_y = 0;
                y = 0;
            }

            index = 0;
            x = 0; y = 0; block_x = 0; block_y = 0;


            for (int row = 0; row < imgHeight / 2; row++, x++)
            {
                if (row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for (int col = 0; col < imgWidth / 2; col++, y++)
                {
                    if (col % 8 == 0 && col != 0)
                    {
                        block_y++;
                        y = 0;
                    }

                    if (block_y == columns - 1 && y > imgWidth / 2 - (columns - 1) * 8)
                        CrBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if (block_x == rows - 1 && x > imgHeight / 2 - (rows - 1) * 8)
                        CrBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else
                        CrBlocks[block_x, block_y].imgBlock[x, y] = crValues[index++];
                }
                block_y = 0;
                y = 0;
            }

            yValues = null;
            cbValues = null;
            crValues = null;

        }

        // Converts img to YCrCB and also subsamples
        // Places results into 3 arrays
        private void convertToYCbCr()
        {
            size = imgBitmap.Width * imgBitmap.Height;

            yValues = new double[size];
            cbValues = new double[size / 4];    // Subsampling 4:2:0
            crValues = new double[size / 4];    // Subsampling 4:2:0

            for (int y = 0, k = 0, cb_k = 0, cr_k = 0; y < imgBitmap.Height; y++)
            {
                for (int x = 0; x < imgBitmap.Width; x++, k++)
                {

                    int red = imgBitmap.GetPixel(x, y).R;
                    int green = imgBitmap.GetPixel(x, y).G;
                    int blue = imgBitmap.GetPixel(x, y).B;

                    double yValue = 0.299 * red + 0.587 * green + 0.114 * blue;
                    yValues[k] = yValue;

                    if ( (x == 0 || x % 2 == 0) && (y == 0 || y % 2 == 0))
                    {
                        double cb = 128 - (0.168736 * red) - (0.331264 * green) + (0.5 * blue);
                        cbValues[cb_k++] = cb;
                    } else if ((x == 0 || x % 2 == 0) && y % 2 == 1 ){ 
                        double cr = 128 + (0.5 * red) - (0.418688 * green) - (0.081312 * blue);
                        crValues[cr_k++] = cr;
                    }
                }
            }
        }

        private int getRValue(double y, double cb, double cr)
        {
            return (int)Math.Round(y + 1.402 * (cr - 128));
        }

        private int getGValue(double y, double cb, double cr)
        {
            return (int)Math.Round(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128));
        }
        
        private int getBValue(double y, double cb, double cr)
        {
            return (int)Math.Round(y + 1.772 * (cb - 128));
        }

        private void meanReduce(double[,] F)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    F[i, j] -= 128;
        }

        private void meanIncrease(double [,] F)
        {
            for ( int i = 0; i < 8; i++)
                for ( int j = 0; j < 8; j++)
                    F[i, j] += 128;
        }

        // Perform dct on 64 array
        private double[,] dct(double[,] F)
        {
            double[,] newF = new double[8, 8];
            double Cu, Cv;
            double sum = 0;

            for ( int u = 0; u < 8; u++)
            {
                for ( int v = 0; v < 8; v++)
                {
                    if (u == 0)
                        Cu = 1 / Math.Sqrt(2);  // Had it before as Math.Sqrt(2) / 2
                    else
                        Cu = 1;

                    if (v == 0)
                        Cv = 1 / Math.Sqrt(2);
                    else
                        Cv = 1;

                    for ( int x = 0; x < 8; x++)
                    {
                        for ( int y = 0; y < 8; y++)
                        {
                            
                            sum += Math.Cos((2 * x + 1) * u * Math.PI / 16) * Math.Cos((2 * y + 1) * v * Math.PI / 16) * F[x, y];
                        }
                    }
                    
                    newF[u, v] = Cu * Cv * sum / 4;
                    sum = 0;
                }
            }

            return newF;

        }

        // Performs quantization on matrix F using the quantization matrix Q
        // Formula F' = round(F/Q)
        private int[,] quantization(double[,] F, int[] Q)
        {

            int[,] newF = new int[8,8];

            for (int i = 0, k = 0; i < 8; i++)
            {
               for(int j = 0; j < 8; j++, k++)
                {
                    newF[i, j] = (int)Math.Round(F[i,j] / Q[k]);
                }           
            }

            return newF;
        }

        private int[] zigzag(int[,] F)
        {
            int[] newF = new int[64];

            int n = 8;
            int i = 0, j = 0;
            int d = -1; // -1 for top-right move, +1 for bottom-left move
            int start = 0, end = n * n - 1;
            do
            {
                newF[start++] = F[i, j];
                newF[end--] = F[n - i - 1, n - j - 1];

                i += d; j -= d;
                if (i < 0)
                {
                    i++;
                    d = -d; // top reached, reverse
                }
                else if (j < 0)
                {
                    j++;
                    d = -d; // left reached, reverse
                }
            } while (start < end);

            if (start == end)
                newF[start] = F[i, j];

            return newF;

        }

        private int[,] dzigzag(int[] F)
        {
            int[,] newF = new int[8, 8];

            int n = 8;
            int i = 0, j = 0;
            int d = -1;
            int start = 0, end = n * n -1;

            do
            {
                newF[i, j] = F[start++];
                newF[n - i - 1, n - j - 1] = F[end--];

                i += d;
                j -= d;
                if (i < 0)
                {
                    i++;
                    d = -d; // top reached, reverse
                }
                else if (j < 0)
                {
                    j++;
                    d = -d; // left reached, reverse
                }
            } while (start < end);

            return newF;
        }

        private double[,] dquantization(int[,] F, int [] Q)
        {
            double[,] newF = new double[8, 8];

            for ( int i = 0, k = 0; i < 8; i++)
                for ( int j = 0; j < 8; j++, k++)
                    newF[i, j] = F[i, j] * Q[k];

            return newF;
        }

        private double[,] idct(double[,] F)
        {
            double[,] newF = new double[8, 8];
            double sum = 0;
            double Cu, Cv;

            for ( int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for ( int u = 0; u < 8; u++)
                    {
                        for ( int v = 0; v < 8; v++)
                        {
                            if (u == 0)
                                Cu = 1 / Math.Sqrt(2);  // Had it before as Math.Sqrt(2) / 2
                            else
                                Cu = 1;

                            if (v == 0)
                                Cv = 1 / Math.Sqrt(2);
                            else
                                Cv = 1;

                            sum += Cv * Cu * F[u, v] * Math.Cos(u * Math.PI * (2 * x + 1) / 16) * Math.Cos(v * Math.PI * (2 * y + 1) / 16);
                        }
                    }
                    newF[x, y] = (int)Math.Round(sum / 4);
                    sum = 0;
                }
            }

            return newF;
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
                if ( imgBitmap != null )
                    encoder.compressImage(imgBitmap, saveFileDialogue_.FileName);
            }
        }

        private void decompressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Decompress";
            ofd.Filter = "Andrei File|*.andrei";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //decompressImg(ofd.FileName);
                _rightImgBitmat = decoder.decompressImage(ofd.FileName);
                rightImg_box.Image = _rightImgBitmat;
            }
        }

        public int[] runLengthEncoding(int[] F)
        {

            int[] newF = new int[70];   // may crash in very unlikely cases

            bool flag = false;
            int j = 0, counter = 0; // j - index of the new array , counter - counts the length of 0's
            for ( int i = 0; i < F.Length; i++)
            {
                if ( flag == false && F[i] != 0)
                {
                    newF[j++] = F[i];
                    
                } else if ( flag == false && F[i] == 0)
                {
                    newF[j++] = 0;
                    flag = true;
                    counter++;
                    
                } else if ( flag == true && F[i] == 0)
                {
                    counter++;
                } else if ( flag == true && F[i] != 0)
                {
                    flag = false;
                    newF[j++] = counter;
                    newF[j++] = F[i];
                    counter = 0;
                }
            }

            if ( flag == true)
            {
                newF[j++] = counter;
            }
            
            int[] finalF = new int[j];
            Array.Copy(newF, finalF, j);

            return finalF;
        }

        public int[] runLengthDecode(int[] F, int size)
        {
            int[] newF = new int[size];
            bool flag = false;

            for (int i = 0, index = 0; i < F.Length; i++, index++)
            {
                if (!flag)
                {
                    newF[index] = F[i];
                    if (F[i] == 0)
                        flag = true;
                }
                else
                {
                    if (F[i] == 1)
                    {
                        index--;
                    } else
                    {
                        for (int j = 2; j < F[i]; j++, index++)
                        {
                            int tmp = F[i];
                            newF[index] = 0;
                        }
                    }
                    flag = false;
                }
            
            }

            return newF;
        }

        public int validateCompressedSize(int[] F)
        {
            int counter = 0;
            for ( int i = 0; i < F.Length; i++)
            {

                if ( F[i] != 0 )
                {
                    counter++;
                    continue;
                }
                else if ( F[i] == 0 )
                {
                    counter += F[i + 1];
                    i++;
                }
            }

            return counter;
        }

        public int validateCompressionFile(List<int> source)
        {
            int counter = 0;
            int size = source.Count; 
            for ( int i = 0; i < size; i++)
            {

                if (source.ElementAt(i) != 0)
                {
                    counter++;
                    continue;
                }
                else if (source.ElementAt(i) == 0)
                {
                    counter += source.ElementAt(i + 1);
                    i++;
                }
            }
            return counter;
        }

        public void compressImg(String filePath)
        {

            if (LeftImage_Box == null)
                return;

            int[] blockInts;
            sbyte[] blockBytes;
            byte[] equivalentBytes;
                        
            convertToYCbCr();
            populateImgBlocks();

            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            writer.Write(imgWidth);
            writer.Write(imgHeight);

            for (int i = 0; i < YBlocks.GetLength(0); i++)
            {
                for ( int j = 0; j < YBlocks.GetLength(1); j++)
                {
                    meanReduce(YBlocks[i, j].imgBlock);
                    blockInts = runLengthEncoding(zigzag(quantization(dct(YBlocks[i, j].imgBlock), luminanceTable)));

                    blockBytes = new sbyte[blockInts.Length];

                    for ( int k = 0; k < blockInts.Length; k++)
                    {
                        // This is a test to see if any values are greater than 128
                        if (blockInts[k] > 128)
                            blockInts[k] = 128;

                        //validationTest.Add(blockInts[k]);           // to be deleted later
                            
                        blockBytes[k] = Convert.ToSByte(blockInts[k]);
                    }
                    equivalentBytes = (byte[])(object)blockBytes;
                    writer.Write(equivalentBytes, 0, equivalentBytes.Length);

                    //actualSize = validateCompressionFile(validationTest);
                    //supposedSize += 64;

                }
            }

            for ( int i = 0; i < CbBlocks.GetLength(0); i++)
            {
                for ( int j = 0; j < CbBlocks.GetLength(1); j++)
                {
                    meanReduce(CbBlocks[i, j].imgBlock);
                    blockInts = runLengthEncoding(zigzag(quantization(dct(CbBlocks[i, j].imgBlock), chrominanceTable)));

                    blockBytes = new sbyte[blockInts.Length];

                    for (int k = 0; k < blockInts.Length; k++)
                    {
                        // This is a test to see if any values are greater than 128
                        if (blockInts[k] == 128)
                            blockInts[k] = 128;

                        blockBytes[k] = Convert.ToSByte(blockInts[k]);
                    }
                    equivalentBytes = (byte[])(object)blockBytes;
                    writer.Write(equivalentBytes, 0, equivalentBytes.Length);

                }
            }

            for (int i = 0; i < CrBlocks.GetLength(0); i++)
            {
                for (int j = 0; j < CrBlocks.GetLength(1); j++)
                {
                    meanReduce(CrBlocks[i, j].imgBlock);
                    blockInts = runLengthEncoding(zigzag(quantization(dct(CrBlocks[i, j].imgBlock), chrominanceTable)));
                    
                    blockBytes = new sbyte[blockInts.Length];

                    for (int k = 0; k < blockInts.Length; k++)
                    {
                        // This is a test to see if any values are greater than 128
                        if (blockInts[k] > 128)
                            blockInts[k] = 128;

                        validationTest.Add(blockInts[k]);

                        blockBytes[k] = Convert.ToSByte(blockInts[k]);
                    }
                    equivalentBytes = (byte[])(object)blockBytes;
                    writer.Write(equivalentBytes, 0, equivalentBytes.Length);

                }
            }

            writer.Close();
            fileStream.Close();
            
        }

        public void decompressImg(String filePath)
        {
            BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
            imgWidth = reader.ReadInt32();
            imgHeight = reader.ReadInt32();

            long fileSize = new System.IO.FileInfo(filePath).Length;
            long fileInfoSize = fileSize - 8;       // Header size = 8 bytes 

            int blockImgWidth = imgWidth + imgWidth % 8;
            int blockImgHeight = imgHeight + imgHeight % 8;

            int size = blockImgWidth * blockImgHeight;
            int cbSize = size + blockImgWidth * blockImgHeight / 4;

            int[] fileContents = new int[fileInfoSize];

            yValues = new double[size];
            cbValues = new double[size / 4];
            crValues = new double[size / 4];

            for (int i = 0; i < fileInfoSize; i++)
            {
                sbyte tmp = reader.ReadSByte();
                fileContents[i] = Convert.ToInt32(tmp);
            }

            reader.Close();

            int[] fileContentsDecoded = runLengthDecode(fileContents, size + size/2);
            fileContents = null;

            for (int i = 0, j = 0, k = 0; i < fileContentsDecoded.Length; i++)
            {
                if (i < size)
                    yValues[i] = fileContentsDecoded[i];
                else if (i < cbSize)
                    cbValues[j++] = fileContentsDecoded[i];
                else
                    crValues[k++] = fileContentsDecoded[i];
            }

            fileContentsDecoded = null;

            decodeChannels();

            yValues = new double[imgWidth * imgHeight];
            toSingleArray(YBlocks, yValues);
            YBlocks = null;

            cbValues = new double[imgWidth * imgHeight / 4];
            toSingleArrayMod(CbBlocks, cbValues);
            CbBlocks = null;

            crValues = new double[imgWidth * imgHeight / 4];
            toSingleArrayMod(CrBlocks, crValues);
            CrBlocks = null;

            cbValues = interpolateCbValues();
            crValues = interpolateCrValues();

            drawDecompressedImage();

        }

        private void decodeChannels()
        {
            initImgBlocks();

            decodeImgBlock(YBlocks, yValues, luminanceTable);
            yValues = null;

            decodeImgBlock(CbBlocks, cbValues, chrominanceTable);
            cbValues = null;

            decodeImgBlock(CrBlocks, crValues, chrominanceTable);
            crValues = null;

        }

        private int[] arrayCopy(double[] source, int startIndex)
        {
            int[] newArray = new int[64];
            
            for ( int i = 0, index = startIndex; i < 64; i++)
                newArray[i] = (int)source[startIndex++];              
            
            return newArray;
        }

        // Creates empty Image blocks to store image data
        private void initImgBlocks()
        {
            int rows = (int)Math.Ceiling((double)imgHeight / 8);
            int columns = (int)Math.Ceiling((double)imgWidth / 8);

            YBlocks = new ImageBlock[rows, columns];
            // Setting up the YBlocks
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    YBlocks[row, column].imgBlock = new double[8, 8];


            rows = (int)Math.Ceiling((double)imgHeight / 16);
            columns = (int)Math.Ceiling((double)imgWidth / 16);
            
            // Setting up the Cb Blocks
            CbBlocks = new ImageBlock[rows, columns];
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    CbBlocks[row, column].imgBlock = new double[8, 8];

            // Setting up the Cr Blocks
            CrBlocks = new ImageBlock[rows, columns];
            for (int row = 0; row < rows; row++)       
                for (int column = 0; column < columns; column++)            
                    CrBlocks[row, column].imgBlock = new double[8, 8];

        }

        private void decodeImgBlock(ImageBlock[,] imgBlock, double[] values, int[] quantizeTable)
        {
            int index = 0;
            for (int i = 0; i < imgBlock.GetLength(0); i++)
            {
                for (int j = 0; j < imgBlock.GetLength(1); j++)
                {
                    imgBlock[i, j].imgBlock = idct(dquantization(dzigzag(arrayCopy(values, index)), quantizeTable));
                    meanIncrease(imgBlock[i, j].imgBlock);
                    for ( int k = 0; k < 8; k++)
                    {
                        for ( int l = 0; l < 8; l++)
                        {
                            if (imgBlock[i, j].imgBlock[k, l] > 255)
                                imgBlock[i, j].imgBlock[k, l] = 255;
                        }
                    }
                    index += 64;
                }
            }
            return;
        }

        private void drawDecompressedImage()
        {
            _rightImgBitmat = new Bitmap(imgWidth, imgHeight);
            rightImg_box.Image = _rightImgBitmat;

            int red = 0, green = 0, blue = 0;
            double cb = cbValues[0], cr = crValues[0];

            int index = 0, cb_index = 0, cr_index = 0;

            int counterR = 0, counterG = 0, counterB = 0;

            for ( int y = 0; y < imgHeight; y++)
            {

                for ( int x = 0; x < imgWidth; x++, index++)
                {
                    
                    if ((x == 0 || x % 2 == 0) && (y == 0 || y % 2 == 0))
                       cb = cbValues[cb_index++];
                    
                    if ((x != 0 && x % 2 == 1) && (y == 0 || y % 2 == 0))
                        cr = crValues[cr_index++];
                    
                    
                    red = getRValue(yValues[index], cbValues[index], crValues[index]);
                    green = getGValue(yValues[index], cbValues[index], crValues[index]);
                    blue = getBValue(yValues[index], cbValues[index], crValues[index]);

                    int tmp = (int)yValues[index];
                    int tmpCb = (int)cbValues[index];
                    int tmpCr = (int)crValues[index];

                    if (blue < 0) { 
                        blue = 10;
                        counterB++;
                    }
                    if (blue > 255) { 
                        blue = 255;
                        counterB++;
                    }
                    if (red < 0) { 
                        red = 10;
                        //byte rred = imgBitmap.GetPixel(index % imgWidth, index / imgHeight).R;
                        counterR++;
                    }
                    if (red > 255) { 
                        red = 255;
                        //byte rred = imgBitmap.GetPixel(index % imgWidth, index / imgHeight).R;
                        counterR++;
                    }
                    if (green > 255){
                        green = 255;
                        counterG++;
                    }
                        
                    if (green < 0){
                        green = 10;
                        counterG++;
                    }
                        
                    _rightImgBitmat.SetPixel(x, y, Color.FromArgb(red, green, blue));
                    //_rightImgBitmat.SetPixel(x, y, Color.FromArgb(tmp, tmpCb, tmpCr));
                }
            }
        }

        private void toSingleArray(ImageBlock[,] imgBlock, double [] dest)
        {
            int index = 0;
            int img_col = 0, img_row = 0, x = 0, y = 0;
            for ( int i = 0; i < imgHeight; i++)
            {
                if ( x != 0 && x % 8 == 0)
                {
                    x = 0;
                    img_row++;
                }

                for ( int j = 0; j < imgWidth; j++, index++)
                {
                    if ( y != 0 && y % 8 == 0)
                    {
                        y = 0;
                        img_col++;
                    }
                    dest[index] = imgBlock[img_row, img_col].imgBlock[x, y];
                    y++;
                }
                img_col = 0;
                y = 0;
                x++;
            }

        }

        private void toSingleArrayMod(ImageBlock[,] imgBlock, double[] dest)
        {
            int index = 0;
            int img_col = 0, img_row = 0, x = 0, y = 0;
            for (int i = 0; i < imgHeight / 2; i++)
            {
                if (x != 0 && x % 8 == 0)
                {
                    x = 0;
                    img_row++;
                }

                for (int j = 0; j < imgWidth / 2; j++, index++)
                {
                    if (y != 0 && y % 8 == 0)
                    {
                        y = 0;
                        img_col++;
                    }
                    dest[index] = imgBlock[img_row, img_col].imgBlock[x, y];
                    y++;
                }
                img_col = 0;
                y = 0;
                x++;
            }
        }

        private double[] interpolateCbValues()
        {
            int[,] newCbValues = new int[imgWidth, imgHeight];
            int index = 0;

            for ( int x = 0; x < imgHeight; x += 2)
            {
                for ( int y = 0; y < imgWidth; y++)
                {
                    if (y == 0 || y % 2 == 0)
                        newCbValues[x, y] = (int)cbValues[index++];
                    else if ( index == cbValues.Length )
                        newCbValues[x, y] = (int)Math.Round(cbValues[index - 1]);
                    else
                        newCbValues[x, y] = (int)Math.Round((cbValues[index] + cbValues[index - 1]) / 2);
                }
            }

            for ( int x = 1; x < imgHeight; x += 2)
            {
                for ( int y = 0; y < imgWidth; y++)
                {
                    if( x == imgHeight - 1)
                        newCbValues[x, y] = newCbValues[x - 1, y];
                    else
                        newCbValues[x, y] = (newCbValues[x - 1, y] + newCbValues[x + 1, y]) / 2;
                }
            }

            // To single array
            return toSingleArrayFromDouble(newCbValues);
        }
        
        private double[] interpolateCrValues()
        {
            int[,] newCrValues = new int[imgWidth, imgHeight];
            int index = 0;

            for (int x = 1; x < imgHeight; x += 2)
            {
                for (int y = 0; y < imgWidth; y++)
                {
                    if (y == 0 || y % 2 == 0)
                        newCrValues[x, y] = (int)crValues[index++];
                    else if (index == crValues.Length)
                        newCrValues[x, y] = (int)Math.Round(crValues[index - 1]);
                    else
                        newCrValues[x, y] = (int)Math.Round((crValues[index] + crValues[index - 1]) / 2);
                }
            }

            for (int x = 0; x < imgHeight; x += 2)
            {
                for (int y = 0; y < imgWidth; y++)
                {
                    if (x == 0)
                        newCrValues[x, y] = newCrValues[x + 1, y];
                    else
                        newCrValues[x, y] = (newCrValues[x - 1, y] + newCrValues[x + 1, y]) / 2;
                }
            }

            return toSingleArrayFromDouble(newCrValues);
        }
        
        private double[] toSingleArrayFromDouble(int[,] source)
        {
            int size = source.GetLength(0) * source.GetLength(1);
            double[] newArray = new double[size];
            int index = 0;
            for ( int i = 0; i < source.GetLength(0); i++)
            {
                for ( int j = 0; j < source.GetLength(1); j++)
                {
                    newArray[index++] = source[i, j];
                }
            }

            return newArray;
        }

        private void printToFile()
        {
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\cbVal1.txt", cbValues.OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\crVal1.txt", crValues.OfType<double>().Select(o => o.ToString()).ToArray());
        }

        private void printToFile2()
        {
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\cbVal2.txt", cbValues.OfType<double>().Select(o => o.ToString()).ToArray());
            System.IO.File.WriteAllLines(@"C:\Users\Andrei\Desktop\COMP4932\crVal2.txt", crValues.OfType<double>().Select(o => o.ToString()).ToArray());
        }
    }
}
