using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace GraphEditor.Models
{
    public class RectangleShape : ShapeBase
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }

        public override void Draw(Canvas canvas)
        {
            Rectangle rect = new Rectangle
            {
                Width = this.Width,
                Height = this.Height,
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null
            };
            Canvas.SetLeft(rect, Position.X);
            Canvas.SetTop(rect, Position.Y);
            canvas.Children.Add(rect);
        }
    }
}
