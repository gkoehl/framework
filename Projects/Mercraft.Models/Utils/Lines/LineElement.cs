﻿using Mercraft.Core;

namespace Mercraft.Models.Utils.Lines
{
    public class LineElement<T>
    {
        public T Data { get; private set; }

        public float Width { get; private set; }

        public bool IsNotContinuation { get; set; }

        public MapPoint[] Points { get; set; }

        public LineElement(T data, MapPoint[] points, float width)
        {
            Data = data;
            Points = points;
            Width = width;
        }
    }
}
