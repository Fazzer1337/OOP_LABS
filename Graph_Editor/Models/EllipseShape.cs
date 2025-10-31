using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace GraphEditor.Models
{
    public class EllipseShape : ShapeBase
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }

        public override void Draw(Canvas canvas)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = this.Width,
                Height = this.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(ellipse, Position.X);
            Canvas.SetTop(ellipse, Position.Y);
            canvas.Children.Add(ellipse);
        }
    }
}
