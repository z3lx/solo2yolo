using UnityEngine;

namespace z3lx.solo2yolo
{
    internal class RectLayout
    {
        public float lineHeight;
        public Rect contentRect;

        private Vector2 _currentPosition;

        public Rect GetNextRect(float width, float height)
        {
            float x = _currentPosition.x;
            float y = _currentPosition.y + ((lineHeight - height) / 2);
            _currentPosition.x += width;
            return new Rect(x, y, width, height);
        }

        public Rect GetNextRect(float width)
        {
            return GetNextRect(width, lineHeight);
        }

        public void NewLine()
        {
            _currentPosition.x = 0;
            _currentPosition.y += lineHeight;
        }
    }
}