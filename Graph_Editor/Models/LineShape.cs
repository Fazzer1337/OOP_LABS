using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

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
    }
}
