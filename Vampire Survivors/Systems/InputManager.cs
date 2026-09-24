namespace Vampire_Survivors.Systems
{
    public class InputManager
    {
        public bool MoveUp { get; private set; }
        public bool MoveDown { get; private set; }
        public bool MoveLeft { get; private set; }
        public bool MoveRight { get; private set; }

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }

        public void KeyDown(Keys key)
        {
            if (key == Keys.W)
                MoveUp = true;

            if (key == Keys.S)
                MoveDown = true;

            if (key == Keys.A)
                MoveLeft = true;

            if (key == Keys.D)
                MoveRight = true;
        }

        public void KeyUp(Keys key)
        {
            if (key == Keys.W)
                MoveUp = false;

            if (key == Keys.S)
                MoveDown = false;

            if (key == Keys.A)
                MoveLeft = false;

            if (key == Keys.D)
                MoveRight = false;
        }

        public void SetMouse(float x, float y)
        {
            MouseX = x;
            MouseY = y;
        }

        public void Reset()
        {
            MoveUp = false;
            MoveDown = false;
            MoveLeft = false;
            MoveRight = false;
        }

        public PointF GetMovementDirection()
        {
            float directionX = 0;
            float directionY = 0;

            if (MoveUp)
                directionY -= 1;

            if (MoveDown)
                directionY += 1;

            if (MoveLeft)
                directionX -= 1;

            if (MoveRight)
                directionX += 1;

            if (directionX != 0 && directionY != 0)
            {
                const float diagonal = 0.7071f;

                directionX *= diagonal;
                directionY *= diagonal;
            }

            return new PointF(directionX, directionY);
        }
    }
}
