﻿namespace Mercraft.Models.Terrain.Roads
{
    public class RoadSegment
    {
        public Segment Left;
        public Segment Right;

        public RoadSegment(Segment left, Segment right)
        {
            Left = left;
            Right = right;
        }
    }
}