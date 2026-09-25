using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Vampire_Survivors.UI
{
    // Organic main-menu logo animation (random target rotation, scale, position drift).
    // Calculates motion strictly relative to stored base bounds and source bitmap to prevent drift.
    internal sealed class LogoAnimator : IDisposable
    {
        // Base on-screen scale relative to the 128x64 source bitmap (384x192).
        public const int DisplayScale = 3;

        private struct LogoState
        {
            public float Angle;       // -5° to +5°
            public float Scale;       // 0.96 to 1.05
            public float OffsetX;     // -8 to +8 px
            public float OffsetY;     // -6 to +6 px
        }

        private readonly PictureBox pictureBox;
        // Borrowed from Properties.Resources. Never disposed here.
        private readonly Bitmap source;
        private readonly Rectangle baseBounds;
        private readonly Random random = new();

        private LogoState startState;
        private LogoState targetState;
        private double transitionDuration;
        private double transitionElapsed;

        // Owned. Disposed when replaced, restored, or disposed.
        private Bitmap? currentFrame;
        private bool isDisposed;

        public LogoAnimator(PictureBox pictureBox, Bitmap source)
        {
            this.pictureBox = pictureBox;
            this.source = source;
            baseBounds = new Rectangle(pictureBox.Left, pictureBox.Top, pictureBox.Width, pictureBox.Height);

            startState = new LogoState
            {
                Angle = 0f,
                Scale = 1.0f,
                OffsetX = 0f,
                OffsetY = 0f
            };

            targetState = GenerateRandomTarget(startState);
            transitionDuration = PickRandomDuration();
            transitionElapsed = 0.0;
        }

        private double PickRandomDuration()
        {
            // 800ms to 2200ms (0.8s to 2.2s)
            return 0.8 + random.NextDouble() * 1.4;
        }

        private LogoState GenerateRandomTarget(LogoState current)
        {
            // Pick random target angle between -5.0° and +5.0° with noticeable change
            float angle;
            int attempts = 0;
            do
            {
                angle = (float)(-5.0 + random.NextDouble() * 10.0);
                attempts++;
            } while (Math.Abs(angle - current.Angle) < 1.5f && attempts < 10);

            // Random scale between 0.96 and 1.05
            float scale = (float)(0.96 + random.NextDouble() * (1.05 - 0.96));

            // Random subtle drift: X between -8.0 and +8.0, Y between -6.0 and +6.0
            float offsetX = (float)(-8.0 + random.NextDouble() * 16.0);
            float offsetY = (float)(-6.0 + random.NextDouble() * 12.0);

            return new LogoState
            {
                Angle = angle,
                Scale = scale,
                OffsetX = offsetX,
                OffsetY = offsetY
            };
        }

        // Quintic smoothstep: zero 1st and 2nd derivative at boundaries for organic ease-in-out
        private static float QuinticSmoothStep(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        public void Update(double deltaSeconds)
        {
            if (isDisposed)
                return;

            transitionElapsed += deltaSeconds;

            if (transitionElapsed >= transitionDuration)
            {
                startState = targetState;
                targetState = GenerateRandomTarget(startState);
                transitionDuration = PickRandomDuration();
                transitionElapsed = 0.0;
            }

            float t = (float)(transitionElapsed / transitionDuration);
            float ease = QuinticSmoothStep(t);

            float currentAngle = startState.Angle + (targetState.Angle - startState.Angle) * ease;
            float currentScale = startState.Scale + (targetState.Scale - startState.Scale) * ease;
            float currentOffsetX = startState.OffsetX + (targetState.OffsetX - startState.OffsetX) * ease;
            float currentOffsetY = startState.OffsetY + (targetState.OffsetY - startState.OffsetY) * ease;

            // Compute scaled dimensions and position relative to baseBounds
            int animW = (int)Math.Round(baseBounds.Width * currentScale);
            int animH = (int)Math.Round(baseBounds.Height * currentScale);
            int animX = baseBounds.X + (int)Math.Round(currentOffsetX) - (animW - baseBounds.Width) / 2;
            int animY = baseBounds.Y + (int)Math.Round(currentOffsetY) - (animH - baseBounds.Height) / 2;

            if (pictureBox.Left != animX || pictureBox.Top != animY || pictureBox.Width != animW || pictureBox.Height != animH)
            {
                pictureBox.SetBounds(animX, animY, animW, animH);
            }

            // Canvas size provides generous margin so +/- 5 deg rotation and scale never clips corners
            Bitmap frame = new Bitmap(animW, animH, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(frame))
            {
                // Crisp nearest-neighbor interpolation for pixel art logo
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                float drawWidth = source.Width * DisplayScale * currentScale;
                float drawHeight = source.Height * DisplayScale * currentScale;

                g.TranslateTransform(animW / 2f, animH / 2f);
                g.RotateTransform(currentAngle);
                g.DrawImage(
                    source,
                    -drawWidth / 2f,
                    -drawHeight / 2f,
                    drawWidth,
                    drawHeight
                );
            }

            Bitmap? previous = currentFrame;
            currentFrame = frame;
            pictureBox.Image = frame;
            previous?.Dispose();
        }

        public void Restore()
        {
            if (currentFrame is null)
                return;

            currentFrame.Dispose();
            currentFrame = null;

            pictureBox.SetBounds(baseBounds.X, baseBounds.Y, baseBounds.Width, baseBounds.Height);
            pictureBox.Image = source;
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            Restore();
        }
    }
}
