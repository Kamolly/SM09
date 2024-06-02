using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SM09
{
    public partial class UserControl1: UserControl
    {

        private List<PointF> points = new List<PointF>();
        private List<PointF> convexVertices = new List<PointF>();
        private List<PointF> reflexVertices = new List<PointF>();
        private List<List<PointF>> monotonePolygons = new List<List<PointF>>();
        private const int PointRadius = 3;
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            this.Width = 800;
            this.Height = 600;

            Button generateButton = new Button
            {
                Text = "Generate Polygon",
                Location = new Point(10, 10)
            };
            generateButton.Click += GenerateButton_Click;
            this.Controls.Add(generateButton);

            this.Paint += DrawingPanel_Paint;
        }


        private void GenerateButton_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            convexVertices.Clear();
            reflexVertices.Clear();
            monotonePolygons.Clear();
            points.Clear();

             int n = 10;

             for (int i = 0; i < n; i++)
             {
                 points.Add(new PointF(rand.Next(50, 700), rand.Next(50, 450)));
             }


            points = points.OrderBy(p => Math.Atan2(p.Y - points.Average(pt => pt.Y), p.X - points.Average(pt => pt.X))).ToList();

            IdentifyConvexAndReflexVertices();
            PartitionIntoMonotonePolygons();
            this.Invalidate();
        }
        ////////////////////////////////////////////
        private void IdentifyConvexAndReflexVertices()
        {
            for (int i = 0; i < points.Count; i++)
            {
                PointF prevPoint = points[(i - 1 + points.Count) % points.Count];
                PointF currentPoint = points[i];
                PointF nextPoint = points[(i + 1) % points.Count];

                int orientation = Orientation(prevPoint, currentPoint, nextPoint);
                if (orientation == 1)
                    convexVertices.Add(currentPoint);
                else if (orientation == -1)
                    reflexVertices.Add(currentPoint);
            }
        }

        private void PartitionIntoMonotonePolygons()
        {
            List<PointF> reflexVerticesCopy = new List<PointF>(reflexVertices);

            if (reflexVerticesCopy.Count == 0)
            {
                monotonePolygons.Add(points);
                return;
            }

            reflexVerticesCopy = reflexVerticesCopy.OrderByDescending(p => p.Y).ThenBy(p => p.X).ToList();

            foreach (var reflexVertex in reflexVerticesCopy)
            {
                int index = convexVertices.FindIndex(v => v.X < reflexVertex.X);
                if (index == -1)
                    continue;

                PointF v1 = convexVertices[index];
                PointF v2 = convexVertices[(index + 1) % convexVertices.Count];

                List<PointF> monotonePolygon = new List<PointF> { reflexVertex, v1, v2 };
                monotonePolygons.Add(monotonePolygon);

                reflexVertices.Remove(reflexVertex);
            }

            monotonePolygons = monotonePolygons.OrderBy(poly => poly.Min(p => p.Y)).ToList();
        }


        private int Orientation(PointF p0, PointF p1, PointF p2)
        {
            float val = (p1.Y - p0.Y) * (p2.X - p1.X) - (p1.X - p0.X) * (p2.Y - p1.Y);

            if (Math.Abs(val) < 0.000001)
                return 0;
            return (val > 0) ? 1 : -1;
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen polygonPen = new Pen(Color.Red, 2);
            Brush pointBrush = Brushes.Blue;

            foreach (var point in points)
            {
                g.FillEllipse(pointBrush, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
            }

            if (points.Count > 1)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    PointF p1 = points[i];
                    PointF p2 = points[(i + 1) % points.Count];
                    g.DrawLine(polygonPen, p1, p2);
                }
            }

            foreach (var monotonePolygon in monotonePolygons)
            {
                if (monotonePolygon.Count > 1)
                {
                    for (int i = 0; i < monotonePolygon.Count; i++)
                    {
                        PointF p1 = monotonePolygon[i];
                        PointF p2 = monotonePolygon[(i + 1) % monotonePolygon.Count];
                        g.DrawLine(polygonPen, p1, p2);
                    }
                }
            }
        }
       /////////////////////////////////////////////
    }
}
