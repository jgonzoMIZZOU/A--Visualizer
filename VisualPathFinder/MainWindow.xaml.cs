using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
//using System.Windows.Media;

namespace VisualPathFinder
{
    /*
     * This is the control handler for the project 
     */
    public partial class MainWindow : Window
    {
        PathFinder pathfinding;
        int size;
        Node startNode, endNode;
        int red = RandomWithRange(0, 255);
        int green = RandomWithRange(0, 255);
        int blue = RandomWithRange(0, 255);
        Color greenhighlight = Color.FromArgb(132, 255, 138);
        Color redHighlight = Color.FromArgb(253, 90, 90);
        Color blueHighlight = Color.FromArgb(32, 233, 255);

        public static MainWindow AppWindow;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            StartFrame();
        }

        public void StartFrame()
        {
            size = 25;
            modeLabel.Content = "Mode: Map Creation";

            // set up pathfinding
            pathfinding = new PathFinder(frame, size);
            if ((bool)stepsCheckbox.IsChecked) // don't know why this is considered an optional type?????????
            {
                pathfinding.SetDiagonal(true);
            }
            else
            {
                pathfinding.SetDiagonal(false);
            }
        }

        private void ShowStepsCheckBox(object sender, RoutedEventArgs e)
        {
            
        }

        private void DiagonalCheckBox(object sender, RoutedEventArgs e)
        {
            pathfinding.SetDiagonal(true);
        }

        private void TrigCheckBox(object sender, RoutedEventArgs e)
        {
            pathfinding.SetTrig(true);
        }

        private void SpeedSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedLabel.Content = "Speed: " + speedSlider.Value;
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            
            // move one step ahead in path 
            if (pathfinding.IsRunning() && (bool)stepsCheckbox.IsChecked)
            {
                pathfinding.FindPath(pathfinding.getPar());
                modeLabel.Content = "Running";
            }

            if (!pathfinding.IsRunning())
            {
                Start();
            } 
            else
            {
                pathfinding.Reset();
            }
            paintComponent();
        }

        private static int RandomWithRange(int min, int max)
        {
            Random r = new Random();
            return r.Next(min, max);
        }

        public void paintComponent()
        {
            double height = 500;
            double width = 700;

            // If pathfinding is complete (found path)
            if (pathfinding.IsComplete())
            {
                // set complete mode
                if ((bool)stepsCheckbox.IsChecked)
                {
                    if (pathfinding.IsNoPath())
                    {
                        modeLabel.Content = "No Path";
                    }
                    else
                    {
                        modeLabel.Content = "Completed in " + pathfinding.getRunTime() + "ms";
                    }
                }

                pathLabel.Content = "Path: " + pathfinding.GetPathList().Count;
                openCLabel.Content = "Open: " + pathfinding.GetOpenList().Count;
                closedLabel.Content = "Closed: " + pathfinding.GetClosedList().Count;
            }

            // draw grid
            for (int i = 0; i < height; i += size)
            {
                for (int j = 0; j < width; j += size)
                {
                    System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                    rect.Stroke = System.Windows.Media.Brushes.Black;
                    rect.StrokeThickness = 1;
                    rect.Width = size;
                    rect.Height = size;
                    Canvas.SetTop(rect, i);
                    Canvas.SetLeft(rect, j);
                    frame.Children.Add(rect);
                }
            }

            // draw all obstacles
            for (int i = 0; i < pathfinding.GetObstacleList().Count; i++)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.Black;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, pathfinding.GetObstacleList()[i].GetX() + 1);
                Canvas.SetLeft(rect, pathfinding.GetObstacleList()[i].GetY() + 1);
                frame.Children.Add(rect);
            }

            // draw all open nodes (path finding nodes)
            for (int i = 0; i < pathfinding.GetOpenList().Count; i++)
            {
                Node current = pathfinding.GetOpenList()[i];
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.LightGreen;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, current.GetX() + 1);
                Canvas.SetLeft(rect, current.GetY() + 1);
                frame.Children.Add(rect);
            }

            // draw all the closed nodes
            for (int i = 0; i < pathfinding.GetClosedList().Count; i++)
            {
                Node current = pathfinding.GetClosedList()[i];
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.PaleVioletRed;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, current.GetX() + 1);
                Canvas.SetLeft(rect, current.GetY() + 1);
                frame.Children.Add(rect);
            }

            // draw all final path nodes
            for (int i = 0; i < pathfinding.GetPathList().Count; i++)
            {
                Node current = pathfinding.GetPathList()[i];
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.LightSkyBlue;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, current.GetX() + 1);
                Canvas.SetLeft(rect, current.GetY() + 1);
                frame.Children.Add(rect);
            }

            // draw start of the path
            if (startNode != null)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.Blue;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, startNode.GetX() + 1);
                Canvas.SetLeft(rect, startNode.GetY() + 1);
                frame.Children.Add(rect);
            }

            // draw the end node
            if (endNode != null)
            {
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = System.Windows.Media.Brushes.Red;
                rect.StrokeThickness = 1;
                rect.Width = size - 2;
                rect.Height = size - 2;
                Canvas.SetTop(rect, endNode.GetX() + 1);
                Canvas.SetLeft(rect, endNode.GetY() + 1);
                frame.Children.Add(rect);
            }
        }
        
        public void MapCalculations(MouseButtonEventArgs e)
        {
            // if left mouse button is clicked
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    // if s is pressed create start node
                    if (Keyboard.IsKeyDown(Key.S))
                    {
                        System.Windows.Point mousePos = e.GetPosition(this);
                        double x = mousePos.X - (mousePos.X % size);
                        double y = mousePos.Y - (mousePos.Y % size);

                        if (startNode == null)
                        {
                            startNode = new Node(Convert.ToInt32(y), Convert.ToInt32(x));
                        } 
                        else
                        {
                            frame.Children.Clear();
                            startNode.SetXY(Convert.ToInt32(y), Convert.ToInt32(x));
                        }
                    }
                    // if e is pressed create end node
                    else if (Keyboard.IsKeyDown(Key.E))
                    {
                        System.Windows.Point mousePos1 = e.GetPosition(this);
                        double x = mousePos1.X - (mousePos1.X % size);
                        double y = mousePos1.Y - (mousePos1.Y % size);

                        if (endNode == null)
                        {
                            endNode = new Node(Convert.ToInt32(y), Convert.ToInt32(x));
                        }
                        else
                        {
                            frame.Children.Clear();
                            endNode.SetXY(Convert.ToInt32(y), Convert.ToInt32(x));
                        }
                    }
                    // otherwise create an obstacle
                    else
                    {
                        System.Windows.Point mousePos2 = e.GetPosition(this);
                        double xObstacle = mousePos2.X - (mousePos2.X % size);
                        double yObstacle = mousePos2.Y - (mousePos2.Y % size);

                        Node newObstacle = new Node(Convert.ToInt32(yObstacle), Convert.ToInt32(xObstacle));
                        pathfinding.AddToObstacle(newObstacle);
                        
                    }
                    paintComponent();
                    break;
                case MouseButton.Right:
                    System.Windows.Point mousePos3 = e.GetPosition(this);
                    double mouseBoxX = mousePos3.X - (mousePos3.X % size);
                    double mouseBoxY = mousePos3.Y - (mousePos3.Y % size);

                    // if s is pressed remove start node
                    if (Keyboard.IsKeyDown(Key.S))
                    {
                        if (startNode != null && mouseBoxX == startNode.GetY() && startNode.GetX() == mouseBoxY)
                        {
                            startNode = null;
                        }
                    }
                    // if e is pressed remove end node
                    else if (Keyboard.IsKeyDown(Key.E))
                    {
                        if (endNode != null && mouseBoxX == endNode.GetY() && endNode.GetX() == mouseBoxY)
                        {
                            endNode = null;
                        }
                    }
                    // otherwise remove all
                    else
                    {
                        int location = pathfinding.SearchObstacle(Convert.ToInt32(mouseBoxY), Convert.ToInt32(mouseBoxX));
                        if (location != -1)
                        {
                            pathfinding.RemoveFromObstacles(location);
                        }
                    }
                    frame.Children.Clear();
                    paintComponent();
                    break;
            }
        }

        private void MouseButtonClicked(object sender, MouseButtonEventArgs e)
        {
            MapCalculations(e);
        }

        private void drawGridButton_Click(object sender, RoutedEventArgs e)
        {
            paintComponent();
        }

        private void ClearGrid(object sender, RoutedEventArgs e)
        {
            pathfinding.Reset();
            frame.Children.Clear();
            paintComponent();
        }

        public void Start()
        {
            if (startNode != null && endNode != null)
            {
                pathfinding.Start(startNode, endNode);
            }
            else
            {
                Console.WriteLine("ERROR: Needs start and end points to run....");
            }
        }
    }
}
