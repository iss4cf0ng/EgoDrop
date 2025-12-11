using EgoDrop.Properties;
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
        private Node selectedNodeForHighlight = null;
        private Point dragOffset;

        private bool isDraggingConnection = false;
        private Node connectionStartNode = null;
        private Point currentMousePos;

        private float zoom = 1.0f;
        private Point panOffset = new Point(0, 0);

        private Point lastMouse;
        private Rectangle hScrollBar, vScrollBar;
        private bool isDraggingHScroll = false, isDraggingVScroll = false;
        private Point scrollDragStart;

        private int virtualWidth = 2000;
        private int virtualHeight = 2000;

        public Node SelectedNode { get; private set; }
        public ImageList ImageList;

        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = Math.Max(0.1f, Math.Min(5f, value));
                Invalidate();
            }
        }

        public NetworkView()
        {
            DoubleBuffered = true;
            BackColor = Color.Black;
            Paint += NetworkView_Paint;
            MouseDown += NetworkView_MouseDown;
            MouseMove += NetworkView_MouseMove;
            MouseUp += NetworkView_MouseUp;
            MouseWheel += NetworkView_MouseWheel;
        }

        #region MouseWheel

        //Scrollbar.
        private void UpdateScrollBar()
        {
            int barWidth = 12;
            int hLength = (int)(this.Width * (this.Width / (float)(virtualWidth * zoom)));
            int vLength = (int)(this.Height * (this.Height / (float)(virtualHeight * zoom)));

            hScrollBar = new Rectangle((int)(-panOffset.X * this.Width / (virtualWidth * zoom)), this.Height - barWidth, hLength, barWidth);
            vScrollBar = new Rectangle(this.Width - barWidth, (int)(-panOffset.Y * this.Height / (virtualHeight * zoom)), barWidth, vLength);
        }

        private void NetworkView_MouseWheel(object sender, MouseEventArgs e)
        {
            int scrollSpeed = 10; //Scrolling amount.

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                //Ctrl -> Zoom.
                float oldZoom = zoom;
                zoom *= e.Delta > 0 ? 1.1f : 0.9f;
                zoom = Math.Max(0.1f, Math.Min(5f, zoom));

                //Scrolling base on cursor as the center.
                panOffset.X = (int)(e.X - (e.X - panOffset.X) * zoom / oldZoom);
                panOffset.Y = (int)(e.Y - (e.Y - panOffset.Y) * zoom / oldZoom);
            }
            else
            {
                int scrollAmount = (e.Delta > 0 ? scrollSpeed : -scrollSpeed);
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    panOffset.X += scrollAmount; //Shift + Scrolling -> Horizontal scrolling.
                else
                    panOffset.Y += scrollAmount; //Vertical scrolling (Default).
            }

            UpdateScrollBar();
            Invalidate();
        }

        private void NetworkView_MouseDown(object sender, MouseEventArgs e)
        {
            if (hScrollBar.Contains(e.Location))
            {
                isDraggingHScroll = true;
                scrollDragStart = e.Location;
                return;
            }

            if (vScrollBar.Contains(e.Location))
            {
                isDraggingVScroll = true;
                scrollDragStart = e.Location;
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                selectedNodeForHighlight = null;
            }

            foreach (var node in nodes)
            {
                Rectangle nodeRect = new Rectangle(
                    Transform(node.Position).X - (int)(node.Size * zoom / 2),
                    Transform(node.Position).Y - (int)(node.Size * zoom / 2),
                    (int)(node.Size * zoom),
                    (int)(node.Size * zoom));

                if (!nodeRect.Contains(e.Location))
                    continue;

                if (e.Button == MouseButtons.Left)
                {
                    selectedNode = node;
                    selectedNodeForHighlight = node;
                    dragOffset = new Point(e.X - Transform(node.Position).X, e.Y - Transform(node.Position).Y);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    selectedNode = node;
                    selectedNodeForHighlight = node;
                }

                Invalidate();
                return;
            }

            lastMouse = e.Location;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            Node clicked = HitTestNode(e.Location);

            if (e.Button == MouseButtons.Left)
            {
                SelectedNode = clicked;
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (clicked != null)
                    SelectedNode = clicked;

                if (SelectedNode != null && this.ContextMenuStrip != null)
                    this.ContextMenuStrip.Show(this, e.Location);

                Invalidate();
            }
        }

        private Rectangle GetNodeBounds(Node n)
        {
            int iconW = n.Icon.Width;
            int iconH = n.Icon.Height;

            int textH = 40;

            return new Rectangle(
                n.Position.X - iconW / 2,
                n.Position.Y - (iconH + textH) / 2,
                iconW,
                iconH + textH
            );
        }

        private Node HitTestNode(Point p)
        {
            foreach (var n in nodes)
            {
                var r = GetNodeBounds(n);
                if (r.Contains(p))
                    return n;
            }
            return null;
        }



        private void NetworkView_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location;

            if (isDraggingHScroll)
            {
                int dx = e.X - scrollDragStart.X;
                panOffset.X -= (int)(dx * virtualWidth * zoom / this.Width);
                scrollDragStart = e.Location;
                UpdateScrollBar();
                Invalidate();
                return;
            }

            if (isDraggingVScroll)
            {
                int dy = e.Y - scrollDragStart.Y;
                panOffset.Y -= (int)(dy * virtualHeight * zoom / this.Height);
                scrollDragStart = e.Location;
                UpdateScrollBar();
                Invalidate();
                return;
            }

            if (selectedNode != null && e.Button == MouseButtons.Left)
            {
                selectedNode.Position = new Point((int)((e.X - dragOffset.X) / zoom - panOffset.X),
                                                 (int)((e.Y - dragOffset.Y) / zoom - panOffset.Y));
                Invalidate();
            }

            if (isDraggingConnection)
                Invalidate();
        }

        private void NetworkView_MouseUp(object sender, MouseEventArgs e)
        {
            selectedNode = null;
            isDraggingHScroll = false;
            isDraggingVScroll = false;

            if (isDraggingConnection && connectionStartNode != null)
            {
                foreach (var node in nodes)
                {
                    Rectangle nodeRect = new Rectangle(Transform(node.Position).X - (int)(node.Size * zoom / 2),
                                                       Transform(node.Position).Y - (int)(node.Size * zoom / 2),
                                                       (int)(node.Size * zoom),
                                                       (int)(node.Size * zoom));
                    if (nodeRect.Contains(e.Location) && node != connectionStartNode)
                    {
                        connections.Add(new Connection { From = connectionStartNode, To = node });
                        break;
                    }
                }
            }

            isDraggingConnection = false;
            connectionStartNode = null;

            UpdateScrollBar();
            Invalidate();
        }

        #endregion
        #region Painting

        private void NetworkView_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(Color.Black);

            //Draw line.
            foreach (var c in connections)
                DrawArrow(g, c.From, c.To, Color.LimeGreen);

            //Draw dragging line.
            if (isDraggingConnection && connectionStartNode != null)
            {
                Node temp = new Node { Position = currentMousePos, Size = 1 };
                DrawArrow(g, connectionStartNode, temp, Color.Gray);
            }

            //Draw node.
            foreach (var n in nodes)
                DrawNode(g, n);

            //Draw scrollbar.
            using (var brush = new SolidBrush(Color.Gray))
                g.FillRectangle(brush, hScrollBar);
            using (var brush = new SolidBrush(Color.Gray))
                g.FillRectangle(brush, vScrollBar);
        }

        private Point Transform(Point p)
        {
            return new Point(
                (int)((p.X + panOffset.X) * zoom),
                (int)((p.Y + panOffset.Y) * zoom)
            );
        }

        private void DrawNode(Graphics g, Node node)
        {
            int iconSize = (int)(node.Size * zoom);
            int highlightMargin = (int)(6 * zoom);
            int textMargin = (int)(4 * zoom);

            Point pos = Transform(node.Position);

            if (node == selectedNodeForHighlight)
            {
                using (var highlightPen = new Pen(Color.Cyan, 3 * zoom))
                {
                    int highlightSize = iconSize + highlightMargin * 2;
                    g.DrawEllipse(highlightPen,
                        pos.X - highlightSize / 2,
                        pos.Y - highlightSize / 2,
                        highlightSize,
                        highlightSize);
                }
            }

            var iconRect = new Rectangle(pos.X - iconSize / 2,
                                         pos.Y - iconSize / 2,
                                         iconSize, iconSize);

            if (node.Icon != null)
                g.DrawImage(node.Icon, iconRect);
            else
                using (var p = new Pen(Color.White, 2 * zoom))
                    g.DrawRectangle(p, iconRect);

            if (!string.IsNullOrEmpty(node.HostID))
            {
                int maxWidth = (int)(iconRect.Width + 20);
                var idRect = new Rectangle(iconRect.X - 10, iconRect.Bottom + textMargin, maxWidth, 1000);

                using (var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Near,
                    Alignment = StringAlignment.Center,
                    FormatFlags = 0,
                })
                using (var font = new Font(Font.FontFamily, 9 * zoom, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(node.HostID, font, brush, idRect, sf);
                }
            }
        }

        private void DrawArrow(Graphics g, Node fromNode, Node toNode, Color color, int edgeIndex = 0)
        {
            double dx = toNode.Position.X - fromNode.Position.X;
            double dy = toNode.Position.Y - fromNode.Position.Y;

            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.01) return;

            double nx = -dy / len;
            double ny = dx / len;

            double offsetAmount = 12 * edgeIndex * zoom;

            PointF offset = new PointF((float)(nx * offsetAmount), (float)(ny * offsetAmount));

            int safeMargin = (int)(8 * zoom);
            int offsetStart = (int)(fromNode.Size * 0.5f * zoom) + safeMargin;
            int offsetEnd = (int)(toNode.Size * 0.5f * zoom) + safeMargin;

            PointF p1 = MovePointTowardsF(fromNode.Position, toNode.Position, offsetStart);
            PointF p2 = MovePointTowardsF(toNode.Position, fromNode.Position, offsetEnd);

            PointF start = TransformF(new PointF(p1.X + offset.X, p1.Y + offset.Y));
            PointF end = TransformF(new PointF(p2.X + offset.X, p2.Y + offset.Y));

            using (var shadow = new Pen(Color.FromArgb(150, 0, 0, 0), 6 * zoom))
            {
                shadow.StartCap = LineCap.Round;
                shadow.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(shadow, start, end);
            }

            using (var pen = new Pen(color, 4 * zoom))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen, start, end);
            }
        }

        private PointF MovePointTowardsF(Point from, Point to, float distance)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) return from;
            float k = distance / len;
            return new PointF(from.X + dx * k, from.Y + dy * k);
        }

        private PointF TransformF(PointF p)
        {
            return new PointF(
                (float)((p.X + panOffset.X) * zoom),
                (float)((p.Y + panOffset.Y) * zoom)
            );
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

        #endregion
        #region Public API

        public Node AddNode(string name, string hostID, Point pos, Image icon = null)
        {
            Node n = new Node { Name = name, HostID = hostID, Position = pos, Icon = icon };
            n.Position = GetNonOverlappingPosition(n);
            nodes.Add(n);
            Invalidate();
            return n;
        }

        public void RemoveNode(Node node)
        {
            if (node == null) return;
            connections.RemoveAll(c => c.From == node || c.To == node);
            nodes.Remove(node);
            if (selectedNodeForHighlight == node)
                selectedNodeForHighlight = null;
            Invalidate();
        }

        public void AddConnection(Node a, Node b)
        {
            if (a != null && b != null)
            {
                connections.Add(new Connection { From = a, To = b });
                Invalidate();
            }
        }

        public void Clear()
        {
            nodes.Clear();
            connections.Clear();
            selectedNodeForHighlight = null;
            Invalidate();
        }

        //Avoiding overlapping.
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

        public Node FindNodeWithName(string szName)
        {
            foreach (var node in nodes)
                if (string.Equals(node.Name, szName))
                    return node;

            return null;
        }

        public Node FindNodeWithID(string szID)
        {
            foreach (var node in nodes)
                if (string.Equals(node.HostID, szID))
                    return node;

            return null;
        }

        public Connection FindConnection(Node srcNode, Node dstNode, bool bDirected = true)
        {
            foreach (var conn in connections)
            {
                if (bDirected && conn.From == srcNode && conn.To == dstNode)
                {
                    return conn;
                }
                else if (!bDirected & ((conn.From == srcNode && conn.To == dstNode) || (conn.To == srcNode && conn.From == dstNode)))
                {
                    return conn;
                }
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class Node
    {
        public string Name;    //Node's name.
        public string HostID;  //Host's ID.
        public Point Position; //Node's position.
        public Image Icon;     //Node's icon.

        public int Size = 100;
        public Rectangle Bounds => new Rectangle(Position.X - Size / 2, Position.Y - Size / 2, Size, Size);

        public Node ChildNode = null;
        public Node ParentNode = null;

        public void fnSetChildNode(Node node)
        {

        }

        public void fnSetParentNode(Node node)
        {
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Connection
    {
        public Node From; //Source node.
        public Node To; //Destination node.
    }
}
