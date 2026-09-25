using System.Drawing;
using System.Windows.Forms;

namespace Vampire_Survivors.UI
{
    // Shared button animator handling smooth hover and press feedback.
    // Calculations are strictly computed from original Designer base bounds to prevent drift.
    internal sealed class MenuAnimator
    {
        private sealed class ButtonState
        {
            public Button Button { get; }
            public Rectangle BaseBounds { get; set; }
            public float CurrentScale { get; set; } = 1.0f;
            public float TargetScale { get; set; } = 1.0f;
            public float CurrentColorProgress { get; set; } = 0.0f;
            public float TargetColorProgress { get; set; } = 0.0f;
            public bool IsHovered { get; set; }
            public bool IsPressed { get; set; }

            public Color NormalBackColor { get; set; }
            public Color HoverBackColor { get; set; }
            public Color PressedBackColor { get; set; }

            public Color NormalBorderColor { get; set; }
            public Color HoverBorderColor { get; set; }
            public Color PressedBorderColor { get; set; }

            public Color NormalForeColor { get; set; }
            public Color HoverForeColor { get; set; }

            public ButtonState(Button button, Rectangle baseBounds)
            {
                Button = button;
                BaseBounds = baseBounds;
            }
        }

        private readonly List<ButtonState> buttonStates = new();

        public void RegisterButton(
            Button button,
            Color? normalBack = null,
            Color? hoverBack = null,
            Color? pressedBack = null,
            Color? normalBorder = null,
            Color? hoverBorder = null,
            Color? pressedBorder = null,
            Color? normalFore = null,
            Color? hoverFore = null)
        {
            button.Cursor = Cursors.Hand;

            Rectangle baseBounds = new Rectangle(button.Left, button.Top, button.Width, button.Height);
            var state = new ButtonState(button, baseBounds)
            {
                NormalBackColor = normalBack ?? Color.FromArgb(28, 28, 32),
                HoverBackColor = hoverBack ?? Color.FromArgb(46, 46, 54),
                PressedBackColor = pressedBack ?? Color.FromArgb(18, 18, 22),

                NormalBorderColor = normalBorder ?? Color.FromArgb(65, 65, 75),
                HoverBorderColor = hoverBorder ?? Color.FromArgb(135, 135, 150),
                PressedBorderColor = pressedBorder ?? Color.FromArgb(48, 48, 56),

                NormalForeColor = normalFore ?? Color.FromArgb(235, 235, 240),
                HoverForeColor = hoverFore ?? Color.FromArgb(255, 255, 255)
            };

            button.MouseEnter += (s, e) =>
            {
                state.IsHovered = true;
                state.TargetScale = 1.03f;
                state.TargetColorProgress = 1.0f;
            };

            button.MouseLeave += (s, e) =>
            {
                state.IsHovered = false;
                state.IsPressed = false;
                state.TargetScale = 1.0f;
                state.TargetColorProgress = 0.0f;
            };

            button.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    state.IsPressed = true;
                    state.TargetScale = 0.98f;
                    state.TargetColorProgress = -0.5f;
                }
            };

            button.MouseUp += (s, e) =>
            {
                if (state.IsPressed)
                {
                    state.IsPressed = false;
                    state.TargetScale = state.IsHovered ? 1.03f : 1.0f;
                    state.TargetColorProgress = state.IsHovered ? 1.0f : 0.0f;
                }
            };

            button.GotFocus += (s, e) =>
            {
                if (!state.IsHovered)
                {
                    state.TargetColorProgress = 0.5f;
                }
            };

            button.LostFocus += (s, e) =>
            {
                if (!state.IsHovered)
                {
                    state.TargetColorProgress = 0.0f;
                }
            };

            buttonStates.Add(state);
            ApplyState(state);
        }

        public void Update(double deltaSeconds)
        {
            // Responsive smooth interpolation
            float speed = 18f;
            float t = 1f - (float)Math.Exp(-speed * deltaSeconds);

            foreach (var state in buttonStates)
            {
                state.CurrentScale += (state.TargetScale - state.CurrentScale) * t;
                state.CurrentColorProgress += (state.TargetColorProgress - state.CurrentColorProgress) * t;

                ApplyState(state);
            }
        }

        public void Reset()
        {
            foreach (var state in buttonStates)
            {
                state.IsHovered = false;
                state.IsPressed = false;
                state.CurrentScale = 1.0f;
                state.TargetScale = 1.0f;
                state.CurrentColorProgress = 0.0f;
                state.TargetColorProgress = 0.0f;
                ApplyState(state);
            }
        }

        private static void ApplyState(ButtonState state)
        {
            // ALWAYS compute bounds relative to original BaseBounds
            int w = (int)Math.Round(state.BaseBounds.Width * state.CurrentScale);
            int h = (int)Math.Round(state.BaseBounds.Height * state.CurrentScale);
            int x = state.BaseBounds.X - (w - state.BaseBounds.Width) / 2;
            int y = state.BaseBounds.Y - (h - state.BaseBounds.Height) / 2;

            if (state.Button.Left != x || state.Button.Top != y || state.Button.Width != w || state.Button.Height != h)
            {
                state.Button.SetBounds(x, y, w, h);
            }

            Color backColor;
            Color borderColor;
            Color foreColor;

            if (state.CurrentColorProgress >= 0f)
            {
                float factor = Math.Clamp(state.CurrentColorProgress, 0f, 1f);
                backColor = Lerp(state.NormalBackColor, state.HoverBackColor, factor);
                borderColor = Lerp(state.NormalBorderColor, state.HoverBorderColor, factor);
                foreColor = Lerp(state.NormalForeColor, state.HoverForeColor, factor);
            }
            else
            {
                float factor = Math.Clamp(-state.CurrentColorProgress * 2f, 0f, 1f);
                backColor = Lerp(state.NormalBackColor, state.PressedBackColor, factor);
                borderColor = Lerp(state.NormalBorderColor, state.PressedBorderColor, factor);
                foreColor = Lerp(state.NormalForeColor, Color.FromArgb(200, 200, 205), factor);
            }

            state.Button.BackColor = backColor;
            state.Button.FlatAppearance.BorderColor = borderColor;
            state.Button.ForeColor = foreColor;
        }

        private static Color Lerp(Color c1, Color c2, float amount)
        {
            int r = (int)Math.Round(c1.R + (c2.R - c1.R) * amount);
            int g = (int)Math.Round(c1.G + (c2.G - c1.G) * amount);
            int b = (int)Math.Round(c1.B + (c2.B - c1.B) * amount);
            return Color.FromArgb(Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
        }
    }
}
