using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class EllipseShape : ShapeBase
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public override void Draw(Canvas c)
        {
            var ellipse = new Ellipse
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(ellipse, Position.X);
            Canvas.SetTop(ellipse, Position.Y);
            c.Children.Add(ellipse);
        }

        public override bool ContainsPoint(Point point)
        {
            double cx = Position.X + Width / 2;
            double cy = Position.Y + Height / 2;
            double rx = Width / 2;
            double ry = Height / 2;

            double dx = point.X - cx;
            double dy = point.Y - cy;
            return (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new EllipseShape
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
