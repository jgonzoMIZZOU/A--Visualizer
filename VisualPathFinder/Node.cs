using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualPathFinder
{
    public class Node : IComparable<Node>
    {
        private int x, y, g, h, f;
        private Node parent;

        public Node(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        // Default comparer for Node type.
        public int CompareTo(Node compareNode)
        {
            // A null value means that this object is greater.
            if (compareNode == null)
                return 1;

            else
                return GetF().CompareTo(compareNode.GetF());
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public int GetG()
        {
            return g;
        }

        public int GetH()
        {
            return h;
        }

        public int GetF()
        {
            return f;
        }

        public Node GetParent()
        {
            return parent;
        }

        public void SetXY(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public void SetG(int _g)
        {
            g = _g;
        }

        public void SetH(int _h)
        {
            h = _h;
        }

        public void SetF(int _f)
        {
            f = _f;
        }

        public void SetParent(Node _parent)
        {
            parent = _parent;
        }

        public static Boolean IsEqual(Node s, Node e)
        {
            if (s.GetX() == e.GetX() && s.GetY() == e.GetY())
            {
                return true;
            }
            return false;
        }
    }
}