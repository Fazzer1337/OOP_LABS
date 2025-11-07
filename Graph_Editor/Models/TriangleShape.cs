using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class TriangleShape : ShapeBase
    {
        public Point Point2 { get; set; }
        public Point Point3 { get; set; }

        public override void Draw(Canvas c)
        {
            var polygon = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null,
                Points = new PointCollection { Position, Point2, Point3 }
            };
            c.Children.Add(polygon);
        }

        public override bool ContainsPoint(Point point)
        {
            // Метод проверки принадлежности точки треугольнику (Алгоритм с барицентрическими координатами)
            var p0 = Position;
            var p1 = Point2;
            var p2 = Point3;

            double area = 0.5 * (-p1.Y * p2.X + p0.Y * (-p1.X + p2.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y);
            double s = 1 / (2 * area) * (p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * point.X + (p0.X - p2.X) * point.Y);
            double t = 1 / (2 * area) * (p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * point.X + (p1.X - p0.X) * point.Y);

            return s >= 0 && t >= 0 && (s + t) <= 1;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            Point2 = new Point(Point2.X + dx, Point2.Y + dy);
            Point3 = new Point(Point3.X + dx, Point3.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new TriangleShape
            {
                Position = this.Position,
                Point2 = this.Point2,
                Point3 = this.Point3,
                StrokeColor = this.StrokeColor,
                FillColor = this.FillColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled
            };
        }
    }
}
