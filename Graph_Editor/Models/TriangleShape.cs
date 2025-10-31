using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;

namespace GraphEditor.Models
{
    public class TriangleShape : ShapeBase
    {
        public Point Point2 { get; set; }
        public Point Point3 { get; set; }
        public bool IsFilled { get; set; }

        public override void Draw(Canvas canvas)
        {
            Polygon triangle = new Polygon
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Fill = IsFilled ? new SolidColorBrush(FillColor) : null,
                Points = new PointCollection {
                    Position, Point2, Point3
                }
            };
            canvas.Children.Add(triangle);
        }
    }
}
