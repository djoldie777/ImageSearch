using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

namespace image_search
{
    public enum ORIENTATION { NORTH, NORTH_EAST, EAST, SOUTH_EAST, SOUTH, SOUTH_WEST, WEST, NORTH_WEST, NONE };

    public partial class Form1 : Form
    {
        private Bitmap bm;
        Color col;
        private const int hystSize = 8;
        private const double distThreshold = 0.2;
        List<Bitmap> images;
        List<Bitmap> listOfResults;

        public Form1()
        {
            InitializeComponent();
            bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            getBitmapList();
            clear();
            col = Color.Black;
            transformToolStripMenuItem.Enabled = false;
            goToolStripMenuItem.Enabled = false;
        }

        private void getBitmapList()
        {
            string[] files = System.IO.Directory.GetFiles(Application.StartupPath + "\\Images");
            images = new List<Bitmap>();

            for (int i = 0; i < files.Length; i++)
            {
                Bitmap btm = Bitmap.FromFile(files[i]) as Bitmap;

                if (btm.Width > pictureBox1.Width || btm.Height > pictureBox1.Height)
                {
                    Bitmap tmpBtm = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                    using (Graphics g = Graphics.FromImage(tmpBtm))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(btm, 0, 0, pictureBox1.Width, pictureBox1.Height);
                    }

                    images.Add(tmpBtm);
                }
                else
                    images.Add(btm);
            }
        }

        private void clear()
        {
            listBox1.Items.Clear();

            using (Graphics g = Graphics.FromImage(bm))
            {
                g.Clear(Color.White);
            }

            pictureBox1.Image = bm;

            transformToolStripMenuItem.Enabled = false;
            goToolStripMenuItem.Enabled = false;
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void drawTriangle()
        {
            using (Graphics g = Graphics.FromImage(bm))
            {
                System.Drawing.Point point1 = new System.Drawing.Point(200, 330);
                System.Drawing.Point point2 = new System.Drawing.Point(400, 330);
                System.Drawing.Point point3 = new System.Drawing.Point(300, 100);

                System.Drawing.Point[] points = { point1, point2, point3 };

                g.FillPolygon(new SolidBrush(col), points);
            }

            pictureBox1.Refresh();
        }

        private void drawRectangle()
        {
            using (Graphics g = Graphics.FromImage(bm))
            {
                System.Drawing.Point point1 = new System.Drawing.Point(200, 330);
                System.Drawing.Point point2 = new System.Drawing.Point(400, 330);
                System.Drawing.Point point3 = new System.Drawing.Point(400, 100);
                System.Drawing.Point point4 = new System.Drawing.Point(200, 100);

                System.Drawing.Point[] points = { point1, point2, point3, point4 };

                g.FillPolygon(new SolidBrush(col), points);
            }

            pictureBox1.Refresh();
        }

        private void drawRhomb()
        {
            using (Graphics g = Graphics.FromImage(bm))
            {
                System.Drawing.Point point1 = new System.Drawing.Point(200, 225);
                System.Drawing.Point point2 = new System.Drawing.Point(300, 330);
                System.Drawing.Point point3 = new System.Drawing.Point(400, 225);
                System.Drawing.Point point4 = new System.Drawing.Point(300, 100);

                System.Drawing.Point[] points = { point1, point2, point3, point4 };

                g.FillPolygon(new SolidBrush(col), points);
            }

            pictureBox1.Refresh();
        }

        private void drawCircle()
        {
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.FillEllipse(new SolidBrush(col), 200, 125, 200, 200);
            }

            pictureBox1.Refresh();
        }

        private void drawFigure(object sender, EventArgs e)
        {
            clear();

            ColorDialog cd = new ColorDialog();
            cd.ShowDialog();
            col = cd.Color;

            string name = (sender as ToolStripMenuItem).Name;

            if (name == "triangleToolStripMenuItem")
                drawTriangle();
            else if (name == "rectangleToolStripMenuItem")
                drawRectangle();
            else if (name == "rhombToolStripMenuItem")
                drawRhomb();
            else
                drawCircle();

            transformToolStripMenuItem.Enabled = true;
            goToolStripMenuItem.Enabled = true;
        }

        private CvRect getCroppedImage(Bitmap btm)
        {
            List<OpenCvSharp.CPlusPlus.Point> mPoints = new List<OpenCvSharp.CPlusPlus.Point>();

            Mat m = btm.ToMat();

            Cv2.CvtColor(m, m, ColorConversion.BgraToGray);

            for (int i = 0; i < m.Height; i++)
                for (int j = 0; j < m.Width; j++)
                    if (m.At<byte>(i, j) != 255)
                    {
                        mPoints.Add(new OpenCvSharp.CPlusPlus.Point(j, i));

                        goto left;
                    }

        left:
            for (int j = 0; j < m.Width; j++)
                for (int i = 0; i < m.Height; i++)
                    if (m.At<byte>(i, j) != 255)
                    {
                        mPoints.Add(new OpenCvSharp.CPlusPlus.Point(j, i));

                        goto bottom;
                    }

        bottom:
            for (int i = m.Height - 1; i >= 0; i--)
                for (int j = 0; j < m.Width; j++)
                    if (m.At<byte>(i, j) != 255)
                    {
                        mPoints.Add(new OpenCvSharp.CPlusPlus.Point(j, i));

                        goto right;
                    }

        right:
            for (int j = m.Width - 1; j >= 0; j--)
                for (int i = 0; i < m.Height; i++)
                    if (m.At<byte>(i, j) != 255)
                    {
                        mPoints.Add(new OpenCvSharp.CPlusPlus.Point(j, i));

                        goto exit;
                    }
        exit:
            CvRect croppedImage = new CvRect(mPoints[1].X, mPoints[0].Y, mPoints[3].X - mPoints[1].X + 1, mPoints[2].Y - mPoints[0].Y + 1);

            return croppedImage;
        }

        private void transformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();

            CvRect roi = getCroppedImage(bm);

            Mat originalImage = bm.ToMat();

            double scaleRatio = rnd.NextDouble() + 0.5;

            int newWidth = (newWidth = (int)Math.Floor(roi.Width * scaleRatio)) > originalImage.Width ? originalImage.Width - 1 : newWidth;
            int newHeight = (newHeight = (int)Math.Floor(roi.Height * scaleRatio)) > originalImage.Height ? originalImage.Height - 1 : newHeight;

            Mat scaledImage = new Mat(new OpenCvSharp.CPlusPlus.Size(newWidth, newHeight), originalImage.Type());
            Mat shiftedImage = new Mat(originalImage.Size(), originalImage.Type());

            Cv2.Resize(originalImage.SubMat(roi), scaledImage, scaledImage.Size(), 0, 0, Interpolation.Cubic);

            int dx = rnd.Next(0, originalImage.Width - newWidth);
            int dy = rnd.Next(0, originalImage.Height - newHeight);

            shiftedImage.SetTo(Cv.ScalarAll(255));
            scaledImage.CopyTo(shiftedImage.SubMat(new CvRect(dx, dy, newWidth, newHeight)));

            pictureBox1.Image = shiftedImage.ToBitmap();
        }

        private OpenCvSharp.CPlusPlus.Point[] getContourPoints(Bitmap btm)
        {
            Mat image = btm.ToMat();
            Mat grayScale = new Mat(image.Size(), image.Type());

            Cv2.CvtColor(image, grayScale, ColorConversion.BgraToGray);
            Cv2.Threshold(grayScale, grayScale, 227, 255, ThresholdType.Binary);
            Cv2.GaussianBlur(grayScale, grayScale, new OpenCvSharp.CPlusPlus.Size(17, 17), 0);

            OpenCvSharp.CPlusPlus.Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(grayScale, out contours, out hierarchy, ContourRetrieval.List, ContourChain.ApproxNone, new OpenCvSharp.CPlusPlus.Point(0, 0));

            return contours[0];
        }

        private int checkOrientation(OpenCvSharp.CPlusPlus.Point p1, OpenCvSharp.CPlusPlus.Point p2)
        {
            int dx = p2.X - p1.X;
            int dy = p2.Y - p1.Y;

            if (dx == 0 && dy == -1)
                return (int)ORIENTATION.NORTH;
            else if (dx == 1 && dy == -1)
                return (int)ORIENTATION.NORTH_EAST;
            else if (dx == 1 && dy == 0)
                return (int)ORIENTATION.EAST;
            else if (dx == 1 && dy == 1)
                return (int)ORIENTATION.SOUTH_EAST;
            else if (dx == 0 && dy == 1)
                return (int)ORIENTATION.SOUTH;
            else if (dx == -1 && dy == 1)
                return (int)ORIENTATION.SOUTH_WEST;
            else if (dx == -1 && dy == 0)
                return (int)ORIENTATION.WEST;
            else if (dx == -1 && dy == -1)
                return (int)ORIENTATION.NORTH_WEST;
            else
                return (int)ORIENTATION.NONE;
        }

        private double[] getNormalizedHystogram(double[] hyst)
        {
            double[] resHyst = new double[hystSize];

            double sum = 0;

            for (int i = 0; i < hyst.Length; i++)
                sum += hyst[i];

            for (int i = 0; i < hyst.Length; i++)
                resHyst[i] = hyst[i] / sum;

            return resHyst;
        }

        private double[] getHystogram(Bitmap btm)
        {
            double[] hyst = new double[hystSize];
            int orientation;

            var contourPoints = getContourPoints(btm);

            for (int i = 1; i < contourPoints.Length; i++)
            {
                orientation = checkOrientation(contourPoints[i - 1], contourPoints[i]);

                hyst[orientation]++;
            }

            orientation = checkOrientation(contourPoints[contourPoints.Length - 1], contourPoints[0]);

            hyst[orientation]++;

            return getNormalizedHystogram(hyst);
        }

        private double getDistanceBetweenHystograms(double[] hyst1, double[] hyst2)
        {
            double result = 0;

            if (hyst1.Length != hyst2.Length)
                return -1;

            for (int i = 0; i < hyst1.Length; i++)
                result += Math.Abs(hyst1[i] - hyst2[i]);

            return result;
        }

        private void goToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            listOfResults = new List<Bitmap>();
            double[] hyst = getHystogram(bm);

            for (int i = 0; i < images.Count; i++)
            {
                double distance = getDistanceBetweenHystograms(hyst, getHystogram(images[i]));

                if (distance < distThreshold)
                {
                    listOfResults.Add(images[i]);

                    listBox1.Items.Add("Image" + i.ToString());
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);

            if (index != System.Windows.Forms.ListBox.NoMatches)
                pictureBox1.Image = listOfResults[index];
        }
    }
}
