using GraphEditor.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        private ShapeBase? selectedShape = null;
        private bool isMoving = false;
        private Point moveStartPoint;

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
        private ShapeBase? tempShape;
        private Color selectedColor = Colors.Black;
        private Polyline? currentStroke;
        private bool isFreeDrawing = false;
        private string? currentFilePath = null;
        private bool isDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChangeTool(Tool tool)
        {
            currentTool = tool;
            isFreeDrawing = (tool == Tool.FreeDraw);
            selectedShape = null;
            isMoving = false;
        }

        private void LineToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Line);
        private void RectToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Rectangle);
        private void EllipseToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Ellipse);
        private void TriangleToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Triangle);
        private void CursorToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Cursor);
        private void FreeDrawToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.FreeDraw);

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);

            if (currentTool == Tool.Cursor)
            {
                // Поиск по фигурам сверху вниз
                selectedShape = null;
                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    if (shapes[i].ContainsPoint(pos))
                    {
                        selectedShape = shapes[i];
                        break;
                    }
                }
                if (selectedShape != null)
                {
                    isMoving = true;
                    moveStartPoint = pos;
                    DrawCanvas.CaptureMouse();
                }
            }
            else
            {
                isDrawing = true;
                startPoint = pos;

                if (isFreeDrawing)
                {
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
                    bool isFilled = FillCheckBox.IsChecked == true;
                    switch (currentTool)
                    {
                        case Tool.Line:
                            tempShape = new LineShape
                            {
                                Position = startPoint,
                                EndPoint = startPoint,
                                StrokeColor = selectedColor,
                                StrokeThickness = 2
                            };
                            break;
                        case Tool.Rectangle:
                            tempShape = new RectangleShape
                            {
                                Position = startPoint,
                                Width = 0,
                                Height = 0,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = 2,
                                IsFilled = isFilled
                            };
                            break;
                        case Tool.Ellipse:
                            tempShape = new EllipseShape
                            {
                                Position = startPoint,
                                Width = 0,
                                Height = 0,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = 2,
                                IsFilled = isFilled
                            };
                            break;
                        case Tool.Triangle:
                            tempShape = new TriangleShape
                            {
                                Position = startPoint,
                                Point2 = startPoint,
                                Point3 = startPoint,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = 2,
                                IsFilled = isFilled
                            };
                            break;
                        default:
                            tempShape = null;
                            break;
                    }
                    RedrawCanvas();
                }
            }
        }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);
            CursorPositionText.Text = $"Координаты курсора: {pos.X}, {pos.Y}";

            if (isMoving && selectedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = pos.X - moveStartPoint.X;
                double dy = pos.Y - moveStartPoint.Y;
                selectedShape.MoveBy(dx, dy);
                moveStartPoint = pos;
                RedrawCanvas();
            }
            else if (isDrawing)
            {
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
                        rect.Width = Math.Abs(pos.X - startPoint.X);
                        rect.Height = Math.Abs(pos.Y - startPoint.Y);
                        rect.Position = new Point(Math.Min(pos.X, startPoint.X), Math.Min(pos.Y, startPoint.Y));
                    }
                    else if (tempShape is EllipseShape ellipse)
                    {
                        ellipse.Width = Math.Abs(pos.X - startPoint.X);
                        ellipse.Height = Math.Abs(pos.Y - startPoint.Y);
                        ellipse.Position = new Point(Math.Min(pos.X, startPoint.X), Math.Min(pos.Y, startPoint.Y));
                    }
                    else if (tempShape is TriangleShape triangle)
                    {
                        triangle.Point2 = new Point(pos.X, startPoint.Y);
                        triangle.Point3 = new Point((startPoint.X + pos.X) / 2, pos.Y);
                    }
                    RedrawCanvas();
                }
            }
        }

        private void DrawCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMoving)
            {
                isMoving = false;
                DrawCanvas.ReleaseMouseCapture();
            }
            else if (isDrawing)
            {
                isDrawing = false;

                if (isFreeDrawing && currentStroke != null)
                {
                    var freeDraw = new FreeDrawShape
                    {
                        Points = new List<Point>(currentStroke.Points),
                        StrokeColor = selectedColor,
                        StrokeThickness = 2
                    };
                    shapes.Add(freeDraw);

                    DrawCanvas.ReleaseMouseCapture();
                    currentStroke = null;
                    tempShape = null;
                    RedrawCanvas();
                }
                else if (tempShape != null)
                {
                    shapes.Add(tempShape);
                    tempShape = null;
                    RedrawCanvas();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Delete && selectedShape != null)
            {
                shapes.Remove(selectedShape);
                selectedShape = null;
                RedrawCanvas();
            }
        }

        private void RedrawCanvas()
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in shapes)
                shape.Draw(DrawCanvas);

            if (tempShape != null)
                tempShape.Draw(DrawCanvas);

            // Optional: выделить выбранную фигуру рамкой
            if (selectedShape != null)
            {
                var bounds = GetShapeBounds(selectedShape);
                if (bounds != Rect.Empty)
                {
                    var selectionRect = new Rectangle
                    {
                        Width = bounds.Width,
                        Height = bounds.Height,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 2 }
                    };
                    Canvas.SetLeft(selectionRect, bounds.X);
                    Canvas.SetTop(selectionRect, bounds.Y);
                    DrawCanvas.Children.Add(selectionRect);
                }
            }
        }

        // Получает ограничивающий прямоугольник фигуры (для выделения)
        private Rect GetShapeBounds(ShapeBase shape)
        {
            if (shape is RectangleShape r)
                return new Rect(r.Position.X, r.Position.Y, r.Width, r.Height);
            if (shape is EllipseShape e)
                return new Rect(e.Position.X, e.Position.Y, e.Width, e.Height);
            if (shape is LineShape l)
            {
                double minX = Math.Min(l.Position.X, l.EndPoint.X);
                double minY = Math.Min(l.Position.Y, l.EndPoint.Y);
                double w = Math.Abs(l.Position.X - l.EndPoint.X);
                double h = Math.Abs(l.Position.Y - l.EndPoint.Y);
                return new Rect(minX, minY, w, h);
            }
            if (shape is TriangleShape t)
            {
                double minX = Math.Min(t.Position.X, Math.Min(t.Point2.X, t.Point3.X));
                double minY = Math.Min(t.Position.Y, Math.Min(t.Point2.Y, t.Point3.Y));
                double maxX = Math.Max(t.Position.X, Math.Max(t.Point2.X, t.Point3.X));
                double maxY = Math.Max(t.Position.Y, Math.Max(t.Point2.Y, t.Point3.Y));
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
            if (shape is FreeDrawShape f && f.Points.Count > 0)
            {
                double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                foreach (var p in f.Points)
                {
                    minX = Math.Min(minX, p.X);
                    minY = Math.Min(minY, p.Y);
                    maxX = Math.Max(maxX, p.X);
                    maxY = Math.Max(maxY, p.Y);
                }
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
            return Rect.Empty;
        }

        // Меню Файл -- далее ваш базовый код без изменений

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            shapes.Clear();
            DrawCanvas.Children.Clear();
            tempShape = null;
            currentFilePath = null;
        }

        private void SaveProject(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            var json = JsonSerializer.Serialize(shapes, options);
            File.WriteAllText(filePath, json);
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
                SaveAsProject_Click(sender, e);
            else
                SaveProject(currentFilePath!);
        }

        private void SaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "Graph files|*.graph" };
            if (dlg.ShowDialog() == true)
            {
                currentFilePath = dlg.FileName;
                SaveProject(currentFilePath);
            }
        }

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

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Graph files|*.graph" };
            if (dlg.ShowDialog() == true)
            {
                var json = File.ReadAllText(dlg.FileName);
                shapes = JsonSerializer.Deserialize<List<ShapeBase>>(json)!;
                currentFilePath = dlg.FileName;
                RedrawCanvas();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }

    // Полноценная фигура для свободного рисования
    public class FreeDrawShape : ShapeBase
    {
        public List<Point> Points { get; set; } = new List<Point>();

        public override void Draw(Canvas c)
        {
            var pl = new Polyline
            {
                Stroke = new SolidColorBrush(StrokeColor),
                StrokeThickness = StrokeThickness,
                Points = new PointCollection(Points)
            };
            c.Children.Add(pl);
        }

        public override bool ContainsPoint(Point point)
        {
            foreach (var p in Points)
            {
                if (Math.Abs(point.X - p.X) <= StrokeThickness + 2 &&
                    Math.Abs(point.Y - p.Y) <= StrokeThickness + 2)
                    return true;
            }
            return false;
        }

        public override void MoveBy(double dx, double dy)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Point(Points[i].X + dx, Points[i].Y + dy);
            }
        }
    }
}
