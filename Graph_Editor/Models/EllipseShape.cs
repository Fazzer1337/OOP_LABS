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
            var ellipse = new Ellipse
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(RotationAngle)
            };

            Canvas.SetLeft(ellipse, Position.X);
            Canvas.SetTop(ellipse, Position.Y);

            canvas.Children.Add(ellipse);
        }


        public override bool ContainsPoint(Point point)
        {
            double centerX = Position.X + Width / 2;
            double centerY = Position.Y + Height / 2;
            var angleRad = -RotationAngle * System.Math.PI / 180.0;
            var sin = System.Math.Sin(angleRad);
            var cos = System.Math.Cos(angleRad);

            var dx = point.X - centerX;
            var dy = point.Y - centerY;

            var x = cos * dx - sin * dy + centerX;
            var y = sin * dx + cos * dy + centerY;

            double rx = Width / 2;
            double ry = Height / 2;

            double nx = (x - centerX) / rx;
            double ny = (y - centerY) / ry;

            return nx * nx + ny * ny <= 1.0;
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new EllipseShape()
            {
                Position = this.Position,
                Width = this.Width,
                Height = this.Height,
                StrokeColor = this.StrokeColor,
                FillColor = this.FillColor,
                StrokeThickness = this.StrokeThickness,
                IsFilled = this.IsFilled,
                RotationAngle = this.RotationAngle
            };
        }
    }
}
