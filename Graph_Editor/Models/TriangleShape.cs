using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class TriangleShape : ShapeBase
    {
        public Point Point2 { get; set; }
        public Point Point3 { get; set; }

        public override void Draw(System.Windows.Controls.Canvas canvas)
        {
            var polygon = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };

            var points = new PointCollection
            {
                Position,
                Point2,
                Point3
            };

            // Центр треугольника (центр масс)
            Point center = new Point(
                (Position.X + Point2.X + Point3.X) / 3,
                (Position.Y + Point2.Y + Point3.Y) / 3);

            var rotatedPoints = new PointCollection();
            double angleRad = RotationAngle * System.Math.PI / 180.0;
            double cos = System.Math.Cos(angleRad);
            double sin = System.Math.Sin(angleRad);
            foreach (var p in points)
            {
                double dx = p.X - center.X;
                double dy = p.Y - center.Y;
                double rx = cos * dx - sin * dy + center.X;
                double ry = sin * dx + cos * dy + center.Y;
                rotatedPoints.Add(new Point(rx, ry));
            }

            polygon.Points = rotatedPoints;
            canvas.Children.Add(polygon);
        }

        public override bool ContainsPoint(Point point)
        {
            // Центр масс для обратного вращения
            Point center = new Point(
                (Position.X + Point2.X + Point3.X) / 3,
                (Position.Y + Point2.Y + Point3.Y) / 3);

            double angleRad = -RotationAngle * System.Math.PI / 180.0;
            double cos = System.Math.Cos(angleRad);
            double sin = System.Math.Sin(angleRad);
            double dx = point.X - center.X;
            double dy = point.Y - center.Y;
            double x = cos * dx - sin * dy + center.X;
            double y = sin * dx + cos * dy + center.Y;

            return PointInTriangle(new Point(x, y), Position, Point2, Point3);
        }

        private bool PointInTriangle(Point p, Point p0, Point p1, Point p2)
        {
            double dX = p.X - p2.X;
            double dY = p.Y - p2.Y;
            double dX21 = p2.X - p1.X;
            double dY12 = p1.Y - p2.Y;
            double D = dY12 * (p0.X - p2.X) + dX21 * (p0.Y - p2.Y);
            double s = dY12 * dX + dX21 * dY;
            double t = (p2.Y - p0.Y) * dX + (p0.X - p2.X) * dY;
            if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
            return s >= 0 && t >= 0 && s + t <= D;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            Point2 = new Point(Point2.X + dx, Point2.Y + dy);
            Point3 = new Point(Point3.X + dx, Point3.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new TriangleShape()
            {
                Position = this.Position,
                Point2 = this.Point2,
                Point3 = this.Point3,
                StrokeColor = this.StrokeColor,
                FillColor = this.FillColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled,
                RotationAngle = this.RotationAngle
            };
        }
    }
}
