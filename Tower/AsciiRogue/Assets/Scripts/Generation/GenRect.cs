﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GenRect 
{
    public int MinX;
    public int MaxX;
    public int MinY;
    public int MaxY;

    public int WidthT => MaxX - MinX + 1;
    public int HeightT => MaxY - MinY + 1;


    public GenRect(int minX, int maxX, int minY, int maxY)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }

    public GenRect(GenRect rect)
    {
        MinX = rect.MinX;
        MaxX = rect.MaxX;
        MinY = rect.MinY;
        MaxY = rect.MaxY;
    }

    /// <summary>
    /// Expand the Rect in these directions (negative for shrinking)
    /// </summary>
    /// <param name="dLeft"></param>
    /// <param name="dTop"></param>
    /// <param name="dRight"></param>
    /// <param name="dBot"></param>
    public GenRect Transform(int dLeft, int dTop, int dRight, int dBot)
    {
        MinX = Mathf.Min(MinX - dLeft, MaxX);
        MinY = Mathf.Min(MinY - dTop, MaxY);
        MaxX = Mathf.Max(MaxX + dRight, MinX);
        MaxY = Mathf.Max(MaxY + dBot, MinY);
        return this;
    }

    public GenRect Move(int dx, int dy)
    {
        MinX = MinX + dx;
        MaxX = MaxX + dx;
        MinY = MinY + dy;
        MaxY = MaxY + dy;
        return this;
    }

    public bool Overlaps(GenRect rect)
    {
        return MinX <= rect.MaxX && MaxX >= rect.MinX && MinY <= rect.MaxY && MaxY >= rect.MinY;
    }

    public GenTile[,] ResizeTiles(GenTile[,] original,ref int gx, ref int gy)
    {
        GenTile[,] empty = new GenTile[WidthT, HeightT];

        empty.Place(gx - MinX, gy - MinY, original);

        gx = MinX;
        gy = MinY;

        return empty;
    }
    
    public bool IsInside(GenRect other)
    {
        return other.MinX < MinX && other.MinY < MinY && other.MaxX >MaxX && other.MaxY > MaxY;
    }

    public bool IsEnclosing(GenRect other)
    {
        return MinX < other.MinX && MinY < other.MinY && MaxX > other.MaxX && MaxY > other.MaxY;
    }

    public Vector2Int GetCenter()
    {
        float xC = (MinX + MaxX) / 2f;
        float yC = (MinY + MaxY) / 2f;

        int xA = RNG.Range(0, 2) == 0 ? Mathf.FloorToInt(xC) : Mathf.CeilToInt(xC);
        int yA = RNG.Range(0, 2) == 0 ? Mathf.FloorToInt(yC) : Mathf.CeilToInt(yC);

        return new Vector2Int(xA, yA);
    }

    public GenRect Union(GenRect other)
    {
        GenRect rect =
            new GenRect(
                Mathf.Max(other.MinX, MinX),
                Mathf.Min(other.MaxX, MaxX),
                Mathf.Max(other.MinY, MinY),
                Mathf.Min(other.MaxY, MaxY));
        return rect;
    }


}
