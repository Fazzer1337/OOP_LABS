using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GraphEditor.Models
{
    public abstract class ShapeBase
    {
        public Point Position { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public double StrokeThickness { get; set; }
        public bool IsFilled { get; set; }

        public abstract void Draw(Canvas canvas);
        public abstract bool ContainsPoint(Point point);
        public abstract void MoveBy(double dx, double dy);
    }
}
