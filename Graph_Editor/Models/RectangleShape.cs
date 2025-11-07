using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class RectangleShape : ShapeBase
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public override void Draw(Canvas canvas)
        {
            Rectangle rect = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(rect, Position.X);
            Canvas.SetTop(rect, Position.Y);
            canvas.Children.Add(rect);
        }

        public override bool ContainsPoint(Point point)
        {
            return point.X >= Position.X && point.X <= Position.X + Width &&
                   point.Y >= Position.Y && point.Y <= Position.Y + Height;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }
    }
}
