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

        public override void Draw(Canvas canvas)
        {
            Polygon triangle = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null,
                Points = new PointCollection { Position, Point2, Point3 }
            };
            canvas.Children.Add(triangle);
        }

        public override bool ContainsPoint(Point point)
        {
            // Алгоритм: координаты Баррицентра
            double x = point.X, y = point.Y;
            double x1 = Position.X, y1 = Position.Y;
            double x2 = Point2.X, y2 = Point2.Y;
            double x3 = Point3.X, y3 = Point3.Y;

            double d1 = (x - x2) * (y1 - y2) - (y - y2) * (x1 - x2);
            double d2 = (x - x3) * (y2 - y3) - (y - y3) * (x2 - x3);
            double d3 = (x - x1) * (y3 - y1) - (y - y1) * (x3 - x1);

            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            Point2 = new Point(Point2.X + dx, Point2.Y + dy);
            Point3 = new Point(Point3.X + dx, Point3.Y + dy);
        }
    }
}
