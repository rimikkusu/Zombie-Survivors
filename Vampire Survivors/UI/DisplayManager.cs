namespace Vampire_Survivors.UI
{
    public sealed class DisplayManager
    {
        private readonly Form form;
        private readonly ComboBox modeSelector;
        private readonly ScreenController screens;
        private DisplayMode currentMode = DisplayMode.WindowedMaximized;
        private Rectangle savedWindowedBounds;
        private bool isApplyingMode;

        public DisplayManager(Form form, ComboBox modeSelector, ScreenController screens)
        {
            this.form = form;
            this.modeSelector = modeSelector;
            this.screens = screens;
            Rectangle workArea = Screen.FromControl(form).WorkingArea;
            int width = Math.Min(1280, (int)(workArea.Width * 0.85f));
            int height = Math.Min(720, (int)(workArea.Height * 0.85f));
            int x = workArea.X + (workArea.Width - width) / 2;
            int y = workArea.Y + (workArea.Height - height) / 2;
            savedWindowedBounds = new Rectangle(x, y, width, height);
            modeSelector.DrawMode = DrawMode.OwnerDrawFixed;
            modeSelector.ItemHeight = 24;
            modeSelector.DrawItem += DrawItem;
            SyncSelector();
        }

        public void ApplySelectedMode()
        {
            if (modeSelector.SelectedItem is not string selected)
                return;
            DisplayMode mode = selected switch
            {
                "Windowed" => DisplayMode.Windowed,
                "Windowed Maximized" => DisplayMode.WindowedMaximized,
                "Fullscreen" => DisplayMode.Fullscreen,
                _ => currentMode
            };
            Apply(mode);
        }

        public void Apply(DisplayMode mode)
        {
            isApplyingMode = true;
            try
            {
                if (currentMode == DisplayMode.Windowed && form.WindowState == FormWindowState.Normal)
                    savedWindowedBounds = form.Bounds;

                switch (mode)
                {
                    case DisplayMode.Windowed:
                        if (form.WindowState == FormWindowState.Maximized)
                            form.WindowState = FormWindowState.Normal;
                        form.FormBorderStyle = FormBorderStyle.Sizable;
                        form.MaximizeBox = true;
                        Rectangle workArea = Screen.FromControl(form).WorkingArea;
                        int width = Math.Clamp(savedWindowedBounds.Width, 640, workArea.Width);
                        int height = Math.Clamp(savedWindowedBounds.Height, 480, workArea.Height);
                        int x = Math.Clamp(savedWindowedBounds.X, workArea.Left,
                            Math.Max(workArea.Left, workArea.Right - width));
                        int y = Math.Clamp(savedWindowedBounds.Y, workArea.Top,
                            Math.Max(workArea.Top, workArea.Bottom - height));
                        savedWindowedBounds = new Rectangle(x, y, width, height);
                        form.Bounds = savedWindowedBounds;
                        form.WindowState = FormWindowState.Normal;
                        break;
                    case DisplayMode.WindowedMaximized:
                        if (form.FormBorderStyle != FormBorderStyle.Sizable)
                            form.FormBorderStyle = FormBorderStyle.Sizable;
                        form.MaximizeBox = true;
                        if (form.WindowState != FormWindowState.Maximized)
                            form.WindowState = FormWindowState.Maximized;
                        break;
                    case DisplayMode.Fullscreen:
                        if (form.WindowState == FormWindowState.Maximized)
                            form.WindowState = FormWindowState.Normal;
                        form.FormBorderStyle = FormBorderStyle.None;
                        form.WindowState = FormWindowState.Normal;
                        form.Bounds = Screen.FromControl(form).Bounds;
                        break;
                }
                currentMode = mode;
                SyncSelector();
                screens.RecenterLayout();
            }
            finally
            {
                isApplyingMode = false;
            }
        }

        public void HandleSizeChanged()
        {
            if (!isApplyingMode)
            {
                if (form.WindowState == FormWindowState.Maximized && currentMode != DisplayMode.Fullscreen)
                {
                    currentMode = DisplayMode.WindowedMaximized;
                    SyncSelector();
                }
                else if (form.WindowState == FormWindowState.Normal &&
                         currentMode == DisplayMode.WindowedMaximized)
                {
                    currentMode = DisplayMode.Windowed;
                    savedWindowedBounds = form.Bounds;
                    SyncSelector();
                }
                else if (form.WindowState == FormWindowState.Normal && currentMode == DisplayMode.Windowed)
                    savedWindowedBounds = form.Bounds;
            }
            screens.RecenterLayout();
        }

        private void SyncSelector()
        {
            string target = currentMode switch
            {
                DisplayMode.Windowed => "Windowed",
                DisplayMode.WindowedMaximized => "Windowed Maximized",
                DisplayMode.Fullscreen => "Fullscreen",
                _ => "Windowed Maximized"
            };
            if (modeSelector.SelectedItem as string != target)
                modeSelector.SelectedItem = target;
        }

        private void DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color background = selected ? Color.FromArgb(50, 50, 60) : Color.FromArgb(28, 28, 32);
            using (var brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);
            string text = modeSelector.Items[e.Index]?.ToString() ?? string.Empty;
            TextRenderer.DrawText(e.Graphics, text, modeSelector.Font, e.Bounds,
                Color.FromArgb(235, 235, 240),
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
    }
}
