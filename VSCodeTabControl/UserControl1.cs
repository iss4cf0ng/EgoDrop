using System.ComponentModel;

namespace VSCodeTabControlLibrary
{
    [DesignerCategory("Code")]
    public partial class VSCodeTabControl : TabControl
    {
        private const int CloseSize = 14;
        private const int CloseMargin = 6;

        private int hoverIndex = -1;
        private int dragIndex = -1;
        private bool dragging = false;

        private ContextMenuStrip menu;

        public VSCodeTabControl()
        {
            InitializeComponent();

            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.Padding = new Point(25, 3);
            this.SizeMode = TabSizeMode.Normal;

            BuildContextMenu();
        }

        private void BuildContextMenu()
        {
            menu = new ContextMenuStrip();
            menu.Items.Add("Close", null, (s, e) => CloseTab(menu.Tag as TabPage));
            menu.Items.Add("Close Others", null, (s, e) => CloseOthers(menu.Tag as TabPage));
            menu.Items.Add("Close All", null, (s, e) => CloseAll());
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0 || e.Index >= TabPages.Count) return;

            TabPage tab = this.TabPages[e.Index];
            Rectangle tabRect = GetTabRect(e.Index);

            // Draw text
            TextRenderer.DrawText(
                e.Graphics,
                tab.Text,
                this.Font,
                tabRect,
                Color.Black,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // Close button rectangle
            Rectangle closeRect = new Rectangle(
                tabRect.Right - CloseSize - CloseMargin,
                tabRect.Top + (tabRect.Height - CloseSize) / 2,
                CloseSize,
                CloseSize);

            Brush bg = (e.Index == hoverIndex) ? Brushes.LightGray : Brushes.Transparent;
            e.Graphics.FillRectangle(bg, closeRect);

            using var f = new Font(this.Font, FontStyle.Bold);
            e.Graphics.DrawString("×", f, Brushes.Black, closeRect.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            hoverIndex = -1;
            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle rect = GetTabRect(i);
                Rectangle closeRect = new Rectangle(
                    rect.Right - CloseSize - CloseMargin,
                    rect.Top + (rect.Height - CloseSize) / 2,
                    CloseSize,
                    CloseSize);

                if (closeRect.Contains(e.Location))
                {
                    hoverIndex = i;
                    break;
                }
            }
            Invalidate();

            if (dragging && dragIndex >= 0)
            {
                for (int i = 0; i < TabPages.Count; i++)
                {
                    Rectangle rect = GetTabRect(i);
                    if (rect.Contains(e.Location) && i != dragIndex)
                    {
                        TabPage page = TabPages[dragIndex];
                        TabPages.RemoveAt(dragIndex);
                        TabPages.Insert(i, page);
                        dragIndex = i;
                        SelectedTab = page;
                        break;
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle rect = GetTabRect(i);

                // Right-click menu
                if (e.Button == MouseButtons.Right && rect.Contains(e.Location))
                {
                    menu.Tag = TabPages[i];
                    menu.Show(this, e.Location);
                    return;
                }

                // Close button click
                Rectangle closeRect = new Rectangle(
                    rect.Right - CloseSize - CloseMargin,
                    rect.Top + (rect.Height - CloseSize) / 2,
                    CloseSize,
                    CloseSize);

                if (closeRect.Contains(e.Location) && e.Button == MouseButtons.Left)
                {
                    CloseTab(TabPages[i]);
                    return;
                }
            }

            // Drag start
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < TabPages.Count; i++)
                {
                    if (GetTabRect(i).Contains(e.Location))
                    {
                        dragIndex = i;
                        dragging = true;
                        break;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragging = false;
            dragIndex = -1;
        }

        public void CloseTab(TabPage tab)
        {
            if (tab == null) return;
            TabPages.Remove(tab);
        }

        public void CloseOthers(TabPage tab)
        {
            for (int i = TabPages.Count - 1; i >= 0; i--)
            {
                if (TabPages[i] != tab)
                    TabPages.RemoveAt(i);
            }
            SelectedTab = tab;
        }

        public void CloseAll()
        {
            TabPages.Clear();
        }
    }
}
