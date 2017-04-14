using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment2
{

    class Encoder
    {
        static int counter = 0;

        private int _imgWidth, _imgHeight;

        private double[] _yValues, _cbValues, _crValues;
        private ImageBlock[,] _yBlocks, _cbBlocks, _crBlocks;

        private double[,] _yIFrame, _cbIFrame, _crIFrame;
        private Point[] _motionVectors;

        private Bitmap _imgBitmap;

        public void compressImage(Bitmap image, String filePath)
        {
            _imgBitmap = image;
            _imgWidth = image.Width;
            _imgHeight = image.Height;

            convertToYCbCr();
            populateIFrame();
            populateImgBlocks();

            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            writer.Write(_imgWidth);
            writer.Write(_imgHeight);

            writeBlocks(writer, _yBlocks, Form1.luminanceTable);
            writeBlocks(writer, _cbBlocks, Form1.chrominanceTable);
            writeBlocks(writer, _crBlocks, Form1.chrominanceTable);

            writer.Close();
            fileStream.Close();

            _yBlocks = null;
            _cbBlocks = null;
            _crBlocks = null;
        }

        public void compressPFrame(Bitmap image, String filePath)
        {
            _imgBitmap = image;
            //_imgWidth = image.Width;
            //_imgHeight = image.Height;

            convertToYCbCr();       // To remove after finishing compression
            //populateIFrame();
            populateImgBlocks();    // To remove later maybe

            findMotionVectors();

            drawMV();

            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            writeMotionVectors(writer);
            writePBlocks(writer, _yIFrame, _yBlocks, Form1.luminanceTable);
            writePBlocks(writer, _cbIFrame, _cbBlocks, Form1.chrominanceTable);
            writePBlocks(writer, _crIFrame, _crBlocks, Form1.chrominanceTable);


            writer.Close();
            fileStream.Close();

        }

        private void writeMotionVectors(BinaryWriter writer)
        {
            sbyte x, y;
            int size = _motionVectors.Length;

            writer.Write(Convert.ToInt32(size));

            for ( int i = 0; i < _motionVectors.Length; i++)
            {
                x = Convert.ToSByte(_motionVectors[i].X);
                y = Convert.ToSByte(_motionVectors[i].Y);

                writer.Write((byte)x);
                writer.Write((byte)y);
            }
        }

        private void writePBlocks(BinaryWriter writer, double[,] iFrame, ImageBlock[,] imgBlock, int[] quantizationTable)
        {
            int[] blockInts;
            sbyte[] blockBytes;
            byte[] equivalentBytes;
            int index = 0;

            for (int i = 0; i < imgBlock.GetLength(0); i++)
            {
                for (int j = 0; j < imgBlock.GetLength(1); j++)
                {

                    if (_motionVectors[index].X == 0 && _motionVectors[index].Y == 0)
                    {
                        blockInts = new int[] { 0, 64 };                      
                    }
                    else
                    {
                        blockInts = runLengthEncoding(zigzag(quantization(dct(imgBlockDifference(imgBlock[i, j], _motionVectors[index], i, j)), quantizationTable)));
                    }
                    index++;
                    blockBytes = new sbyte[blockInts.Length];

                    for (int k = 0; k < blockInts.Length; k++)
                    {
                        // This is a test to see if any values are greater than 128
                        if (blockInts[k] > 127)
                            blockInts[k] = 127;
                        else if (blockInts[k] < -128)
                            blockInts[k] = -128;

                        blockBytes[k] = Convert.ToSByte(blockInts[k]);
                    }
                    equivalentBytes = (byte[])(object)blockBytes;
                    writer.Write(equivalentBytes, 0, equivalentBytes.Length);

                }
            }
        }

        private double[,] imgBlockDifference(ImageBlock imgBlock, Point mv, int x, int y)
        {
            int relativeX = x * 8 + 4;
            int relativeY = y * 8 + 4;

            double[,] newF = new double[8, 8];

            for ( int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    double tmp = _yIFrame[relativeX + mv.X + i, relativeY + mv.Y + j];
                    newF[i, j] = imgBlock.imgBlock[i, j] - _yIFrame[relativeX + mv.X + i, relativeY + mv.Y + j];
                }
            }

            return newF;
        }

        private void drawMV()
        {
            for ( int i = 0; i < _motionVectors.Length; i++)
            {
                if ( _motionVectors[i].X != 0 || _motionVectors[i].Y != 0)
                {
                    int row = i / _yBlocks.GetLength(0); 
                    int col = i % _yBlocks.GetLength(1);

                    if (row == 0 || col == 0)
                        continue;

                    _imgBitmap.SetPixel(8 * col, 8 * row, Color.Red);
                }
            }
        }

        private void findMotionVectors()
        {
            _motionVectors = new Point[_yBlocks.GetLength(0) * _yBlocks.GetLength(1)];

            int index = 0;

            for (int x = 0; x < _yBlocks.GetLength(0); x++)
            {
                for ( int y = 0; y < _yBlocks.GetLength(1); y++)
                {
                    _motionVectors[index++] = sequentialSearch(x, y);
                }
            }

            return;
        }

        private Point sequentialSearch(int x, int y)
        {
            double curMad = 0;
            double min = 99999;
            
            int u = 0, v = 0;
            int p = 8;

            int relativeX = x * 8;
            int relativeY = y * 8;

            for ( int i = -1 * p; i < p; i++)
            {
                for ( int j = -1 * p; j < p; j++)
                {
                    if (relativeX + i < 0)
                        continue;
                    else if (relativeX + i > _imgHeight - 8)
                        continue;
                    else if (relativeY + j < 0)
                        continue;
                    else if (relativeY + j > _imgWidth - 8)
                        continue;

                    curMad = mad(_yBlocks[x, y], relativeX, relativeY, i, j);
                    if ( curMad < min )
                    {
                        min = curMad;
                        u = i;
                        v = j;
                    }
                }
            }

            return new Point(u, v);
        }

        private double mad(ImageBlock imgBlock, int x, int y, int i, int j)
        {
            double difference = 0;

            for (int k = 0; k < 8; k++)
            {
                for (int l = 0; l < 8; l++)
                {
                    difference += imgBlock.imgBlock[k, l] - _yIFrame[x + i + k, y + j + l];
                    counter++;
                }
            }

            return Math.Abs(difference) / 64;
        }

        // Creates a image matric of y values - to be used as an I-Frame by P-Frame for MAD
        private void populateIFrame()
        {
            _yIFrame = new double[(int)Math.Round(_imgWidth / 8.0) * 8, (int)Math.Round(_imgHeight / 8.0) * 8];

            int index = 0;
            for ( int x = 0; x < _imgHeight; x++)
                for ( int y = 0; y < _imgWidth; y++ )
                    _yIFrame[x, y] = _yValues[index++];

            index = 0;
            _cbIFrame = new double[(int)Math.Ceiling(_imgWidth / 16.0), (int)Math.Ceiling(_imgHeight / 16.0)];
            for (int x = 0; x < _cbIFrame.GetLength(0); x++)
                for (int y = 0; y < _cbIFrame.GetLength(1); y++)
                    _cbIFrame[x, y] = _cbValues[index++];

            index = 0;
            _crIFrame = new double[(int)Math.Ceiling(_imgWidth / 16.0), (int)Math.Ceiling(_imgHeight / 16.0)];
            for (int x = 0; x < _crIFrame.GetLength(0); x++)
                for (int y = 0; y < _crIFrame.GetLength(1); y++)
                    _crIFrame[x, y] = _crValues[index++];

        }

        // Converts img to YCrCB and also subsamples
        // Places results into 3 arrays
        private void convertToYCbCr()
        {
            int size = _imgWidth * _imgHeight;

            _yValues = new double[size];
            _cbValues = new double[size / 4];    // Subsampling 4:2:0
            _crValues = new double[size / 4];    // Subsampling 4:2:0

            for (int y = 0, k = 0, cb_k = 0, cr_k = 0; y < _imgHeight; y++)
            {
                for (int x = 0; x < _imgWidth; x++, k++)
                {

                    int red = _imgBitmap.GetPixel(x, y).R;
                    int green = _imgBitmap.GetPixel(x, y).G;
                    int blue = _imgBitmap.GetPixel(x, y).B;

                    double yValue = 0.299 * red + 0.587 * green + 0.114 * blue;
                    _yValues[k] = yValue;

                    if ((x == 0 || x % 2 == 0) && (y == 0 || y % 2 == 0))
                    {
                        double cb = 128 - (0.168736 * red) - (0.331264 * green) + (0.5 * blue);
                        _cbValues[cb_k++] = cb;
                    }
                    else if ((x == 0 || x % 2 == 0) && y % 2 == 1)
                    {
                        double cr = 128 + (0.5 * red) - (0.418688 * green) - (0.081312 * blue);
                        _crValues[cr_k++] = cr;
                    }
                }
            }
        }

        private void populateImgBlocks()
        {
            initImgBlocks();

            populateYBlocks();
            populateCbBlocks();
            populateCrBlocks();

            _yValues = null;
            _cbValues = null;
            _crValues = null;
        }

        private void initImgBlocks()
        {
            int rows = (int)Math.Ceiling((double)_imgHeight / 8);
            int columns = (int)Math.Ceiling((double)_imgWidth / 8);

            _yBlocks = new ImageBlock[rows, columns];
            // Setting up the YBlocks
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    _yBlocks[row, column].imgBlock = new double[8, 8];


            rows = (int)Math.Ceiling((double)_imgHeight / 16);
            columns = (int)Math.Ceiling((double)_imgWidth / 16);

            // Setting up the Cb Blocks
            _cbBlocks = new ImageBlock[rows, columns];
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    _cbBlocks[row, column].imgBlock = new double[8, 8];

            // Setting up the Cr Blocks
            _crBlocks = new ImageBlock[rows, columns];
            for (int row = 0; row < rows; row++)
                for (int column = 0; column < columns; column++)
                    _crBlocks[row, column].imgBlock = new double[8, 8];
        }

        private void populateYBlocks()
        {
            int rows = (int)Math.Ceiling((double)_imgHeight / 8);
            int columns = (int)Math.Ceiling((double)_imgWidth / 8);

            int index = 0;
            int x = 0, y = 0, block_x = 0, block_y = 0;

            for (int row = 0; row < _imgHeight; row++, x++)
            {
                if (row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for (int col = 0; col < _imgWidth; col++, y++)
                {
                    if (col % 8 == 0 && col != 0)
                    {
                        block_y++;
                        y = 0;
                    }

                    if (block_y == columns - 1 && y > _imgWidth - (columns - 1) * 8)
                        _yBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if (block_x == rows - 1 && x > _imgHeight - (rows - 1) * 8)
                        _yBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else
                        _yBlocks[block_x, block_y].imgBlock[x, y] = _yValues[index++];
                }
                block_y = 0;
                y = 0;
            }
        }
        
        private void populateCbBlocks()
        {
            int rows = (int)Math.Ceiling((double)_imgHeight / 16);
            int columns = (int)Math.Ceiling((double)_imgWidth / 16);

            int index = 0;
            int x = 0, y = 0, block_x = 0, block_y = 0;

            for (int row = 0; row < _imgHeight / 2; row++, x++)
            {
                if (row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for (int col = 0; col < _imgWidth / 2; col++, y++)
                {
                    if (col % 8 == 0 && col != 0)
                    {
                        block_y++;
                        y = 0;
                    }

                    if (block_y == columns - 1 && y > _imgWidth / 2 - (columns - 1) * 8)
                        _cbBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if (block_x == rows - 1 && x > _imgHeight / 2 - (rows - 1) * 8)
                        _cbBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else
                        _cbBlocks[block_x, block_y].imgBlock[x, y] = _cbValues[index++];
                }
                block_y = 0;
                y = 0;
            }
        }

        private void populateCrBlocks()
        {
            int rows = (int)Math.Ceiling((double)_imgHeight / 16);
            int columns = (int)Math.Ceiling((double)_imgWidth / 16);

            int index = 0;
            int x = 0, y = 0, block_x = 0, block_y = 0;

            for (int row = 0; row < _imgHeight / 2; row++, x++)
            {
                if (row % 8 == 0 && row != 0)
                {
                    block_x++;
                    x = 0;
                }

                for (int col = 0; col < _imgWidth / 2; col++, y++)
                {
                    if (col % 8 == 0 && col != 0)
                    {
                        block_y++;
                        y = 0;
                    }

                    if (block_y == columns - 1 && y > _imgWidth / 2 - (columns - 1) * 8)
                        _crBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else if (block_x == rows - 1 && x > _imgHeight / 2 - (rows - 1) * 8)
                        _crBlocks[block_x, block_y].imgBlock[x, y] = 0;
                    else
                        _crBlocks[block_x, block_y].imgBlock[x, y] = _crValues[index++];
                }
                block_y = 0;
                y = 0;
            }
        }

        private void writeBlocks(BinaryWriter writer, ImageBlock[,] imgBlock, int[] quantizeTable)
        {
            int[] blockInts;
            sbyte[] blockBytes;
            byte[] equivalentBytes;

            for (int i = 0; i < imgBlock.GetLength(0); i++)
            {
                for (int j = 0; j < imgBlock.GetLength(1); j++)
                {
                    meanReduce(imgBlock[i, j].imgBlock);
                    blockInts = runLengthEncoding(zigzag(quantization(dct(imgBlock[i, j].imgBlock), quantizeTable)));

                    blockBytes = new sbyte[blockInts.Length];

                    for (int k = 0; k < blockInts.Length; k++)
                    {
                        // This is a test to see if any values are greater than 128
                        if (blockInts[k] > 127)
                            blockInts[k] = 127;

                        if (blockInts[k] < -128)
                            blockInts[k] = -128;

                        blockBytes[k] = Convert.ToSByte(blockInts[k]);
                    }
                    equivalentBytes = (byte[])(object)blockBytes;
                    writer.Write(equivalentBytes, 0, equivalentBytes.Length);

                }
            }
        }

        private void meanReduce(double[,] F)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    F[i, j] -= 128;
        }

        private double[,] dct(double[,] F)
        {
            double[,] newF = new double[8, 8];
            double Cu, Cv;
            double sum = 0;

            for (int u = 0; u < 8; u++)
            {
                for (int v = 0; v < 8; v++)
                {
                    if (u == 0)
                        Cu = 1 / Math.Sqrt(2);  // Had it before as Math.Sqrt(2) / 2
                    else
                        Cu = 1;

                    if (v == 0)
                        Cv = 1 / Math.Sqrt(2);
                    else
                        Cv = 1;

                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
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

            int[,] newF = new int[8, 8];

            for (int i = 0, k = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++, k++)
                {
                    newF[i, j] = (int)Math.Round(F[i, j] / Q[k]);
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

        private int[] runLengthEncoding(int[] F)
        {

            int[] newF = new int[128];   // may crash in very unlikely cases

            bool flag = false;
            int j = 0, counter = 0; // j - index of the new array , counter - counts the length of 0's
            for (int i = 0; i < F.Length; i++)
            {
                if (flag == false && F[i] != 0)
                {
                    newF[j++] = F[i];

                }
                else if (flag == false && F[i] == 0)
                {
                    newF[j++] = 0;
                    flag = true;
                    counter++;

                }
                else if (flag == true && F[i] == 0)
                {
                    counter++;
                }
                else if (flag == true && F[i] != 0)
                {
                    flag = false;
                    newF[j++] = counter;
                    newF[j++] = F[i];
                    counter = 0;
                }
            }

            if (flag == true)
            {
                newF[j++] = counter;
            }

            int[] finalF = new int[j];
            Array.Copy(newF, finalF, j);

            return finalF;
        }
    }
}
