using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct GridPos : IEquatable<GridPos>
{
    public readonly int X;
    public readonly int Y;

    public GridPos(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static GridPos operator +(GridPos p, Vector2Int d) => new GridPos(p.X + d.x, p.Y + d.y);

    public bool Equals(GridPos other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is GridPos other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

public class GridLevelData
{
    public HashSet<GridPos> Cells;
    public HashSet<GridPos> Obstacles;
    public HashSet<GridPos> Solution;
    public int MinLights;
    public int TargetLights;
    public int Seed;
}

public static class GridLightLogic
{
    public static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    public static List<GridPos> RayCells(GridPos start, Vector2Int dir, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var outCells = new List<GridPos>();
        var cur = new GridPos(start.X + dir.x, start.Y + dir.y);
        while (cells.Contains(cur))
        {
            if (obstacles.Contains(cur)) break;
            outCells.Add(cur);
            cur = new GridPos(cur.X + dir.x, cur.Y + dir.y);
        }
        return outCells;
    }

    public static int RayLength(GridPos start, Vector2Int dir, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        return RayCells(start, dir, cells, obstacles).Count;
    }

    public static HashSet<GridPos> GetIlluminated(HashSet<GridPos> lights, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var lit = new HashSet<GridPos>();
        foreach (var l in lights)
        {
            lit.Add(l);
            foreach (var d in Dirs)
            {
                foreach (var c in RayCells(l, d, cells, obstacles))
                {
                    lit.Add(c);
                }
            }
        }
        return lit;
    }

    public static bool PairSeesEachOther(GridPos a, GridPos b, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        if (a.X != b.X && a.Y != b.Y) return false;
        var dx = Math.Sign(b.X - a.X);
        var dy = Math.Sign(b.Y - a.Y);
        var cur = new GridPos(a.X + dx, a.Y + dy);
        while (!cur.Equals(b))
        {
            if (!cells.Contains(cur) || obstacles.Contains(cur)) return false;
            cur = new GridPos(cur.X + dx, cur.Y + dy);
        }
        return true;
    }

    public static bool LightsSeeEachOther(HashSet<GridPos> lights, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var arr = lights.ToArray();
        for (var i = 0; i < arr.Length; i++)
        {
            for (var j = i + 1; j < arr.Length; j++)
            {
                if (PairSeesEachOther(arr[i], arr[j], cells, obstacles)) return true;
            }
        }
        return false;
    }

    public static List<GridPos> LightableCells(HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        return cells.Where(c => !obstacles.Contains(c)).ToList();
    }

    public static bool IsValidPlacement(HashSet<GridPos> lights, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        foreach (var l in lights)
        {
            if (obstacles.Contains(l)) return false;
        }
        if (LightsSeeEachOther(lights, cells, obstacles)) return false;
        var lit = GetIlluminated(lights, cells, obstacles);
        return LightableCells(cells, obstacles).All(lit.Contains);
    }

    public static (int minX, int maxX, int minY, int maxY) Bounds(HashSet<GridPos> cells)
    {
        var minX = cells.Min(p => p.X);
        var maxX = cells.Max(p => p.X);
        var minY = cells.Min(p => p.Y);
        var maxY = cells.Max(p => p.Y);
        return (minX, maxX, minY, maxY);
    }

    public static HashSet<GridPos> BuildConflictLights(HashSet<GridPos> playerLights, HashSet<GridPos> cells, HashSet<GridPos> obstacles)
    {
        var conflicts = new HashSet<GridPos>();
        var list = playerLights.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            for (var j = i + 1; j < list.Count; j++)
            {
                if (PairSeesEachOther(list[i], list[j], cells, obstacles))
                {
                    conflicts.Add(list[i]);
                    conflicts.Add(list[j]);
                }
            }
        }
        return conflicts;
    }
}
