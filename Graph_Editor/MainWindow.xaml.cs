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
        private Color selectedColor = Colors.Black; // предположим, что управление цветом есть
        private Polyline? currentStroke;
        private bool isFreeDrawing = false;
        private string? currentFilePath = null;
        private bool isDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Изменение текущего инструмента
        private void ChangeTool(Tool tool)
        {
            currentTool = tool;
            isFreeDrawing = (tool == Tool.FreeDraw);
        }

        // Обработчики кнопок инструмента
        private void LineToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Line);
        private void RectToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Rectangle);
        private void EllipseToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Ellipse);
        private void TriangleToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Triangle);
        private void CursorToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Cursor);
        private void FreeDrawToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.FreeDraw);

        // Логика рисования
        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            startPoint = e.GetPosition(DrawCanvas);
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
                            FillColor = Colors.Transparent,
                            StrokeThickness = 2,
                            IsFilled = false
                        };
                        break;
                    case Tool.Ellipse:
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
                        break;
                    case Tool.Triangle:
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
                        break;
                    default:
                        tempShape = null;
                        break;
                }
                RedrawCanvas(); // показать начальную форму
            }
        }

        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);
            CursorPositionText.Text = $"Координаты курсора: {pos.X}, {pos.Y}";

            if (isDrawing)
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
            if (!isDrawing) return;
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

        private void RedrawCanvas()
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in shapes)
                shape.Draw(DrawCanvas);

            if (tempShape != null)
                tempShape.Draw(DrawCanvas);
        }

        // Меню Файл
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
            // Пример, если появилась палитра, вызов вашего ColorPickerWindow
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

    // Класс для свободного рисования (карандаш)
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
    }
}
