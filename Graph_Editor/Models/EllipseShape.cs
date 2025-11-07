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

        public override void Draw(Canvas canvas)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(ellipse, Position.X);
            Canvas.SetTop(ellipse, Position.Y);
            canvas.Children.Add(ellipse);
        }

        public override bool ContainsPoint(Point point)
        {
            double rx = Width / 2;
            double ry = Height / 2;
            double cx = Position.X + rx;
            double cy = Position.Y + ry;
            double norm = Math.Pow((point.X - cx) / rx, 2) + Math.Pow((point.Y - cy) / ry, 2);
            return norm <= 1.0;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }
    }
}
