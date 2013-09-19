using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiamondSquare : MonoBehaviour {
    private float[,] grid;
    int size;
    public Material material;
	// Use this for initialization
	void Start () {
        doTheDiamond(1);
	}

    private bool checkIfArraysEqual(Point[] a1, Point[] a2)
    {        
        foreach (Point p in a1)
        {
            bool each = false;
            foreach (Point o in a2)
            {
                if (p.Equals(o))
                {
                    each = true;
                    break;
                }
            }
            if (!each)
            {
                return false;
            }
        }
        return true;
    }

    private bool pointInRange(Point p)
    {
        return p.X >= 0 && p.X < size && p.Y >= 0 && p.Y < size;
    }

    private void doTheDiamond(int power)
    {
        size = (int)Mathf.Pow(2, power) + 1;
        grid = new float[size, size];
        float roughness = 0.7f;
        float variation = 1.0f;
        List<Point[]> squares = new List<Point[]>();
        // The order is [0]: TopLeft, [1]: TopRight, [2]: BottomRight, [3]: BottomLeft
        squares.Add(new Point[] {
            new Point(0, 0),
            new Point(size - 1, 0),
            new Point(size - 1, size - 1),
            new Point(0, size - 1)
        });
        List<Point[]> diamonds = new List<Point[]>();
        for (int i = 0; i <= power; i++)
        {
            int w = (int)(size / Mathf.Pow(2, i));

            diamonds = new List<Point[]>();
            // Do the diamond step for each square
            foreach (Point[] square in squares)
            {
                Point center = diamondStep(square, variation);
                int x = center.X;
                int y = center.Y;
                // Generate four diamonds from this center
                Point[] d1 = new Point[] {
                    center, 
                    new Point(mod(x - w/2, size), mod(y - w/2, size)),
                    new Point(mod(x ,size), mod(y - w, size)),
                    new Point(mod(x + w/2, size), mod(y - w/2, size))
                };
                Point[] d2 = new Point[] {
                    center, 
                    new Point(mod(x + w/2, size), mod(y - w/2, size)),
                    new Point(mod(x + w, size), mod(y ,size)),
                    new Point(mod(x + w/2, size), mod(y + w/2, size))                   
                };
                Point[] d3 = new Point[] {
                    center, 
                    new Point(mod(x + w/2, size), mod(y + w/2, size)),
                    new Point(mod(x, size), mod(y + w, size)),
                    new Point(mod(x - w/2, size), mod(y + w/2, size))                                     
                };
                Point[] d4 = new Point[] {
                    center, 
                    new Point(mod(x - w/2, size), mod(y + w/2, size)),
                    new Point(mod(x - w, size), mod(y, size)),
                    new Point(mod(x - w/2, size), mod(y - w/2, size))
                };
                
                diamonds.Add(d1);
                diamonds.Add(d2);
                diamonds.Add(d3);
                diamonds.Add(d4);
            }

            squares = new List<Point[]>();
            // Do the square step for each diamond
            foreach (Point[] diamond in diamonds)
            {
                Point center = squareStep(diamond, variation);
            }

            // Update variation
            variation = variation * Mathf.Pow(2, -roughness);
        }

        // Normalize grid values to lie between 0.0 and 1.0
        normalize();

        // Paint it on a texture
        Texture2D map = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                map.SetPixel(x, y, new Color(grid[x, y], 0, 0));
            }
        }

        map.Apply();
        material.mainTexture = map;
    }

    private void normalize()
    {
        float max = int.MinValue, min = int.MaxValue;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float val = grid[x, y];
                if (val > max)
                {
                    max = val;
                }
                if (val < min)
                {
                    min = val;
                }
            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                grid[x, y] = minMaxNormalization(grid[x, y], min, max, 0.0f, 1.0f);
            }
        }
    }

    private float minMaxNormalization(float v, float min, float max, float new_min, float new_max)
    {
        return (v - min) / (max - min) * (new_max - new_min) + new_min;
    }

    private Point diamondStep(Point[] diamond, float variation)
    {
        float val = 0f;
        foreach (Point corner in diamond)
        {
            val += grid[corner.X, corner.Y];
        }
        val /= diamond.Length;
        val += Random.Range(-variation, variation);
        int cx = (diamond[1].X - diamond[0].X) / 2;
        int cy = (diamond[2].Y - diamond[0].Y) / 2;
        grid[cx, cy] = val;
        return new Point(cx, cy);
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private Point squareStep(Point[] square, float variation)
    {
        float val = 0f;
        int left = size * 2, right = -1, top = size * 2, bottom = -1;
        Point leftMost = new Point(), rightMost = new Point(), 
            topMost = new Point(), bottomMost = new Point();
        foreach (Point corner in square)
        {
            print(string.Format("X: {0}, Y: {1}", corner.X, corner.Y));
            val += grid[corner.X, corner.Y];
            if (corner.X < left)
            {
                left = corner.X;
                leftMost = corner;
            }
            if (corner.X > right)
            {
                right = corner.X;
                rightMost = corner;
            }
            if (corner.Y < top)
            {
                top = corner.Y;
                topMost = corner;
            }
            if (corner.Y > bottom)
            {
                bottom = corner.Y;
                bottomMost = corner;
            }
        }
        val /= square.Length;
        val += Random.Range(-variation, variation);
        int cx = 0, cy = 0;
        if (left == 0)
        {
            cx = 0;
            cy = rightMost.Y;
        }
        if (right == size - 1)
        {
            cx = size - 1;
            cy = leftMost.Y;
        }
        if (top == 0)
        {
            cy = 0;
            cx = bottomMost.X;
        }
        if (bottom == size - 1)
        {
            cy = size - 1;
            cx = topMost.X;
        }
        
        grid[cx, cy] = val;
        return new Point(cx, cy);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

public class Point {
    public int X { get; set; }
    public int Y { get; set; }

    public Point() {

    }

    public Point(int x, int y) {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        Point other = obj as Point;
        return this.X == other.X && this.Y == other.Y;
    }
}
