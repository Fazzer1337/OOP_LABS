using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace GraphEditor.Models
{
    public abstract class ShapeBase
    {
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public double StrokeThickness { get; set; }
        public Point Position { get; set; }

        public abstract void Draw(Canvas canvas);
        public virtual void Move(Point newPosition)
        {
            Position = newPosition;
        }
    }
}
