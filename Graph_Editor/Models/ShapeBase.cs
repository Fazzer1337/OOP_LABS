using System.Windows;
using System.Windows.Media;

namespace GraphEditor.Models
{
    public abstract class ShapeBase
    {
        public Point Position { get; set; }
        public Color StrokeColor { get; set; } = Colors.Black;
        public Color FillColor { get; set; } = Colors.Transparent;
        public double StrokeThickness { get; set; } = 2;
        public bool IsFilled { get; set; } = false;

        public double RotationAngle { get; set; } = 0; 

        public abstract void Draw(System.Windows.Controls.Canvas canvas);
        public abstract bool ContainsPoint(Point point);
        public abstract void MoveBy(double dx, double dy);
        public abstract ShapeBase Clone();
    }
}
