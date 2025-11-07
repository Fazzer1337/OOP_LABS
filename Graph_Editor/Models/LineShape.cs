using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class LineShape : ShapeBase
    {
        public Point EndPoint { get; set; }

        public override void Draw(Canvas canvas)
        {
            Line line = new Line
            {
                X1 = Position.X,
                Y1 = Position.Y,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            canvas.Children.Add(line);
        }

        public override bool ContainsPoint(Point point)
        {
            // Алгоритм: Расстояние от точки до отрезка
            double x1 = Position.X, y1 = Position.Y;
            double x2 = EndPoint.X, y2 = EndPoint.Y;
            double px = point.X, py = point.Y;
            double dx = x2 - x1, dy = y2 - y1;
            double lengthSq = dx * dx + dy * dy;
            if (lengthSq == 0)
                return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1)) <= StrokeThickness + 2;
            double t = ((px - x1) * dx + (py - y1) * dy) / lengthSq;
            t = Math.Max(0, Math.Min(1, t));
            double closestX = x1 + t * dx;
            double closestY = y1 + t * dy;
            double dist = Math.Sqrt((px - closestX) * (px - closestX) + (py - closestY) * (py - closestY));
            return dist <= StrokeThickness + 4;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            EndPoint = new Point(EndPoint.X + dx, EndPoint.Y + dy);
        }
    }
}
