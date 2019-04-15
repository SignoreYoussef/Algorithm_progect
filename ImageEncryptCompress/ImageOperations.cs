using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    public class Node
    {
        public int Value = 0;
        public long Frequency = 0;
        public Node Left = null;
        public Node Right = null;

        public Node(int val, int freq)
        {
            this.Value = val;
            this.Frequency = freq;
            this.Left = null;
            this.Right = null;
        }
    }
    public class Priority_Queue
    {
        public static void Min_Heapify(ref int h, List<Node> a, int i)
        {
            int large_value = -1;
            int left = (2 * (i + 1)) - 1;
            int right = (2 * (i + 1));
            if ((left < h) && (a[left].Frequency < a[i].Frequency))
            {
                large_value = left;
            }
            else
            {
                large_value = i;
            }
            if ((right < h) && (a[right].Frequency < a[i].Frequency))
            {
                large_value = right;
            }
            if (large_value != i)
            {
                Node temp = a[i];
                a[i] = a[large_value];
                a[large_value] = temp;
                Min_Heapify(ref h, a, large_value);
            }

        }
        public static void Build_MinHeap(List<Node> a)
        {
            int heap_size = a.Count();
            for (int i = ((a.Count()) / 2); i >= 0; i--)
            {
                Min_Heapify(ref heap_size, a, i);
            }
        }
        public static Node Extract_HeapMin(List<Node> a, ref int h)
        {
            Node Min = a[0];
            a[0] = a[h - 1];
            h = h - 1;
            Min_Heapify(ref h, a, 0);
            return Min;
        }
        public static void Heap_Decrease_key(List<Node> a, int i, Node key)
        {
            a[i] = key;
            while ((i > 0) && (a[i / 2].Frequency > a[i].Frequency))
            {
                Node temp = a[i];
                a[i] = a[i / 2];
                a[i / 2] = temp;
                i = i / 2;
            }
        }
        public static void Min_Heap_Insert(List<Node> a, Node key, ref int h)
        {
            h = h + 1;
            Heap_Decrease_key(a, h - 1, key);
        }
    }

    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }
        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        /// 

        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }
        public static string AlphaNumericPW(string s)
        {
            string res = "";
            int chk = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s.ElementAt(i).Equals('0') || s.ElementAt(i).Equals('1')) chk++;
                res += ((s.ElementAt(i) - 48) % 7).ToString();
            }
            if (chk != s.Length)
            {
                s = "";
                for (int i = 0; i < res.Length; i++)
                {
                    if (i == 0 && res.ElementAt(i) >= '0' && res.ElementAt(i) <= '3') s += "1";
                    if (res.ElementAt(i).Equals('0')) s += "000";
                    else if (res.ElementAt(i).Equals('1')) s += "001";
                    else if (res.ElementAt(i).Equals('2')) s += "010";
                    else if (res.ElementAt(i).Equals('3')) s += "011";
                    else if (res.ElementAt(i) == '4') s += "100";
                    else if (res.ElementAt(i) == '5') s += "101";
                    else if (res.ElementAt(i) == '6') s += "110";
                    else if (res.ElementAt(i) == '7') s += "111";
                }
            }

            while (s.ElementAt(0).Equals('0')) { s = s.Remove(0, 1); }
            while (s.Length > 32)
                /*improve the length and minimize it to 32 bit */
                s = s.Remove(new Random((int)DateTime.Now.Ticks + s.Length).Next(s.Length), 1);
            return s;
        }
        public static long Total_Bits = 0;
        public static float matrix_dimintion = 0;
        public static int red_length = 0;
        public static int green_length = 0;
        public static int blue_length = 0;
        public static string red_string = "";
        public static string green_string = "";
        public static string blue_string = "";

        //public static Dictionary<long, int> temp = new Dictionary<long, int>();

        public static void Huffman_Code(ref RGBPixel[,] ImageMatrix)
        {
            matrix_dimintion = ((GetHeight(ImageMatrix)) * (GetWidth(ImageMatrix))) * 24;

            List<Node> RedL = new List<Node>();
            List<Node> GreenL = new List<Node>();
            List<Node> BlueL = new List<Node>();
            Total_Bits = 0;
            int[] R = new int[256];
            int[] G = new int[256];
            int[] B = new int[256];
            for (int i = 0; i < GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < GetWidth(ImageMatrix); j++)
                {
                    R[Convert.ToInt32(ImageMatrix[i, j].red)] += 1;
                    G[Convert.ToInt32(ImageMatrix[i, j].green)] += 1;
                    B[Convert.ToInt32(ImageMatrix[i, j].blue)] += 1;
                }
            }
            for (int i = 0; i < 256; i++)
            {
                if (R[i] != 0)
                { Node x = new Node(i, R[i]); RedL.Add(x); }
                if (G[i] != 0)
                { Node x = new Node(i, G[i]); GreenL.Add(x); }
                if (B[i] != 0)
                { Node x = new Node(i, B[i]); BlueL.Add(x); }

            }
            red_length = RedL.Count();
            green_length = GreenL.Count();
            blue_length = BlueL.Count();
            FileStream fs = new FileStream("RGB-Tree.txt", FileMode.Truncate);
            StreamWriter sw = new StreamWriter(fs);
            string BinVal = "";
            Node RTRoot = Build_Huffman(RedL);
            Save_Tree(RTRoot, sw, BinVal, ref red_string, ImageMatrix, "red");
            // temp.Clear();
            sw.WriteLine("=============================");
            Node GTRoot = Build_Huffman(GreenL);
            Save_Tree(GTRoot, sw, BinVal, ref green_string, ImageMatrix, "green");
            // temp.Clear();
            sw.WriteLine("=============================");
            Node BTRoot = Build_Huffman(BlueL);
            Save_Tree(BTRoot, sw, BinVal, ref blue_string, ImageMatrix, "blue");
            // temp.Clear();
            long res = (Total_Bits) / 8;
            sw.WriteLine(Convert.ToString(res));
            float ratio = (res * 8) / (ImageOperations.matrix_dimintion) * 100;
            sw.WriteLine(Convert.ToString(ratio) + " % ");
            sw.Close();
            fs.Close();
            MessageBox.Show(" Done ");
            FileStream f = new FileStream("strings.txt", FileMode.Truncate);
            StreamWriter ff = new StreamWriter(f);
            ff.WriteLine(red_string);
            ff.WriteLine(green_string);
            ff.WriteLine(blue_string);
            ff.Close();
            f.Close();
            MessageBox.Show(" strings has been written");
        }
        public static void Get_Password(ref int Seed, int Tape, int n)
        {
            int newBit, TAPEv, MSBv;
            for (int z = 0; z < 8; z++)
            {
                MSBv = Seed;
                MSBv = MSBv & (1 << n);
                MSBv = MSBv >> n;
                TAPEv = Seed;
                TAPEv = TAPEv & (1 << Tape);
                TAPEv = TAPEv >> Tape;
                newBit = MSBv ^ TAPEv;
                Seed = Seed << 1;
                Seed = Seed | newBit;

            }

        }
        public static void Encrypt_Decrypt(ref RGBPixel[,] ImageMatrix, string seed, int tape)
        {
            int se = Convert.ToInt32(seed, 2);
            int sLen = seed.Length - 1;
            for (int i = 0; i < GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < GetWidth(ImageMatrix); j++)
                {
                    //=========Encrypt/Decrypt Red Component=========

                    Get_Password(ref se, tape, sLen);
                    int w = se & 255;
                    ImageMatrix[i, j].red = (byte)((ImageMatrix[i, j].red) ^ w);
                    //=========Encrypt/Decrypt Green Component=========
                    Get_Password(ref se, tape, sLen);
                    w = se & 255;
                    ImageMatrix[i, j].green = (byte)((ImageMatrix[i, j].green) ^ w);
                    //   =========Encrypt/Decrypt Blue Component=========
                    Get_Password(ref se, tape, sLen);
                    w = se & 255;
                    ImageMatrix[i, j].blue = (byte)((ImageMatrix[i, j].blue) ^ w);
                }
            }
        }
        public static Node Build_Huffman(List<Node> Component)
        {
            Priority_Queue.Build_MinHeap(Component);
            int C_Heap = Component.Count();
            int n = Component.Count() - 1;
            for (int i = 0; i < n; i++)
            {
                Node temp = new Node(0, 0);
                Node right = Priority_Queue.Extract_HeapMin(Component, ref C_Heap);
                Node left = Priority_Queue.Extract_HeapMin(Component, ref C_Heap);
                temp.Frequency = (left.Frequency + right.Frequency);
                temp.Left = left;
                temp.Right = right;
                Priority_Queue.Min_Heap_Insert(Component, temp, ref C_Heap);
            }
            return Priority_Queue.Extract_HeapMin(Component, ref C_Heap);
        }

        public static void Save_Tree(Node Root, StreamWriter sw, string BinaryVal, ref string colo, RGBPixel[,] ImageMatrix, string x)
        {
            if (Root.Right != null) Save_Tree(Root.Right, sw, BinaryVal + '1', ref colo, ImageMatrix, x);

            if (Root.Left != null) Save_Tree(Root.Left, sw, BinaryVal + '0', ref colo, ImageMatrix, x);

            if ((Root.Left == null) && (Root.Right == null))
            {

                sw.WriteLine(Convert.ToString(Root.Value) + " " + BinaryVal + " " + Convert.ToString(Root.Frequency) + " " + Convert.ToString((BinaryVal.Length) * (Root.Frequency)));
                Total_Bits += ((BinaryVal.Length) * (Root.Frequency));

            }
            
               
            

        }

    }
}

