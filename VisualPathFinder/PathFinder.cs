using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections;

namespace VisualPathFinder
{
    class PathFinder
    {
        private int size, diagonalMoveCost;
        private long runTime;
        private double kvalue;
        private Canvas frame;
        private Node startNode, endNode, par;
        private bool diagonal, running, noPath, complete, trig;
        private List<Node> obstacles, open, closed, path;

        public PathFinder(Canvas _frame, int _size)
        {
            frame = _frame;
            size = _size;

            diagonalMoveCost = (int)(Math.Sqrt(2 * (Math.Pow(size, 2))));
            kvalue = Math.PI / 2;
            diagonal = true;
            trig = false;
            running = false;
            complete = false;

            obstacles = new List<Node>();
            open = new List<Node>();
            closed = new List<Node>();
            path = new List<Node>();
        }

        public void Start(Node start, Node end)
        {
            running = true;
            startNode = start;
            startNode.SetG(0);
            endNode = end;

            // Adding the starting node to the closed list
            AddToList(closed, startNode);

            DateTime jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long startTime = (long) (DateTime.UtcNow - jan1st1970).TotalMilliseconds;

            FindPath(startNode);

            complete = true;
            long endTime = (long)(DateTime.UtcNow - jan1st1970).TotalMilliseconds;
            runTime = endTime - startTime;
            Console.WriteLine("Completed: " + runTime + "ms");
        }

        public void SetStart(Node _startNode)
        {
            startNode = _startNode;
            startNode.SetG(0);
        }

        public void SetEnd(Node _endNode)
        {
            endNode = _endNode;
        }

        public bool IsRunning()
        {
            return running;
        }

        public bool IsComplete()
        {
            return complete;
        }

        public Node GetStart()
        {
            return startNode;
        }

        public Node GetEnd()
        {
            return endNode;
        }

        public Node getPar()
        {
            return par;
        }

        public bool IsNoPath()
        {
            return noPath;
        }

        public bool IsDiagonal()
        {
            return diagonal;
        }

        public bool IsTrig()
        {
            return trig;
        }

        public void SetDiagonal(bool diagonalBool)
        {
            diagonal = diagonalBool;
        }

        public void SetTrig(bool trigBool)
        {
            trig = trigBool;
        }

        public void SetSize(int _size)
        {
            size = _size;
        }

        public async void FindPath(Node parent)
        {
            Node openNode = null;

            if (diagonal)
            {
                // detects and adds one step of nodes to open list
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 1 && j == 1)
                        {
                            continue;
                        }
                        int possibleX = (parent.GetX() - size) + (size * i);
                        int possibleY = (parent.GetY() - size) + (size * j);

                        /* possible coordinates of obstacle
                         * using (crossObstacleX, parent.GetY()) and (parent.GetX(), crossObstacleY())
                         * to see if there are obstacles in the way
                         */
                        int crossObstacleX = parent.GetX() + (possibleX - parent.GetX());
                        int crossObstacleY = parent.GetY() + (possibleY - parent.GetY());

                        // disables ability to cut corners around obstacles
                        if (SearchList(obstacles, crossObstacleX, parent.GetY()) != -1 | SearchList(obstacles, parent.GetX(), crossObstacleY) != -1 && ((j == 0 | j == 2) && i != 1))
                        {
                            continue;
                        }
                        CalculateNodeValues(possibleX, possibleY, openNode, parent);
                    }
                }
            }
            else if (!trig)
            {
                // detects and adds one step of nodes to open list
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if ((i == 0 && j == 0) || (i == 0 && j == 2) || (i == 1 && j == 1) || (i == 2 && j == 0) || (i == 2 && j == 2))
                        {
                            continue;
                        }
                        int possibleX = (parent.GetX() - size) + (size * i);
                        int possibleY = (parent.GetY() - size) + (size * j);

                        CalculateNodeValues(possibleX, possibleY, openNode, parent);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    // uses cosine and sine functions to get circle of points around parent
                    int possibleX = (int)Math.Round(parent.GetX() + (-size * Math.Cos(kvalue * i)));
                    int possibleY = (int)Math.Round(parent.GetY() + (-size * Math.Sin(kvalue * i)));

                    CalculateNodeValues(possibleX, possibleY, openNode, parent);
                }
            }

            // set the new parent node
            parent = LowestFCost();

            if (parent == null)
            {
                MainWindow.AppWindow.modeLabel.Content = "No Path";
                noPath = true;
                running = false;
                MainWindow.AppWindow.paintComponent();
               
                return;
            }

            if (Node.IsEqual(parent, endNode))
            {
                endNode.SetParent(parent.GetParent());

                ConnectPath();
                running = false;
                complete = true;
                MainWindow.AppWindow.paintComponent();

                return;
            }

            // remove parent node from open list
            RemoveFromOpen(parent);
            // add parent node to closed list
            AddToList(closed, parent);

            /*
             * Allows correction for the shortest path during runtime. When new parent Node is selected... checks
             * all adjacent open nodes... then checks if the (G score of parent + open node distance from parent)
             * is less than the current G score of the open node... If true, sets parent of open node as new parent
             * and re-calculates G and F Values
             */ 
             if (diagonal)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 1 && j == 1)
                        {
                            continue;
                        }
                        int possibleX = (parent.GetX() - size) + (size * i);
                        int possibleY = (parent.GetY() - size) + (size * j);
                        Node openCheck = GetOpenNode(possibleX, possibleY);

                        // if spot being looked at is an open node
                        if (openCheck != null)
                        {
                            int xDistance = parent.GetX() - openCheck.GetX();
                            int yDistance = parent.GetY() - openCheck.GetY();
                            int newG = parent.GetG();

                            if (xDistance != 0 && yDistance != 0)
                            {
                                newG += diagonalMoveCost;
                            }
                            else
                            {
                                newG += size;
                            }

                            if (newG < openCheck.GetG())
                            {
                                int s = SearchList(open, possibleX, possibleY);
                                open[s].SetParent(parent);
                                open[s].SetG(newG);
                                open[s].SetF(open[s].GetG() + open[s].GetH());
                            }
                        }
                    }
                }
            }
            if ((bool)MainWindow.AppWindow.stepsCheckbox.IsChecked)
            {
                MainWindow.AppWindow.paintComponent();
                await Task.Delay((int)MainWindow.AppWindow.speedSlider.Value);
                FindPath(parent);
            }
            else
            {
                FindPath(parent);
            }
    }

        public Node GetOpenNode(int x, int y)
        {
            for (int i = 0; i < open.Count; i++)
            {
                if (open[i].GetX() == x && open[i].GetY() == y)
                {
                    return open[i];
                }
            }
            return null;
        }

        public void ConnectPath()
        {
            if (GetPathList().Count == 0)
            {
                Node parentNode = endNode.GetParent();

                while (!Node.IsEqual(parentNode, startNode))
                {
                    AddToList(path, parentNode);

                    for (int i = 0; i < GetClosedList().Count; i++)
                    {
                        Node current = GetClosedList()[i];

                        if (Node.IsEqual(current, parentNode))
                        {
                            parentNode = current.GetParent();
                            break;
                        }
                    }
                }
                Reverse(GetPathList());
            }
        }

        public void Reverse(List<Node> list)
        {
            int j = list.Count - 1;

            for (int i = 0; i < j; i++)
            {
                Node temp = list[i];
                list.RemoveAt(i);
                list.Insert(i, list[j - 1]);
                list.RemoveAt(j);
                list.Insert(j, temp);
                j--;
            }
        }

        public void AddToList(List<Node> list, Node node)
        {
            if (list.Count == 0)
            {
                list.Add(node);
            }
            else if (!CheckListDuplicate(list, node))
            {
                list.Add(node);
            }
        }

        public void AddToObstacle(Node node)
        {
            if (obstacles.Count == 0)
            {
                obstacles.Add(node);
            }
            else if (!CheckListDuplicate(obstacles, node))
            {
                obstacles.Add(node);
            }
        }

        public void RemoveFromOpen(Node node)
        {
            for (int i = 0; i < open.Count; i++)
            {
                if (node.GetX() == open[i].GetX() && node.GetY() == open[i].GetY())
                {
                    open.RemoveAt(i);
                }
            }
        }

        public void RemoveFromObstacles(Node node)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (node.GetX() == open[i].GetX() && node.GetY() == obstacles[i].GetY())
                {
                    obstacles.RemoveAt(i);
                }
            }
        }

        public void RemoveFromObstacles(int location)
        {
            obstacles.RemoveAt(location);
        }

        public Node LowestFCost()
        {
            if (open.Count > 0)
            {
                open.Sort();
                return open[0];
            }
            return null;
        }

        public void CalculateNodeValues(int possibleX, int possibleY, Node openNode, Node parent)
        {
            // if the coordinates are outside of the borders
            // might get the error of NaN for width/height.... may need to use Actualwidth and Actualheight ==> gonna change it to a static width and height...
            if (possibleX < 0 | possibleY < 0 | possibleX >= 500 | possibleY >= 700)
            {
                return;
            }

            // if the node is already a obstacle, closed, or an already open node, then don't make open node
            if (SearchList(obstacles, possibleX, possibleY) != -1 | SearchList(closed, possibleX, possibleY) != -1 | SearchList(open, possibleX, possibleY) != -1)
            {
                return;
            }

            // crete an open node with the available x and y coords
            openNode = new Node(possibleX, possibleY);

            // set the parent of the open node
            openNode.SetParent(parent);

            /* calculating g cost
             * cost to move from parent node to open node (x and y seperately)
             */
            int gXMoveCost = openNode.GetX() - parent.GetX();
            int gYMoveCost = openNode.GetY() - parent.GetY();
            int gCost = parent.GetG();

            if (gXMoveCost != 0 && gYMoveCost != 0)
            {
                gCost += diagonalMoveCost;
            }
            else
            {
                gCost += size;
            }
            openNode.SetG(gCost);

            // calculating H cost
            int hXDiff = Math.Abs(endNode.GetX() - openNode.GetX());
            int hYDiff = Math.Abs(endNode.GetY() - openNode.GetY());
            int hCost = hXDiff + hYDiff;
            openNode.SetH(hCost);

            // calculating F cost
            int fCost = gCost + hCost;
            openNode.SetF(fCost);

            AddToList(open, openNode);
        }

        public bool CheckListDuplicate(List<Node> list, Node node)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (node.GetX() == list[i].GetX() && node.GetY() == list[i].GetY())
                {
                    return true;
                }
            }
            return false;
        }

        public int SearchList(List<Node> list, int xSearch, int ySearch)
        {
            int location = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetX() == xSearch && list[i].GetY() == ySearch)
                {
                    location = i;
                    break;
                }
            }
            return location;
        }

        public int SearchObstacle(int xSearch, int ySearch)
        {
            int location = -1;

            for(int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].GetX() == xSearch && obstacles[i].GetY() == ySearch)
                {
                    location = i;
                    break;
                }
            }
            return location;
        }

        public void Reset()
        {
            while(open.Count > 0)
            {
                open.RemoveAt(0);
            }

            while(closed.Count > 0)
            {
                closed.RemoveAt(0);
            }

            while(path.Count > 0)
            {
                path.RemoveAt(0);
            }
            noPath = false;
            running = false;
            complete = false;
        }

        public long getRunTime()
        {
            return runTime;
        }

        public List<Node> GetPathList()
        {
            return path;
        }

        public List<Node> GetClosedList()
        {
            return closed;
        }

        public List<Node> GetObstacleList()
        {
            return obstacles;
        }

        public List<Node> GetOpenList()
        {
            return open;
        }

        public void PrintList(List<Node> list)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                Console.Write(list[i].GetX() + ", " + list[i].GetY());
                Console.WriteLine();
            }
            Console.WriteLine("===========================");
        }
    }
}