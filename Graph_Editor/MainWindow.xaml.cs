using GraphEditor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor
{
    public partial class MainWindow : Window
    {
        private List<ShapeBase> shapes = new List<ShapeBase>();

        private enum Tool
        {
            None,
            Cursor,
            Line,
            Rectangle,
            Ellipse,
            Triangle,
            FreeDraw
        }
        private Tool currentTool = Tool.None;

        private Point startPoint;
        private ShapeBase tempShape;

        private Color selectedColor = Colors.Black;
        private Polyline currentStroke;
        private bool isFreeDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Инструменты
        private void LineToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Line;
            isFreeDrawing = false;
            tempShape = null;
        }
        private void RectToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Rectangle;
            isFreeDrawing = false;
            tempShape = null;
        }
        private void EllipseToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Ellipse;
            isFreeDrawing = false;
            tempShape = null;
        }
        private void TriangleToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Triangle;
            isFreeDrawing = false;
            tempShape = null;
        }
        private void CursorToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Cursor;
            isFreeDrawing = false;
            tempShape = null;
        }
        private void FreeDrawToolBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.FreeDraw;
            isFreeDrawing = true;
            tempShape = null;
        }

        // Цвета
        private void ColorPick_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Background is SolidColorBrush brush)
            {
                selectedColor = brush.Color;
            }
        }

        private void AddColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorPickerWindow();
            if (colorDialog.ShowDialog() == true)
            {
                selectedColor = colorDialog.SelectedColor;

                var btn = new Button
                {
                    Background = new SolidColorBrush(selectedColor),
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(2)
                };
                btn.Click += ColorPick_Click;

                PalettePanel.Children.Add(btn);
            }
        }

        // Рисование на холсте
        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isFreeDrawing)
            {
                startPoint = e.GetPosition(DrawCanvas);
                currentStroke = new Polyline
                {
                    Stroke = new SolidColorBrush(selectedColor),
                    StrokeThickness = 2,
                    Points = new PointCollection { startPoint }
                };
                DrawCanvas.Children.Add(currentStroke);
                DrawCanvas.CaptureMouse();
            }
            else
            {
                startPoint = e.GetPosition(DrawCanvas);

                if (currentTool == Tool.Line)
                {
                    tempShape = new LineShape
                    {
                        Position = startPoint,
                        EndPoint = startPoint,
                        StrokeColor = selectedColor,
                        StrokeThickness = 2
                    };
                    shapes.Add(tempShape);
                }
                else if (currentTool == Tool.Rectangle)
                {
                    tempShape = new RectangleShape
                    {
                        Position = startPoint,
                        Width = 0,
                        Height = 0,
                        StrokeColor = selectedColor,
                        FillColor = Colors.Transparent,
                        StrokeThickness = 2,
                        IsFilled = false
                    };
                    shapes.Add(tempShape);
                }
                else if (currentTool == Tool.Ellipse)
                {
                    tempShape = new EllipseShape
                    {
                        Position = startPoint,
                        Width = 0,
                        Height = 0,
                        StrokeColor = selectedColor,
                        FillColor = Colors.Transparent,
                        StrokeThickness = 2,
                        IsFilled = false
                    };
                    shapes.Add(tempShape);
                }
                else if (currentTool == Tool.Triangle)
                {
                    tempShape = new TriangleShape
                    {
                        Position = startPoint,
                        Point2 = startPoint,
                        Point3 = startPoint,
                        StrokeColor = selectedColor,
                        FillColor = Colors.Transparent,
                        StrokeThickness = 2,
                        IsFilled = false
                    };
                    shapes.Add(tempShape);
                }
                else
                {
                    tempShape = null;
                }
            }
        }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);
            CursorPositionText.Text = $"Координаты курсора: {pos.X}, {pos.Y}";

            if (isFreeDrawing && e.LeftButton == MouseButtonState.Pressed && currentStroke != null)
            {
                currentStroke.Points.Add(pos);
            }
            else if (e.LeftButton == MouseButtonState.Pressed && tempShape != null)
            {
                if (tempShape is LineShape line)
                {
                    line.EndPoint = pos;
                }
                else if (tempShape is RectangleShape rect)
                {
                    rect.Width = System.Math.Abs(pos.X - startPoint.X);
                    rect.Height = System.Math.Abs(pos.Y - startPoint.Y);
                    rect.Position = new Point(
                        System.Math.Min(pos.X, startPoint.X),
                        System.Math.Min(pos.Y, startPoint.Y));
                }
                else if (tempShape is EllipseShape ellipse)
                {
                    ellipse.Width = System.Math.Abs(pos.X - startPoint.X);
                    ellipse.Height = System.Math.Abs(pos.Y - startPoint.Y);
                    ellipse.Position = new Point(
                        System.Math.Min(pos.X, startPoint.X),
                        System.Math.Min(pos.Y, startPoint.Y));
                }
                else if (tempShape is TriangleShape triangle)
                {
                    triangle.Point2 = new Point(pos.X, startPoint.Y);
                    triangle.Point3 = new Point((startPoint.X + pos.X) / 2, pos.Y);
                }
                RedrawCanvas();
            }
        }

        private void DrawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isFreeDrawing)
            {
                DrawCanvas.ReleaseMouseCapture();
                currentStroke = null;
                tempShape = null;
            }
            else
            {
                tempShape = null;
            }
        }

        private void RedrawCanvas()
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in shapes)
                shape.Draw(DrawCanvas);
        }

        // Обработка меню
        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            shapes.Clear();
            DrawCanvas.Children.Clear();
            tempShape = null;
        }
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Сохранить проект (реализовать позже)");
        }
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Открыть проект (реализовать позже)");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
