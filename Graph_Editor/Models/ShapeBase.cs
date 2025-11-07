using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace GraphEditor.Models
{
    public abstract class ShapeBase
    {
        public Point Position { get; set; }
        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public bool IsFilled { get; set; } = false;
        public double StrokeThickness { get; set; } = 2;

        public abstract void Draw(Canvas c);
        public abstract bool ContainsPoint(Point point);
        public abstract void MoveBy(double dx, double dy);
        public abstract ShapeBase Clone();
    }
}
