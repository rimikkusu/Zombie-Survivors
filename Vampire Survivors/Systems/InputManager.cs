namespace Vampire_Survivors.Systems
{
    public class InputManager
    {
        private readonly HashSet<Keys> keysDown = new();

        public bool MoveUp { get; private set; }
        public bool MoveDown { get; private set; }
        public bool MoveLeft { get; private set; }
        public bool MoveRight { get; private set; }

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }
        public bool IsMouseDown { get; set; }

        public bool KeyDown(Keys key)
        {
            bool isNewPress = keysDown.Add(key);

            if (key == Keys.W)
                MoveUp = true;

            if (key == Keys.S)
                MoveDown = true;

            if (key == Keys.A)
                MoveLeft = true;

            if (key == Keys.D)
                MoveRight = true;

            return isNewPress;
        }

        public void KeyUp(Keys key)
        {
            keysDown.Remove(key);

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
            keysDown.Clear();
            ResetMovement();
            IsMouseDown = false;
        }

        public void ResetMovement()
        {
            MoveUp = false;
            MoveDown = false;
            MoveLeft = false;
            MoveRight = false;
        }

        public static bool TryGetAbilitySlot(Keys key, out int zeroBasedSlot)
        {
            if (key >= Keys.D1 && key <= Keys.D9)
            {
                zeroBasedSlot = (int)key - (int)Keys.D1;
                return true;
            }

            if (key >= Keys.NumPad1 && key <= Keys.NumPad9)
            {
                zeroBasedSlot = (int)key - (int)Keys.NumPad1;
                return true;
            }

            zeroBasedSlot = -1;
            return false;
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
