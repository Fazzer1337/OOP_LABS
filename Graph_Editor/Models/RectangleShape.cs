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

        public override void Draw(Canvas c)
        {
            var rect = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(rect, Position.X);
            Canvas.SetTop(rect, Position.Y);
            c.Children.Add(rect);
        }

        public override bool ContainsPoint(Point point)
        {
            var rect = new Rect(Position.X, Position.Y, Width, Height);
            return rect.Contains(point);
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new RectangleShape
            {
                Position = this.Position,
                Width = this.Width,
                Height = this.Height,
                StrokeColor = this.StrokeColor,
                FillColor = this.FillColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled
            };
        }
    }
}
