using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment2
{
    class Decoder
    {
        private int _imgWidth, _imgHeight;

        private double[] _yValues, _cbValues, _crValues;

        private ImageBlock[,] _yBlocks, _cbBlocks, _crBlocks;

        private Bitmap _imgBitmap;

        public Bitmap decompressImage(String filePath)
        {
            readFileIntoValues(filePath);

            populateImgBlocks();

            _yValues = yBlockToValues();
            _yBlocks = null;

            _cbValues = chBlockToValues(_cbBlocks);
            _cbBlocks = null;

            _crValues = chBlockToValues(_crBlocks);
            _crBlocks = null;

            _cbValues = interpolateCbValues();
            _crValues = interpolateCrValues();

            drawBitmap();

            _yValues = null;
            _cbValues = null;
            _crValues = null;

            return _imgBitmap;
        }

        private void readFileIntoValues(String filePath)
        {
            BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
            _imgWidth = reader.ReadInt32();
            _imgHeight = reader.ReadInt32();

            long fileSize = new System.IO.FileInfo(filePath).Length;
            long fileInfoSize = fileSize - 8;       // File size - Header size (header is 8 bytes)

            int[] fileContents = new int[fileInfoSize];

            for (int i = 0; i < fileInfoSize; i++)
            {
                sbyte tmp = reader.ReadSByte();
                fileContents[i] = Convert.ToInt32(tmp);
            }

            reader.Close();

            int blockImgWidth = (int)Math.Ceiling((double)_imgWidth / 8);
            int blockImgHeight = (int)Math.Ceiling((double)_imgHeight / 8);
            int ySize = blockImgWidth * blockImgHeight * 64;

            blockImgWidth = (int)Math.Ceiling((double)_imgWidth / 16);
            blockImgHeight = (int)Math.Ceiling((double)_imgHeight / 16);

            int cbSize = blockImgWidth * blockImgHeight * 64;

            int[] fileContentsDecoded = runLengthDecode(fileContents, ySize + cbSize * 2);
            fileContents = null;

            _yValues = new double[ySize];
            _cbValues = new double[cbSize];
            _crValues = new double[cbSize];

            for (int i = 0, j = 0, k = 0; i < fileContentsDecoded.Length; i++)
            {
                if (i < ySize)
                    _yValues[i] = fileContentsDecoded[i];
                else if (i < ySize + cbSize)
                    _cbValues[j++] = fileContentsDecoded[i];
                else
                    _crValues[k++] = fileContentsDecoded[i];
            }

            fileContentsDecoded = null;
        }

        private void populateImgBlocks()
        {
            initImgBlocks();

            decodeImgBlock(_yBlocks, _yValues, Form1.luminanceTable);
            _yValues = null;

            decodeImgBlock(_cbBlocks, _cbValues, Form1.chrominanceTable);
            _cbValues = null;

            decodeImgBlock(_crBlocks, _crValues, Form1.chrominanceTable);
            _crValues = null;
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
                    for (int k = 0; k < 8; k++)
                    {
                        for (int l = 0; l < 8; l++)
                        {
                            if (imgBlock[i, j].imgBlock[k, l] > 255)
                                imgBlock[i, j].imgBlock[k, l] = 255;
                        }
                    }
                    index += 64;
                }
            }
        }

        private double[,] idct(double[,] F)
        {
            double[,] newF = new double[8, 8];
            double sum = 0;
            double Cu, Cv;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
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

                            sum += Cv * Cu * F[u, v] * Math.Cos(u * Math.PI * (2 * x + 1) / 16) * Math.Cos(v * Math.PI * (2 * y + 1) / 16);
                        }
                    }
                    newF[x, y] = (int)Math.Round(sum / 4);
                    sum = 0;
                }
            }

            return newF;
        }

        private double[,] dquantization(int[,] F, int[] Q)
        {
            double[,] newF = new double[8, 8];

            for (int i = 0, k = 0; i < 8; i++)
                for (int j = 0; j < 8; j++, k++)
                    newF[i, j] = F[i, j] * Q[k];

            return newF;
        }

        private int[,] dzigzag(int[] F)
        {
            int[,] newF = new int[8, 8];

            int n = 8;
            int i = 0, j = 0;
            int d = -1;
            int start = 0, end = n * n - 1;

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

        private int[] runLengthDecode(int[] F, int size)
        {
            if ( size != validateCompressedSize(F))
            {
                throw new Exception("Please use correct img");
            }

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
                    }
                    else
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

        private void meanIncrease(double[,] F)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    F[i, j] += 128;
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

        private double[] yBlockToValues()
        {
            double[] dest = new double[_imgWidth * _imgHeight];

            int index = 0;
            int img_col = 0, img_row = 0, x = 0, y = 0;
            for (int i = 0; i < _imgHeight; i++)
            {
                if (x != 0 && x % 8 == 0)
                {
                    x = 0;
                    img_row++;
                }

                for (int j = 0; j < _imgWidth; j++, index++)
                {
                    if (y != 0 && y % 8 == 0)
                    {
                        y = 0;
                        img_col++;
                    }
                    dest[index] = _yBlocks[img_row, img_col].imgBlock[x, y];
                    y++;
                }
                img_col = 0;
                y = 0;
                x++;
            }

            return dest;
        }

        private double[] chBlockToValues(ImageBlock[,] imgBlock)
        {
            double[] dest = new double[_imgWidth * _imgHeight / 4];

            int index = 0;
            int img_col = 0, img_row = 0, x = 0, y = 0;

            for (int row = 0; row < _imgHeight / 2; row++)
            {
                if (x != 0 && x % 8 == 0)
                {
                    x = 0;
                    img_row++;
                }

                for (int col = 0; col < _imgWidth / 2; col++, index++)
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

            return dest;
        }

        private double[] interpolateCbValues()
        {
            int[,] newCbValues = new int[_imgHeight, _imgWidth];
            int index = 0;

            for (int x = 0; x < _imgHeight; x += 2)
            {
                for (int y = 0; y < _imgWidth; y++)
                {
                    if (y == 0 || y % 2 == 0)
                        newCbValues[x, y] = (int)_cbValues[index++];
                    else if (index == _cbValues.Length)
                        newCbValues[x, y] = (int)Math.Round(_cbValues[index - 1]);
                    else
                        newCbValues[x, y] = (int)Math.Round((_cbValues[index] + _cbValues[index - 1]) / 2);
                }
            }

            for (int x = 1; x < _imgHeight; x += 2)
            {
                for (int y = 0; y < _imgWidth; y++)
                {
                    if (x == _imgHeight - 1)
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
            int[,] newCrValues = new int[_imgHeight, _imgWidth];
            int index = 0;

            for (int x = 1; x < _imgHeight; x += 2)
            {
                for (int y = 0; y < _imgWidth; y++)
                {
                    if (y == 0 || y % 2 == 0)
                        newCrValues[x, y] = (int)_crValues[index++];
                    else if (index == _crValues.Length)
                        newCrValues[x, y] = (int)Math.Round(_crValues[index - 1]);
                    else
                        newCrValues[x, y] = (int)Math.Round((_crValues[index] + _crValues[index - 1]) / 2);
                }
            }

            for (int x = 0; x < _imgHeight; x += 2)
            {
                for (int y = 0; y < _imgWidth; y++)
                {
                    if (x == 0)
                        newCrValues[x, y] = newCrValues[x + 1, y];
                    else
                        newCrValues[x, y] = (newCrValues[x - 1, y] + newCrValues[x + 1, y]) / 2;
                }
            }

            return toSingleArrayFromDouble(newCrValues);
        }

        private void drawBitmap()
        {
            _imgBitmap = new Bitmap(_imgWidth, _imgHeight);

            int red = 0, green = 0, blue = 0;
            int index = 0;

            int counterR = 0, counterG = 0, counterB = 0;

            for (int y = 0; y < _imgHeight; y++)
            {

                for (int x = 0; x < _imgWidth; x++, index++)
                {

                    red = getRValue(_yValues[index], _cbValues[index], _crValues[index]);
                    green = getGValue(_yValues[index], _cbValues[index], _crValues[index]);
                    blue = getBValue(_yValues[index], _cbValues[index], _crValues[index]);

                    if (blue < 0)
                    {
                        blue = 10;
                        counterB++;
                    } else if (blue > 255)
                    {
                        blue = 255;
                        counterB++;
                    }

                    if (red < 0)
                    {
                        red = 10;
                        //byte rred = imgBitmap.GetPixel(index % imgWidth, index / imgHeight).R;
                        counterR++;
                    } else if (red > 255)
                    {
                        red = 255;
                        //byte rred = imgBitmap.GetPixel(index % imgWidth, index / imgHeight).R;
                        counterR++;
                    }

                    if (green > 255)
                    {
                        green = 255;
                        counterG++;
                    } else if (green < 0)
                    {
                        green = 10;
                        counterG++;
                    }

                    _imgBitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));
                }
            }
        }

        private double[] toSingleArrayFromDouble(int[,] source)
        {
            int size = source.GetLength(0) * source.GetLength(1);
            double[] newArray = new double[size];
            int index = 0;
            for (int i = 0; i < source.GetLength(0); i++)
            {
                for (int j = 0; j < source.GetLength(1); j++)
                {
                    newArray[index++] = source[i, j];
                }
            }

            return newArray;
        }

        private int[] arrayCopy(double[] source, int startIndex)
        {
            int[] newArray = new int[64];

            for (int i = 0, index = startIndex; i < 64; i++)
                newArray[i] = (int)source[startIndex++];

            return newArray;
        }

        private int validateCompressedSize(int[] F)
        {
            int counter = 0;
            for (int i = 0; i < F.Length; i++)
            {

                if (F[i] != 0)
                {
                    counter++;
                    continue;
                }
                else if (F[i] == 0)
                {
                    if (F[i + 1] > 64)
                        throw new Exception("Something went wrong in the compression");
                    counter += F[i + 1];
                    i++;
                }
            }

            return counter;
        }
    }
}
