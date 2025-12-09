using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace EgoDrop
{
    public partial class NetworkView : UserControl
    {
        private List<Node> nodes = new List<Node>();
        private List<Connection> connections = new List<Connection>();

        private Node selectedNode = null;
        private Point dragOffset;

        private bool isDraggingConnection = false;
        private Node connectionStartNode = null;
        private Point currentMousePos;

        private Node selectedNodeForHighlight = null;

        public NetworkView()
        {
            InitializeComponent();
            DoubleBuffered = true;

            Paint += NetworkView_Paint;
            MouseDown += NetworkView_MouseDown;
            MouseMove += NetworkView_MouseMove;
            MouseUp += NetworkView_MouseUp;

            ForeColor = Color.White;
        }

        private void NetworkView_Load(object sender, EventArgs e)
        {

        }

        private void NetworkView_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var c in connections)
                DrawArrow(g, c.From, c.To, Color.LimeGreen);

            if (isDraggingConnection && connectionStartNode != null)
            {
                Node temp = new Node { Position = currentMousePos, Size = 1 };
                DrawArrow(g, connectionStartNode, temp, Color.LightGray);
            }

            foreach (var n in nodes)
                DrawNode(g, n);
        }

        private void DrawArrow(Graphics g, Node fromNode, Node toNode, Color color)
        {
            int offsetStart = fromNode.Size / 2;
            int offsetEnd = toNode.Size / 2;

            Point start = MovePointTowards(fromNode.Position, toNode.Position, offsetStart);
            Point end = MovePointTowards(toNode.Position, fromNode.Position, offsetEnd);

            using (var shadow = new Pen(Color.FromArgb(120, 0, 0, 0), 6))
            {
                shadow.StartCap = LineCap.Round;
                shadow.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(shadow, start, end);
            }

            using (var pen = new Pen(color, 5))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen, start, end);
            }
        }

        private Point MovePointTowards(Point p1, Point p2, int distance)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len == 0) return p1;
            double scale = distance / len;
            return new Point((int)(p1.X + dx * scale), (int)(p1.Y + dy * scale));
        }

        private void DrawNode(Graphics g, Node node)
        {
            var r = node.Bounds;

            if (node == selectedNodeForHighlight)
            {
                using (var highlightPen = new Pen(Color.Cyan, 3))
                {
                    g.DrawEllipse(highlightPen, r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
                }
            }

            if (node.Icon != null)
            {
                g.DrawImage(node.Icon, r);
            }
            else
            {
                using (var p = new Pen(Color.Black, 2))
                {
                    g.DrawRectangle(p, r);
                    g.DrawRectangle(p, r.X + 4, r.Y + 12, r.Width - 8, r.Height / 2 - 6);
                }
            }

            if (!string.IsNullOrEmpty(node.HostID))
            {
                var idRect = new Rectangle(r.X - 10, r.Bottom + 4, r.Width + 20, 16);
                using (var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center })
                using (var font = new Font(Font.FontFamily, 9, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                    g.DrawString(node.HostID, font, brush, idRect, sf);
            }
        }

        private void NetworkView_MouseDown(object sender, MouseEventArgs e)
        {
            selectedNode = null;
            selectedNodeForHighlight = null;

            foreach (var node in nodes)
            {
                if (!node.Bounds.Contains(e.Location))
                    continue;

                if (e.Button == MouseButtons.Left)
                {
                    selectedNode = node;
                    selectedNodeForHighlight = node;
                    dragOffset = new Point(e.X - node.Position.X, e.Y - node.Position.Y);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    isDraggingConnection = true;
                    connectionStartNode = node;
                    currentMousePos = e.Location;
                }

                Invalidate();
                return;
            }
        }

        private void NetworkView_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location;

            if (selectedNode != null && e.Button == MouseButtons.Left)
            {
                selectedNode.Position = new Point(e.X - dragOffset.X, e.Y - dragOffset.Y);
                Invalidate();
            }

            if (isDraggingConnection)
                Invalidate();
        }

        private void NetworkView_MouseUp(object sender, MouseEventArgs e)
        {
            selectedNode = null;

            if (isDraggingConnection && connectionStartNode != null)
            {
                foreach (var node in nodes)
                {
                    if (node.Bounds.Contains(e.Location) && node != connectionStartNode)
                    {
                        connections.Add(new Connection { From = connectionStartNode, To = node });
                        break;
                    }
                }
            }

            isDraggingConnection = false;
            connectionStartNode = null;
            Invalidate();
        }

        public Node AddNode(string name, string hostID, Point pos, Image icon = null)
        {
            Node n = new Node { Name = name, HostID = hostID, Position = pos, Icon = icon };

            n.Position = GetNonOverlappingPosition(n);

            nodes.Add(n);
            Invalidate();
            return n;
        }

        private Point GetNonOverlappingPosition(Node node)
        {
            const int padding = 10;
            Point newPos = node.Position;

            bool hasCollision;
            int attempt = 0;
            do
            {
                hasCollision = false;
                foreach (var existing in nodes)
                {
                    Rectangle r1 = new Rectangle(newPos.X - node.Size / 2, newPos.Y - node.Size / 2, node.Size, node.Size);
                    Rectangle r2 = existing.Bounds;
                    r2.Inflate(padding, padding);

                    if (r1.IntersectsWith(r2))
                    {
                        newPos.X += node.Size + padding;
                        newPos.Y += node.Size + padding;
                        hasCollision = true;
                        break;
                    }
                }

                attempt++;
                if (attempt > 1000) break;
            } while (hasCollision);

            return newPos;
        }

        public void AddConnection(Node a, Node b)
        {
            connections.Add(new Connection { From = a, To = b });
            Invalidate();
        }

        public void Clear()
        {
            nodes.Clear();
            connections.Clear();
            Invalidate();
        }
    }

    public class Node
    {
        public string Name;
        public string HostID;
        public Point Position;
        public Image Icon;

        public int Size = 100;
        public Rectangle Bounds => new Rectangle(Position.X - Size / 2, Position.Y - Size / 2, Size, Size);
    }

    public class Connection
    {
        public Node From;
        public Node To;
    }
}
