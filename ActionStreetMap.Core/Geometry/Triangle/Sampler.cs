﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionStreetMap.Core.Geometry.Triangle
{
    /// <summary> Used for triangle sampling in the <see cref="TriangleLocator" /> class. </summary>
    internal class Sampler
    {
        // Empirically chosen factor.
        private const int samplefactor = 11;

        private readonly Random rand;

        // Number of random samples for point location (at least 1).
        private int samples = 1;

        // Number of triangles in mesh.
        private int triangleCount;

        // Keys of the triangle dictionary.
        private int[] keys;

        public Sampler()
        {
            rand = new Random(110503);
        }

        /// <summary> Reset the sampler. </summary>
        public void Reset()
        {
            samples = 1;
            triangleCount = 0;
        }

        /// <summary> Update sampling parameters if mesh changed. </summary>
        /// <param name="mesh">Current mesh.</param>
        public void Update(Mesh mesh)
        {
            Update(mesh, false);
        }

        /// <summary> Update sampling parameters if mesh changed. </summary>
        public void Update(Mesh mesh, bool forceUpdate)
        {
            int count = mesh.triangles.Count;

            // TODO: Is checking the triangle count a good way to monitor mesh changes?
            if (triangleCount != count || forceUpdate)
            {
                triangleCount = count;

                // The number of random samples taken is proportional to the cube root of
                // the number of triangles in the mesh.  The next bit of code assumes
                // that the number of triangles increases monotonically (or at least
                // doesn't decrease enough to matter).
                while (samplefactor*samples*samples*samples < count)
                    samples++;

                // TODO: Is there a way not calling ToArray()?
                keys = mesh.triangles.Keys.ToArray();
            }
        }

        /// <summary> Get a random sample set of triangle keys. </summary>
        /// <returns>Array of triangle keys.</returns>
        public List<int> GetSamples(Mesh mesh)
        {
            // TODO: Using currKeys to check key availability?
            List<int> randSamples = new List<int>(samples);

            int range = triangleCount/samples;
            int key;

            for (int i = 0; i < samples; i++)
            {
                // Yeah, rand should be equally distributed, but just to make
                // sure, use a range variable...
                key = rand.Next(i*range, (i + 1)*range - 1);

                if (!mesh.triangles.ContainsKey(keys[key]))
                {
                    // Keys collection isn't up to date anymore!
                    Update(mesh, true);
                    i--;
                }
                else
                    randSamples.Add(keys[key]);
            }

            return randSamples;
        }
    }
}