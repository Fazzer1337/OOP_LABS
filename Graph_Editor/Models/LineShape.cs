using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.Models
{
    public class LineShape : ShapeBase
    {
        public Point EndPoint { get; set; }

        public override void Draw(System.Windows.Controls.Canvas canvas)
        {
            var line = new Line()
            {
                X1 = Position.X,
                Y1 = Position.Y,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness
            };

            // Для линии используется центр середина
            double centerX = (Position.X + EndPoint.X) / 2;
            double centerY = (Position.Y + EndPoint.Y) / 2;

            line.RenderTransform = new RotateTransform(RotationAngle, centerX, centerY);

            canvas.Children.Add(line);
        }

        public override bool ContainsPoint(Point point)
        {
            // Для упрощения проверим расстояние от точки до линии с запасом StrokeThickness
            double dist = DistancePointToSegment(point, Position, EndPoint);
            return dist <= StrokeThickness + 3; // с некоторым запасом
        }

        private double DistancePointToSegment(Point p, Point v, Point w)
        {
            double l2 = (w.X - v.X) * (w.X - v.X) + (w.Y - v.Y) * (w.Y - v.Y);
            if (l2 == 0.0) return Distance(p, v);
            double t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            return Distance(p, new Point(v.X + t * (w.X - v.X), v.Y + t * (w.Y - v.Y)));
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
            EndPoint = new Point(EndPoint.X + dx, EndPoint.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new LineShape()
            {
                Position = this.Position,
                EndPoint = this.EndPoint,
                StrokeColor = this.StrokeColor,
                FillColor = this.FillColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled,
                RotationAngle = this.RotationAngle
            };
        }
    }
}

