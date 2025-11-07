using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class LineShape : ShapeBase
    {
        public Point EndPoint { get; set; }

        public override void Draw(Canvas c)
        {
            var line = new Line
            {
                X1 = Position.X,
                Y1 = Position.Y,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };
            c.Children.Add(line);
        }

        public override bool ContainsPoint(Point point)
        {
            // Проверка, близка ли точка к линии (радиус 5 пикселей)
            double distance = DistancePointToLine(point, Position, EndPoint);
            return distance <= StrokeThickness + 5;
        }

        private double DistancePointToLine(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            if (dx == 0 && dy == 0)
                return (p - a).Length;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            var projection = new Point(a.X + t * dx, a.Y + t * dy);
            return (p - projection).Length;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            EndPoint = new Point(EndPoint.X + dx, EndPoint.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new LineShape
            {
                Position = this.Position,
                EndPoint = this.EndPoint,
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled // обычно для линии false
            };
        }
    }
}
