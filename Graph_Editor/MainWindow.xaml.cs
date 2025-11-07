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
using System.Windows.Media.Imaging;

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
            FreeDraw,
            Eraser,
            Text
        }
        private Tool currentTool = Tool.None;
        private Point startPoint;
        private ShapeBase? tempShape;
        private Color selectedColor = Colors.Black;
        private Polyline? currentStroke;
        private bool isFreeDrawing = false;
        private string? currentFilePath = null;
        private bool isDrawing = false;
        private double currentScale = 1.0;

        private Stack<List<ShapeBase>> undoStack = new();
        private Stack<List<ShapeBase>> redoStack = new();

        private ShapeBase? clipboardShape = null;

        private double brushSize = 2.0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveStateForUndo()
        {
            undoStack.Push(CloneShapeList(shapes));
            redoStack.Clear();
        }

        private List<ShapeBase> CloneShapeList(List<ShapeBase> list)
        {
            var clones = new List<ShapeBase>();
            foreach (var s in list)
                clones.Add(s.Clone());
            return clones;
        }

        private void ChangeTool(Tool tool)
        {
            currentTool = tool;
            isFreeDrawing = (tool == Tool.FreeDraw);
            selectedShape = null;
            isMoving = false;
            RedrawCanvas();
        }

        private void LineToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Line);
        private void RectToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Rectangle);
        private void EllipseToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Ellipse);
        private void TriangleToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Triangle);
        private void CursorToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Cursor);
        private void FreeDrawToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.FreeDraw);
        private void EraserToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Eraser);
        private void TextToolBtn_Click(object sender, RoutedEventArgs e) => ChangeTool(Tool.Text);

        private void BrushSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            brushSize = e.NewValue;
        }

        private void ColorPick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Background is SolidColorBrush brush)
                selectedColor = brush.Color;
        }

        private void AddColor_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ColorPickerWindow();
            if (dlg.ShowDialog() == true)
            {
                selectedColor = dlg.SelectedColor;
                var btn = new Button
                {
                    Background = new SolidColorBrush(selectedColor),
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(2),
                };
                btn.Click += ColorPick_Click;
                PalettePanel.Children.Add(btn);
            }
        }

        private void StrokeColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null) return;
            var dlg = new ColorPickerWindow();
            if (dlg.ShowDialog() == true)
            {
                SaveStateForUndo();
                selectedShape.StrokeColor = dlg.SelectedColor;
                RedrawCanvas();
            }
        }

        private void FillColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape == null) return;
            var dlg = new ColorPickerWindow();
            if (dlg.ShowDialog() == true)
            {
                SaveStateForUndo();
                selectedShape.FillColor = dlg.SelectedColor;
                selectedShape.IsFilled = true;
                RedrawCanvas();
            }
        }

        private void FillCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (currentTool == Tool.Cursor && selectedShape != null)
            {
                SaveStateForUndo();
                selectedShape.IsFilled = FillCheckBox.IsChecked == true;
                RedrawCanvas();
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count == 0) return;
            redoStack.Push(CloneShapeList(shapes));
            shapes = undoStack.Pop();
            selectedShape = null;
            RedrawCanvas();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count == 0) return;
            undoStack.Push(CloneShapeList(shapes));
            shapes = redoStack.Pop();
            selectedShape = null;
            RedrawCanvas();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (selectedShape != null)
                clipboardShape = selectedShape.Clone();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (clipboardShape == null) return;
            SaveStateForUndo();
            var pasteShape = clipboardShape.Clone();
            pasteShape.MoveBy(10, 10);
            shapes.Add(pasteShape);
            selectedShape = pasteShape;
            RedrawCanvas();
        }

        private void DeleteSelectedShape()
        {
            if (selectedShape == null) return;
            SaveStateForUndo();
            shapes.Remove(selectedShape);
            selectedShape = null;
            RedrawCanvas();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedShape();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Delete)
                DeleteSelectedShape();
        }

        private void DrawCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);

            // Ластик — как карандаш, только цвет фона
            if (currentTool == Tool.Eraser)
            {
                isDrawing = true;
                Color eraseColor = Colors.White;
                if (DrawCanvas.Background is SolidColorBrush brush)
                    eraseColor = brush.Color;

                currentStroke = new Polyline()
                {
                    Stroke = new SolidColorBrush(eraseColor),
                    StrokeThickness = brushSize,
                    Points = new PointCollection { pos }
                };
                DrawCanvas.Children.Add(currentStroke);
                DrawCanvas.CaptureMouse();
                SaveStateForUndo();
                return;
            }

            if (currentTool == Tool.Text)
            {
                var inputDlg = new TextInputWindow();
                if (inputDlg.ShowDialog() == true)
                {
                    SaveStateForUndo();
                    var textShape = new TextShape()
                    {
                        Position = pos,
                        Text = inputDlg.InputText,
                        StrokeColor = selectedColor,
                        FontSize = brushSize * 5
                    };
                    shapes.Add(textShape);
                    selectedShape = textShape;
                    RedrawCanvas();
                }
                return;
            }

            if (currentTool == Tool.Cursor)
            {
                selectedShape = null;
                for (int i = shapes.Count - 1; i >= 0; i--)
                {
                    if (shapes[i].ContainsPoint(pos))
                    {
                        selectedShape = shapes[i];
                        break;
                    }
                }
                RedrawCanvas();
                if (selectedShape != null)
                {
                    isMoving = true;
                    moveStartPoint = pos;
                    DrawCanvas.CaptureMouse();
                }
            }
            else
            {
                SaveStateForUndo();
                isDrawing = true;
                startPoint = pos;

                if (isFreeDrawing)
                {
                    currentStroke = new Polyline()
                    {
                        Stroke = new SolidColorBrush(selectedColor),
                        StrokeThickness = brushSize,
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
                            tempShape = new LineShape()
                            {
                                Position = startPoint,
                                EndPoint = startPoint,
                                StrokeColor = selectedColor,
                                StrokeThickness = brushSize
                            };
                            break;
                        case Tool.Rectangle:
                            tempShape = new RectangleShape()
                            {
                                Position = startPoint,
                                Width = 0,
                                Height = 0,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = brushSize,
                                IsFilled = isFilled
                            };
                            break;
                        case Tool.Ellipse:
                            tempShape = new EllipseShape()
                            {
                                Position = startPoint,
                                Width = 0,
                                Height = 0,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = brushSize,
                                IsFilled = isFilled
                            };
                            break;
                        case Tool.Triangle:
                            tempShape = new TriangleShape()
                            {
                                Position = startPoint,
                                Point2 = startPoint,
                                Point3 = startPoint,
                                StrokeColor = selectedColor,
                                FillColor = isFilled ? selectedColor : Colors.Transparent,
                                StrokeThickness = brushSize,
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

            if ((currentTool == Tool.Eraser || isFreeDrawing) && isDrawing && e.LeftButton == MouseButtonState.Pressed && currentStroke != null)
            {
                currentStroke.Points.Add(pos);
                return;
            }

            if (isMoving && selectedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = pos.X - moveStartPoint.X;
                double dy = pos.Y - moveStartPoint.Y;
                selectedShape.MoveBy(dx, dy);
                moveStartPoint = pos;
                RedrawCanvas();
            }
            else if (isDrawing && e.LeftButton == MouseButtonState.Pressed && tempShape != null)
            {
                if (tempShape is LineShape line)
                    line.EndPoint = pos;
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
                if ((isFreeDrawing || currentTool == Tool.Eraser) && currentStroke != null)
                {
                    var shape = new FreeDrawShape()
                    {
                        Points = new List<Point>(currentStroke.Points),
                        StrokeColor = (currentTool == Tool.Eraser)
                            ? (DrawCanvas.Background as SolidColorBrush)?.Color ?? Colors.White
                            : selectedColor,
                        StrokeThickness = brushSize
                    };
                    shapes.Add(shape);
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

        private void GridMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            DrawGrid();
            GridCanvas.Visibility = Visibility.Visible;
        }

        private void GridMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            GridCanvas.Children.Clear();
            GridCanvas.Visibility = Visibility.Collapsed;
        }

        private void DrawGrid()
        {
            GridCanvas.Children.Clear();
            GridCanvas.Width = DrawCanvas.Width;
            GridCanvas.Height = DrawCanvas.Height;

            double gridSize = 20;
            double width = GridCanvas.Width;
            double height = GridCanvas.Height;

            for (double x = 0; x <= width; x += gridSize)
                GridCanvas.Children.Add(new Line
                {
                    Stroke = Brushes.LightGray,
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    StrokeThickness = 0.5
                });

            for (double y = 0; y <= height; y += gridSize)
                GridCanvas.Children.Add(new Line
                {
                    Stroke = Brushes.LightGray,
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    StrokeThickness = 0.5
                });
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            currentScale += 0.1;
            ApplyScale();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (currentScale > 0.2)
            {
                currentScale -= 0.1;
                ApplyScale();
            }
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            currentScale = 1.0;
            ApplyScale();
        }

        private void ApplyScale()
        {
            var st = new ScaleTransform(currentScale, currentScale);
            DrawCanvas.LayoutTransform = st;
            GridCanvas.LayoutTransform = st;
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            this.Background = Brushes.DarkSlateGray;
            DrawCanvas.Background = Brushes.Black;
            GridCanvas.Background = Brushes.Black;
        }

        private void DarkTheme_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Background = Brushes.White;
            DrawCanvas.Background = Brushes.White;
            GridCanvas.Background = Brushes.Transparent;
        }

        private void SaveCanvasAsJpeg(string filePath)
        {
            double width = DrawCanvas.ActualWidth;
            double height = DrawCanvas.ActualHeight;

            var rtb = new RenderTargetBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Pbgra32);
            DrawCanvas.Measure(new Size(width, height));
            DrawCanvas.Arrange(new Rect(new Size(width, height)));
            rtb.Render(DrawCanvas);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = new FileStream(filePath, FileMode.Create);
            encoder.Save(fs);
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            SaveStateForUndo();
            shapes.Clear();
            selectedShape = null;
            tempShape = null;
            DrawCanvas.Children.Clear();
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
            var dlg = new SaveFileDialog { Filter = "Graph files|*.graph|JPEG image|*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                if (dlg.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    SaveCanvasAsJpeg(dlg.FileName);
                else
                {
                    currentFilePath = dlg.FileName;
                    SaveProject(currentFilePath);
                }
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Graph files|*.graph" };
            if (dlg.ShowDialog() == true)
            {
                var json = File.ReadAllText(dlg.FileName);
                shapes = JsonSerializer.Deserialize<List<ShapeBase>>(json)!;
                selectedShape = null;
                RedrawCanvas();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void RedrawCanvas()
        {
            DrawCanvas.Children.Clear();
            foreach (var shape in shapes)
                shape.Draw(DrawCanvas);

            if (tempShape != null)
                tempShape.Draw(DrawCanvas);

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

        public class FreeDrawShape : ShapeBase
        {
            public List<Point> Points { get; set; } = new List<Point>();

            public override void Draw(Canvas canvas)
            {
                var polyline = new Polyline
                {
                    Stroke = new SolidColorBrush(StrokeColor),
                    StrokeThickness = StrokeThickness,
                    Points = new PointCollection(Points)
                };
                canvas.Children.Add(polyline);
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

            public override ShapeBase Clone()
            {
                var clone = (FreeDrawShape)this.MemberwiseClone();
                clone.Points = new List<Point>(Points);
                return clone;
            }
        }

        public class TextShape : ShapeBase
        {
            public string Text { get; set; } = "";
            public double FontSize { get; set; } = 12;

            public override void Draw(Canvas canvas)
            {
                var tb = new TextBlock()
                {
                    Text = Text,
                    Foreground = new SolidColorBrush(StrokeColor),
                    FontSize = FontSize
                };
                Canvas.SetLeft(tb, Position.X);
                Canvas.SetTop(tb, Position.Y);
                canvas.Children.Add(tb);
            }

            public override bool ContainsPoint(Point point)
            {
                var rect = new Rect(Position.X, Position.Y, Text.Length * FontSize * 0.5, FontSize * 1.2);
                return rect.Contains(point);
            }

            public override void MoveBy(double dx, double dy)
            {
                Position = new Point(Position.X + dx, Position.Y + dy);
            }

            public override ShapeBase Clone()
            {
                return new TextShape()
                {
                    Position = this.Position,
                    Text = this.Text,
                    StrokeColor = this.StrokeColor,
                    FontSize = this.FontSize,
                    StrokeThickness = this.StrokeThickness,
                    FillColor = this.FillColor,
                    IsFilled = this.IsFilled
                };
            }
        }
    }
}
