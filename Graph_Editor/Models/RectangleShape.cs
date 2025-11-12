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

        public override void Draw(Canvas canvas)
        {
            var rect = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(RotationAngle)
            };

            Canvas.SetLeft(rect, Position.X);
            Canvas.SetTop(rect, Position.Y);

            canvas.Children.Add(rect);
        }


        public override bool ContainsPoint(Point point)
        {
            var center = new Point(Position.X + Width / 2, Position.Y + Height / 2);
            var angleRad = -RotationAngle * System.Math.PI / 180.0;
            var sin = System.Math.Sin(angleRad);
            var cos = System.Math.Cos(angleRad);

            var dx = point.X - center.X;
            var dy = point.Y - center.Y;

            var x = cos * dx - sin * dy + center.X;
            var y = sin * dx + cos * dy + center.Y;

            var rect = new Rect(Position.X, Position.Y, Width, Height);
            return rect.Contains(new Point(x, y));
        }

        public override void MoveBy(double dx, double dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }

        public override ShapeBase Clone()
        {
            return new RectangleShape()
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
