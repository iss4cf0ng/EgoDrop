using EgoDrop.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.ModelConfiguration.Configuration;
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
        private List<NetworkNode> nodes = new List<NetworkNode>();
        private List<NetworkConnection> connections = new List<NetworkConnection>();

        // ===== Selection =====
        private HashSet<NetworkNode> selectedNodes = new HashSet<NetworkNode>();
        private bool isBoxSelecting = false;
        private Point boxSelectStart;
        private Rectangle boxSelectRect;

        // ===== Drag =====
        private bool isDraggingNodes = false;
        private Dictionary<NetworkNode, Point> dragStartPositions = new Dictionary<NetworkNode, Point>();

        private NetworkNode? selectedNode = null;
        private NetworkNode? selectedNodeForHighlight = null;
        private Point dragOffset;

        private bool isDraggingConnection = false;
        private NetworkNode? connectionStartNode = null;
        private Point currentMousePos;

        private float zoom = 1.0f;
        private Point panOffset = new Point(0, 0);

        private Point lastMouse;
        private Rectangle hScrollBar, vScrollBar;
        private bool isDraggingHScroll = false, isDraggingVScroll = false;
        private Point scrollDragStart;

        private int virtualWidth = 2000;
        private int virtualHeight = 2000;

        public NetworkNode? SelectedNode { get; private set; }
        public List<NetworkNode> SelectNodes { get { return selectedNodes.ToList(); } }
        public ImageList imageList;

        private bool _DisplayProtocol = false;

        /// <summary>
        /// Display the protocol of each connection.
        /// </summary>
        public bool DisplayProtocol
        {
            get { return _DisplayProtocol; }
            set
            {
                _DisplayProtocol = value;

                Invalidate();
            }
        }

        private enTopologyLayout _NetworkViewTopoLogy;

        /// <summary>
        /// NetworkView topology method.
        /// </summary>
        public enTopologyLayout NetworkViewTopoLogy
        {
            get { return _NetworkViewTopoLogy; }
            set
            {
                _NetworkViewTopoLogy = value;

                List<NetworkNode> lsNode = new List<NetworkNode>();
                List<NetworkConnection> lsConn = new List<NetworkConnection>();

                lsNode.AddRange(nodes);
                lsConn.AddRange(connections);

                Clear();

                foreach (NetworkNode node in lsNode)
                {
                    if (node.MachineStatus == enMachineStatus.Firewall)
                        continue;

                    var n1 = AddNode(node.szVictimID, node.szDisplayName, node.MachineStatus);
                    var conn = lsConn.Where(x => string.Equals(x.To.szDisplayName, n1.szDisplayName)).First();
                    if (node.ParentNode.MachineStatus == enMachineStatus.Firewall)
                    {
                        var nodeFirewall = AddNode(node.ParentNode.szVictimID, node.ParentNode.szDisplayName, enMachineStatus.Firewall);
                        n1.ParentNode = nodeFirewall;
                        nodeFirewall.ChildNodes.Add(n1);

                        AddConnection(nodeFirewall, n1, true, conn.enProtocol);
                    }
                    else
                    {
                        var nodeParent = FindNodeWithID(node.ParentNode.szVictimID);
                        n1.ParentNode = nodeParent;
                        nodeParent.ChildNodes.Add(n1);
                        AddConnection(nodeParent, n1, true, conn.enProtocol);
                    }

                    BringGraphIntoView();
                }
            }
        }

        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = Math.Max(0.1f, Math.Min(5f, value));
                Invalidate();
            }
        }

        /// <summary>
        /// Victim's connection type.
        /// </summary>
        public enum enConnectionType
        {
            DISCONNECTED,
            SUPERUSER,

            TCP,
            TLS,
            DNS,
            HTTP,
            HTTPS,
            SMB,

        }

        private Dictionary<enConnectionType, Color> m_dicConnectionColor = new Dictionary<enConnectionType, Color>()
        {
            { enConnectionType.DISCONNECTED, Color.LightGray },
            { enConnectionType.SUPERUSER, Color.Red },

            { enConnectionType.TCP, Color.LimeGreen },
            { enConnectionType.TLS, Color.Plum },
            { enConnectionType.DNS, Color.Blue },
            { enConnectionType.HTTP, Color.LightGreen },
            { enConnectionType.HTTPS, Color.MediumAquamarine },
        };

        /// <summary>
        /// Victim machine status.
        /// </summary>
        public enum enMachineStatus
        {
            Unknown,
            Firewall,

            //Linux.
            Linux_Normal,
            Linux_Infected,
            Linux_Super,
            Linux_Beacon,

            //Windows.
            Windows_Normal,
            Windows_Infected,
            Windows_Super,
            Windows_Beacon,

            //MacOS.
            Mac_Normal,
            Mac_Infected,
            Mac_Super,
            Mac_Beacon,

            //Router.
            Router_Normal,
            Router_Infected,
            Router_Super,
            Router_Beacon,

            //Printer.
            Printer_Normal,
            Printer_Infected,
            Printer_Super,
            Printer_Beacon,

            //Webcam.
            Webcam_Normal,
            Webcam_Infected,
            Webcam_Super,
            Webcam_Beacon,


        }
        public enum enTopologyLayout
        {
            Tree,
            Pyramid,
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
            Rectangle graph = GetGraphBounds();

            float viewW = Width / zoom;
            float viewH = Height / zoom;

            // ===== Horizontal =====
            if (graph.Width <= viewW)
            {
                hScrollBar = Rectangle.Empty;
            }
            else
            {
                float ratio = viewW / graph.Width;
                int barW = Math.Max(20, (int)(Width * ratio));

                float scrollRange = graph.Width - viewW;
                float scrollPos = (-panOffset.X - graph.Left) / scrollRange;

                int x = (int)(scrollPos * (Width - barW));
                x = Math.Max(0, Math.Min(Width - barW, x));

                hScrollBar = new Rectangle(x, Height - 12, barW, 12);
            }

            // ===== Vertical =====
            if (graph.Height <= viewH)
            {
                vScrollBar = Rectangle.Empty;
            }
            else
            {
                float ratio = viewH / graph.Height;
                int barH = Math.Max(20, (int)(Height * ratio));

                float scrollRange = graph.Height - viewH;
                float scrollPos = (-panOffset.Y - graph.Top) / scrollRange;

                int y = (int)(scrollPos * (Height - barH));
                y = Math.Max(0, Math.Min(Height - barH, y));

                vScrollBar = new Rectangle(Width - 12, y, 12, barH);
            }
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
            Focus();

            // ===== Scrollbar priority =====
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

            NetworkNode hit = HitTestNode(e.Location);

            // ===== Left Button =====
            if (e.Button == MouseButtons.Left)
            {
                //Ctrl multi selection.
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    if (hit != null)
                    {
                        if (selectedNodes.Contains(hit))
                            selectedNodes.Remove(hit);
                        else
                            selectedNodes.Add(hit);
                    }
                }
                else
                {
                    if (hit != null)
                    {
                        //Drag all.
                        if (!selectedNodes.Contains(hit))
                        {
                            selectedNodes.Clear();
                            selectedNodes.Add(hit);
                        }

                        StartDragNodes(e.Location);
                    }
                    else
                    {
                        //Draw selection box.
                        selectedNodes.Clear();
                        isBoxSelecting = true;
                        boxSelectStart = e.Location;
                    }
                }

                SelectedNode = hit;
                Invalidate();
            }
        }

        private void StartDragNodes(Point mousePos)
        {
            isDraggingNodes = true;
            dragStartPositions.Clear();

            foreach (var n in selectedNodes)
                dragStartPositions[n] = n.Position;

            lastMouse = mousePos;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            NetworkNode clicked = HitTestNode(e.Location);

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

        private Rectangle GetNodeBounds(NetworkNode n)
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

        private NetworkNode HitTestNode(Point screenPoint)
        {
            // screen → world
            Point world = new Point(
                (int)(screenPoint.X / zoom - panOffset.X),
                (int)(screenPoint.Y / zoom - panOffset.Y)
            );

            foreach (var n in nodes)
            {
                if (n.Bounds.Contains(world))
                    return n;
            }
            return null;
        }

        private void NetworkView_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location;

            // ===== Scrollbar =====
            if (isDraggingHScroll)
            {
                int dx = e.X - scrollDragStart.X;
                panOffset.X -= (int)(dx * virtualWidth * zoom / Width);
                scrollDragStart = e.Location;
                UpdateScrollBar();
                Invalidate();
                return;
            }

            if (isDraggingVScroll)
            {
                int dy = e.Y - scrollDragStart.Y;
                panOffset.Y -= (int)(dy * virtualHeight * zoom / Height);
                scrollDragStart = e.Location;
                UpdateScrollBar();
                Invalidate();
                return;
            }

            // ===== Drag nodes =====
            if (isDraggingNodes)
            {
                int dx = (int)((e.X - lastMouse.X) / zoom);
                int dy = (int)((e.Y - lastMouse.Y) / zoom);

                foreach (var n in selectedNodes)
                {
                    Point p = dragStartPositions[n];
                    n.Position = ClampToVirtual(new Point(p.X + dx, p.Y + dy));
                }

                Invalidate();
                return;
            }

            // ===== Box select =====
            if (isBoxSelecting)
            {
                boxSelectRect = GetRect(boxSelectStart, e.Location);
                Invalidate();
            }
        }

        private void NetworkView_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingHScroll = false;
            isDraggingVScroll = false;
            isDraggingNodes = false;

            if (isBoxSelecting)
            {
                selectedNodes.Clear();

                foreach (var n in nodes)
                {
                    Rectangle r = GetNodeBoundsScreen(n);
                    if (boxSelectRect.IntersectsWith(r))
                        selectedNodes.Add(n);
                }
            }

            Point ptrEmpty = new Point(0, 0);
            isBoxSelecting = false;
            boxSelectRect = GetRect(ptrEmpty, ptrEmpty);

            UpdateScrollBar();
            Invalidate();
        }



        #endregion
        #region Painting

        private void NetworkView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Black);

            foreach (var c in connections)
                DrawArrow(g, c.From, c.To, m_dicConnectionColor[c.enProtocol], c.nOffsetIndex, c.bIsConnected);

            foreach (var n in nodes)
                DrawNode(g, n);

            // ===== Box select =====
            if (isBoxSelecting)
            {
                using var pen = new Pen(Color.Cyan, 1) { DashStyle = DashStyle.Dash };
                g.DrawRectangle(pen, boxSelectRect);
            }

            // ===== Scrollbar =====
            using var brush = new SolidBrush(Color.Gray);

            if (!hScrollBar.IsEmpty)
                g.FillRectangle(brush, hScrollBar);

            if (!vScrollBar.IsEmpty)
                g.FillRectangle(brush, vScrollBar);
        }

        private Point Transform(Point p)
        {
            return new Point(
                (int)((p.X + panOffset.X) * zoom),
                (int)((p.Y + panOffset.Y) * zoom)
            );
        }

        private Rectangle GetImageRectKeepAspect(Image img, Point center, int maxSize)
        {
            float ratio = Math.Min(
                (float)maxSize / img.Width,
                (float)maxSize / img.Height
            );

            int w = (int)(img.Width * ratio);
            int h = (int)(img.Height * ratio);

            return new Rectangle(
                center.X - w / 2,
                center.Y - h / 2,
                w,
                h
            );
        }

        private void DrawNode(Graphics g, NetworkNode node)
        {
            Point pos = Transform(node.Position);
            int size = (int)(node.Size * zoom);
            int textMargin = (int)(6 * zoom);

            // ===== Selection highlight =====
            if (selectedNodes.Contains(node))
            {
                using var pen = new Pen(Color.Cyan, 3 * zoom);
                g.DrawEllipse(
                    pen,
                    pos.X - size / 2 - 6,
                    pos.Y - size / 2 - 6,
                    size + 12,
                    size + 12);
            }

            // ===== Icon =====
            Rectangle iconRect = new Rectangle(
                pos.X - size / 2,
                pos.Y - size / 2,
                size,
                size);

            if (node.Icon != null)
                g.DrawImage(node.Icon, iconRect);
            else
                g.DrawRectangle(Pens.White, iconRect);

            // ===== Text (Display Name) =====
            if (!string.IsNullOrEmpty(node.szDisplayName))
            {
                Rectangle textRect = new Rectangle(
                    iconRect.X - 20,
                    iconRect.Bottom + textMargin,
                    iconRect.Width + 40,
                    (int)(40 * zoom));

                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };

                using var font = new Font(
                    Font.FontFamily,
                    Math.Max(8f, 9f * zoom),
                    FontStyle.Bold);

                using var brush = new SolidBrush(Color.White);

                g.DrawString(node.szDisplayName, font, brush, textRect, sf);
            }
        }

        private Point ClampToVirtual(Point p)
        {
            p.X = Math.Max(0, Math.Min(virtualWidth, p.X));
            p.Y = Math.Max(0, Math.Min(virtualHeight, p.Y));
            return p;
        }

        private Rectangle GetRect(Point a, Point b)
        {
            return new Rectangle(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y),
                Math.Abs(a.X - b.X),
                Math.Abs(a.Y - b.Y));
        }

        private Rectangle GetNodeBoundsScreen(NetworkNode n)
        {
            Point p = Transform(n.Position);
            int s = (int)(n.Size * zoom);
            return new Rectangle(p.X - s / 2, p.Y - s / 2, s, s);
        }

        private void DrawArrow(Graphics g, NetworkNode fromNode, NetworkNode toNode, Color color, int edgeIndex = 0, bool bIsConnected = true)
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

            //Shadow
            using (var shadow = new Pen(Color.FromArgb(150, 0, 0, 0), 8 * zoom))
            {
                shadow.StartCap = LineCap.Round;
                shadow.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(shadow, start, end);
            }

            //Main arrow
            using (var pen = new Pen(color, 4 * zoom))
            {
                if (!bIsConnected)
                    pen.DashStyle = DashStyle.Dot;
                
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen, start, end);
            }

            //Text(Detail)
            if (DisplayProtocol)
            {
                PointF mid = new PointF((start.X + end.X) / 2, (start.Y + end.Y) / 2);
                using (var font = new Font(Font.FontFamily, Math.Max(8f, 9f * zoom), FontStyle.Bold))
                {
                    var conn = FindConnection(fromNode, toNode);
                    if (conn == null)
                        return;

                    string szText = Enum.GetName(conn.enProtocol);
                    SizeF textSize = g.MeasureString(szText, font);

                    RectangleF bg = new RectangleF(
                        mid.X - (textSize.Width / 2) - 4,
                        mid.Y - (textSize.Height / 2) - 2,
                        textSize.Width + 8,
                        textSize.Height + 4
                    );

                    using (var bgBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                    {
                        using (var textBrush = new SolidBrush(Color.White))
                        {
                            g.FillRectangle(bgBrush, bg);
                            g.DrawString(szText, font, textBrush, mid.X - (textSize.Width / 2), mid.Y);
                        }
                    }
                }
            }
        }

        private PointF MovePointTowardsF(Point from, Point to, float distance)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.001f)
                return from;

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

        private Rectangle GetGraphBounds()
        {
            if (nodes.Count == 0)
                return new Rectangle(0, 0, Width, Height);

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (var n in nodes)
            {
                minX = Math.Min(minX, n.Position.X - n.Size / 2);
                minY = Math.Min(minY, n.Position.Y - n.Size / 2);
                maxX = Math.Max(maxX, n.Position.X + n.Size / 2);
                maxY = Math.Max(maxY, n.Position.Y + n.Size / 2);
            }

            const int margin = 100;
            return Rectangle.FromLTRB(
                minX - margin,
                minY - margin,
                maxX + margin,
                maxY + margin
            );
        }

        private void ClampPanOffset()
        {
            Rectangle graph = GetGraphBounds();

            float viewW = Width / zoom;
            float viewH = Height / zoom;

            if (graph.Width <= viewW)
                panOffset.X = (int)(-graph.Left + (viewW - graph.Width) / 2);
            else
                panOffset.X = Math.Min(-graph.Left, Math.Max((int)(viewW - graph.Right), panOffset.X));

            if (graph.Height <= viewH)
                panOffset.Y = (int)(-graph.Top + (viewH - graph.Height) / 2);
            else
                panOffset.Y = Math.Min(
                    -graph.Top,
                    Math.Max((int)(viewH - graph.Bottom), panOffset.Y)
                );
        }

        #endregion
        #region Topology

        private Rectangle fnGetNetworkRectangleWithName(string szDisplayName)
        {
            foreach (var node in nodes)
            {
                if (string.Equals(szDisplayName, node.szDisplayName))
                    return fnGetNetworkRectangleWithID(node.szVictimID);
            }

            return new Rectangle();
        }
        private Rectangle fnGetNetworkRectangleWithID(string szVictimID)
        {
            NetworkNode node = null;
            foreach (var n in nodes)
            {
                if (string.Equals(n.szVictimID, szVictimID))
                {
                    node = n;
                    break;
                }
            }

            if (node == null)
                return new Rectangle();

            List<NetworkNode> lsNode = fnGetAllNodesFromCurrentNetwork(node);
            List<Point> lsPoint = lsNode.Select(x => x.Position).ToList();
            List<int> lsX = lsPoint.Select(x => x.X).ToList();
            List<int> lsY = lsPoint.Select(x => x.Y).ToList();

            int nMaxX = lsX.Max();
            int nMaxY = lsY.Max();
            int nMinX = lsX.Min();
            int nMinY = lsY.Min();

            return new Rectangle()
            {
                X = nMinX,
                Y = nMinY,
                Width = nMaxX - nMinX,
                Height = nMaxY - nMinY,
            };
        }

        private List<NetworkNode> fnGetAllNodesFromCurrentNetwork(NetworkNode node)
        {
            List<NetworkNode> lsNode = new List<NetworkNode>();

            if (node.ParentNode != null && node.ParentNode.MachineStatus != enMachineStatus.Firewall)
                lsNode.AddRange(fnGetAllNodesFromCurrentNetwork(node.ParentNode).Where(x => !lsNode.Contains(x)));

            foreach (var n in node.ChildNodes)
            {
                List<NetworkNode> ln = fnGetAllNodesFromCurrentNetwork(n);
                lsNode.AddRange(ln.Where(x => !lsNode.Contains(x)));
            }

            lsNode.Add(node);

            return lsNode;
        }

        private Dictionary<NetworkNode, List<NetworkNode>> fnGetAllNetworks()
        {
            Dictionary<NetworkNode, List<NetworkNode>> dicNodes = new Dictionary<NetworkNode, List<NetworkNode>>();
            List<NetworkNode> firewallNodes = nodes.Where(x => x.MachineStatus == enMachineStatus.Firewall).ToList();
            foreach (var firewallNode in firewallNodes)
            {
                List<NetworkNode> lsNode = fnGetAllNodesFromCurrentNetwork(firewallNode);
                dicNodes.Add(firewallNode, lsNode);
            }

            return dicNodes;
        }

        private void fnLayoutFanOutChildren(NetworkNode nodeParent, enTopologyLayout layout, int nSpacingPrimary = 200, int nSpacingSecondary = 120)
        {
            if (nodeParent == null || nodeParent.ChildNodes.Count == 0)
                return;

            var children = nodeParent.ChildNodes.ToList();
            int count = children.Count;

            int spacing = 160;   //Distance between child.
            int distance = 200;  //Distance to parent.

            float startIndex = -(count - 1) / 2f;

            for (int i = 0; i < count; i++)
            {
                float idx = startIndex + i;

                if (layout == enTopologyLayout.Tree)
                {
                    children[i].Position = new Point(
                        nodeParent.Position.X + distance,
                        nodeParent.Position.Y + (int)(idx * spacing)
                    );
                }
                else if (layout == enTopologyLayout.Pyramid)
                {
                    children[i].Position = new Point(
                        nodeParent.Position.X + (int)(idx * spacing),
                        nodeParent.Position.Y + distance
                    );
                }
            }
        }

        #endregion
        #region Public API

        public void BringGraphIntoView()
        {
            if (nodes.Count == 0) return;

            int minX = nodes.Min(n => n.Position.X - n.Size / 2);
            int minY = nodes.Min(n => n.Position.Y - n.Size / 2);

            if (minX >= 0 && minY >= 0)
                return;

            int offsetX = minX < 0 ? -minX + 20 : 0;
            int offsetY = minY < 0 ? -minY + 20 : 0;

            foreach (var n in nodes)
            {
                n.Position = new Point(
                    n.Position.X + offsetX,
                    n.Position.Y + offsetY
                );
            }

            Invalidate();
        }


        /// <summary>
        /// Add node into NetworkView.
        /// </summary>
        /// <param name="szVictimID">Device victim ID.</param>
        /// <param name="szDisplayName">NetworkNode display name.</param>
        /// <param name="status">Victim status.</param>
        /// <returns></returns>
        public NetworkNode AddNode(string szVictimID, string szDisplayName, enMachineStatus status)
        {
            int rightMostX = nodes.Count == 0 ? 0 : nodes.Max(n => n.Position.X);
            int bottomY = nodes.Count == 0 ? 0 : nodes.Max(n => n.Position.Y);
            Point pos = new Point(100, 100);

            switch (_NetworkViewTopoLogy)
            {
                case enTopologyLayout.Tree:
                    pos = new Point(pos.X, bottomY);
                    break;
                case enTopologyLayout.Pyramid:
                    pos = new Point(rightMostX, pos.Y);
                    break;
            }

            if (status == enMachineStatus.Firewall && nodes.Select(x => x.MachineStatus).Where(x => x == enMachineStatus.Firewall).ToList().Count != 0)
            {
                Rectangle rect = GetGraphBounds();

                switch (_NetworkViewTopoLogy)
                {
                    case enTopologyLayout.Tree:
                        pos.Y += rect.Y + rect.Height + 10;
                        break;
                    case enTopologyLayout.Pyramid:
                        pos.X += rect.X + rect.Width + 10;
                        break;
                }
            }

            NetworkNode n = new NetworkNode { szDisplayName = szDisplayName, szVictimID = szVictimID, Position = pos, };
            fnSetMachineStatus(n, status);

            n.Position = GetNonOverlappingPosition(n);
            nodes.Add(n);
            Invalidate();


            return n;
        }

        public NetworkNode AddNode(NetworkNode node) => AddNode(node.szVictimID, node.szDisplayName, node.MachineStatus);

        /// <summary>
        /// Remove node from NetworkView.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(NetworkNode node)
        {
            if (node == null)
                return;

            if (selectedNodeForHighlight == node)
                selectedNodeForHighlight = null;

            if (node.ParentNode == null) //Firewall
            {
                nodes.Remove(node);
                return;
            }
            else
            {
                NetworkNode nodeParent = node.ParentNode;
                NetworkConnection? conn = FindConnection(nodeParent, node);
                if (node.ParentNode != null && node.ParentNode.ChildNodes.Contains(node))
                    node.ParentNode.ChildNodes.Remove(node);

                if (conn == null)
                    return;

                connections.Remove(conn);

                nodes.Remove(node);
            }

            Invalidate();
        }

        /// <summary>
        /// Add connection between two node.
        /// </summary>
        /// <param name="a">Source node.</param>
        /// <param name="b">Destination node.</param>
        public void AddConnection(NetworkNode a, NetworkNode b, bool bIsConnected, enConnectionType enProtocol)
        {
            if (a == null || b == null)
                return;

            int count = connections.Count(c => c.From == a && c.To == b);

            connections.Add(new NetworkConnection
            {
                From = a,
                To = b,
                nOffsetIndex = count,

                bIsConnected = bIsConnected,
                enProtocol = enProtocol,
            });

            a.ChildNodes.Add(b);
            b.ParentNode = a;
            b.nDepth += a.nDepth;

            //Rectangle rect = fnGetNetworkRectangleWithID(a.szVictimID);

            fnLayoutFanOutChildren(a, _NetworkViewTopoLogy);

            Invalidate();
        }

        /// <summary>
        /// Add connection between two node.
        /// </summary>
        /// <param name="conn"></param>
        public void AddConnection(NetworkConnection conn) => AddConnection(conn.From, conn.To, true, conn.enProtocol);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="enProtocol"></param>
        public void SetConnectionState(NetworkConnection conn, clsSqlite.enListenerProtocol enProtocol)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="bIsConnected"></param>
        public void SetConnectionState(NetworkConnection conn, bool bIsConnected)
        {

        }

        /// <summary>
        /// Clear all nodes and connections.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            connections.Clear();
            selectedNodeForHighlight = null;
            Invalidate();
        }

        /// <summary>
        /// Avoiding overlapping.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Point GetNonOverlappingPosition(NetworkNode node)
        {
            const int padding = 5;
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
                        switch (_NetworkViewTopoLogy)
                        {
                            case enTopologyLayout.Tree:
                                newPos.Y += node.Size + padding;
                                break;
                            case enTopologyLayout.Pyramid:
                                newPos.X += node.Size + padding;
                                break;
                        }

                        /*
                        newPos.X += node.Size + padding;
                        newPos.Y += node.Size + padding;
                        */
                        
                        hasCollision = true;
                        break;
                    }
                }

                attempt++;
                
                if (attempt > 1000)
                    break;
            }
            while (hasCollision);

            return newPos;
        }

        /// <summary>
        /// Find node with display name.
        /// </summary>
        /// <param name="szName"></param>
        /// <returns></returns>
        public NetworkNode? FindNodeWithName(string szName)
        {
            foreach (var node in nodes)
                if (string.Equals(node.szDisplayName, szName))
                    return node;

            return null;
        }

        /// <summary>
        /// Find node with victim's ID.
        /// </summary>
        /// <param name="szID"></param>
        /// <returns></returns>
        public NetworkNode? FindNodeWithID(string szID)
        {
            foreach (var node in nodes)
            {
                if (string.Equals(node.szVictimID, szID))
                    return node;
            }

            return null;
        }

        /// <summary>
        /// Find connection with two end nodes.
        /// </summary>
        /// <param name="srcNode">Soruce node.</param>
        /// <param name="dstNode">Destination node.</param>
        /// <param name="bDirected">Directed edge.</param>
        /// <returns></returns>
        public NetworkConnection? FindConnection(NetworkNode srcNode, NetworkNode dstNode, bool bDirected = true)
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

        /// <summary>
        /// Set victim machine's status.
        /// </summary>
        /// <param name="node">Specified node.</param>
        /// <param name="status">Machine's status.</param>
        public void fnSetMachineStatus(NetworkNode node, enMachineStatus status)
        {
            node.Icon = imageList.Images[Enum.GetName(status)];
            node.MachineStatus = status;

            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void MoveNodeToLeft(NetworkNode node)
        {
            if (node == null || nodes.Count == 0)
                return;

            int leftMostX = nodes.Min(n => n.Position.X);

            node.Position = new Point(leftMostX - 200, node.Position.Y);

            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layout"></param>
        public void fnSetTopology(enTopologyLayout layout)
        {
            if (layout == enTopologyLayout.Tree)
            {

            }
        }

        #endregion

        private void NetworkView_Load(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NetworkNode
    {
        public string szDisplayName; //Node's name.
        public string szVictimID;  //Host's ID.
        public Point Position; //Node's position.
        public Image Icon;     //Node's icon.

        public int Size = 100;
        public Rectangle Bounds => new Rectangle(Position.X - Size / 2, Position.Y - Size / 2, Size, Size);

        public int nDepth = 0;
        public HashSet<NetworkNode> ChildNodes = new HashSet<NetworkNode>();
        public NetworkNode ParentNode = null;

        public NetworkView.enMachineStatus MachineStatus { get; set; }
        public clsAgent Agent;

        public bool bIsLinux
        {
            get
            {
                switch (MachineStatus)
                {
                    case NetworkView.enMachineStatus.Linux_Normal:
                        return true;
                    case NetworkView.enMachineStatus.Linux_Infected:
                        return true;
                    case NetworkView.enMachineStatus.Linux_Super:
                        return true;
                    case NetworkView.enMachineStatus.Linux_Beacon:
                        return true;
                    default:
                        return false;
                }
            }
        }
        public bool bIsWindows
        {
            get
            {
                switch (MachineStatus)
                {
                    case NetworkView.enMachineStatus.Windows_Normal:
                        return true;
                    case NetworkView.enMachineStatus.Windows_Infected:
                        return true;
                    case NetworkView.enMachineStatus.Windows_Super:
                        return true;
                    case NetworkView.enMachineStatus.Windows_Beacon:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NetworkConnection
    {
        public NetworkNode From; //Source node.
        public NetworkNode To; //Destination node.
        public int nOffsetIndex;

        public NetworkView.enConnectionType enProtocol;
        public bool bIsConnected;
    }
}
