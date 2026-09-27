namespace Vampire_Survivors.Systems
{
    public class InputManager
    {
        private readonly HashSet<Keys> keysDown = new();

        public bool MoveUp { get; private set; }
        public bool MoveDown { get; private set; }
        public bool MoveLeft { get; private set; }
        public bool MoveRight { get; private set; }

        public bool KeyDown(Keys key)
        {
            SetMovement(key, true);
            return keysDown.Add(key);
        }

        public void KeyUp(Keys key)
        {
            keysDown.Remove(key);
            SetMovement(key, false);
        }

        private void SetMovement(Keys key, bool pressed)
        {
            switch (key)
            {
                case Keys.W:
                    MoveUp = pressed;
                    break;
                case Keys.S:
                    MoveDown = pressed;
                    break;
                case Keys.A:
                    MoveLeft = pressed;
                    break;
                case Keys.D:
                    MoveRight = pressed;
                    break;
            }
        }

        public void Reset()
        {
            keysDown.Clear();
            ResetMovement();
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
            float directionX = 0f;
            float directionY = 0f;
            if (MoveRight)
                directionX++;
            if (MoveLeft)
                directionX--;
            if (MoveDown)
                directionY++;
            if (MoveUp)
                directionY--;

            if (directionX != 0 && directionY != 0)
            {
                // Diagonal movement is shortened so it is not faster than straight movement.
                const float diagonal = 0.7071f;

                directionX *= diagonal;
                directionY *= diagonal;
            }

            return new PointF(directionX, directionY);
        }
    }
}
