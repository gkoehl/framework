﻿using System.Collections.Generic;
using ActionStreetMap.Core.Geometry;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Explorer.Scene.Indices;
using ActionStreetMap.Explorer.Utils;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using UnityEngine;

namespace ActionStreetMap.Explorer.Scene.Facades
{
    /// <summary> Creates facade builder for simple facade. </summary>
    internal class FacadeBuilder : IFacadeBuilder
    {
        protected const string LogCategory = "building.facade";

        private readonly IResourceProvider _resourceProvider;

        /// <inheritdoc />
        public string Name { get { return "default"; } }

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="FacadeBuilder"/>. </summary>
        [Dependency]
        public FacadeBuilder(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        /// <inheritdoc />
        public List<MeshData> Build(Building building)
        {
            var random = new System.Random((int)building.Id);
            var footprint = building.Footprint;
            var elevation = building.MinHeight + building.Elevation;
            var gradient = _resourceProvider.GetGradient(building.FacadeColor);

            float height = building.Levels > 1
                ? building.Height / building.Levels
                : random.NextFloat(5f, 6f);

            var emptyWallBuilder = new EmptyWallBuilder()
                .SetGradient(gradient)
                .SetMinHeight(elevation)
                .SetStepHeight(height)
                .SetStepWidth(random.NextFloat(10f, 12f))
                .SetHeight(building.Height);

            int limitIndex;
            var vertCount = CalculateVertexCount(emptyWallBuilder, building.Footprint,
                elevation, 0, out limitIndex);
            var twoSizedMeshCount = vertCount*2;
            if (limitIndex != building.Footprint.Count)
            {
                Trace.Warn(LogCategory, Strings.MeshHasMaxVertexLimit, building.Id.ToString(),
                    twoSizedMeshCount.ToString());
                var meshDataList = new List<MeshData>(2);
                int startIndex = 0;
                while (startIndex != footprint.Count)
                {
                    meshDataList.Add(BuildMeshData(emptyWallBuilder, footprint, vertCount,
                        elevation, startIndex, limitIndex));

                    startIndex = limitIndex;
                    vertCount = CalculateVertexCount(emptyWallBuilder, building.Footprint,
                        elevation, limitIndex, out limitIndex);
                }

                return meshDataList;
            }

            var meshData = BuildMeshData(emptyWallBuilder, footprint, vertCount, 
                elevation, 0, footprint.Count);

            return new List<MeshData>(1) {meshData};
        }

        private int CalculateVertexCount(EmptyWallBuilder emptyWallBuilder, List<Vector2d> footprint,
            float elevation, int startIndex, out int limitIndex)
        {
            var count = 0;
            limitIndex = footprint.Count;
            for (int i = startIndex; i < footprint.Count; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];

                var startVector = new Vector3((float)start.X, elevation, (float)start.Y);
                var endVector = new Vector3((float)end.X, elevation, (float)end.Y);

                var current = emptyWallBuilder.CalculateVertexCount(startVector, endVector);
                count += current;
                if (count * 2 > Consts.MaxMeshSize)
                {
                    // should break as it's multimesh case
                    limitIndex = i;
                    return count - current;
                }
            }

            return count;
        }

        private MeshData BuildMeshData(EmptyWallBuilder emptyWallBuilder, List<Vector2d> footprint, 
            int vertCount, float elevation, int startIndex, int endIndex)
        {
            var meshIndex = new MultiPlaneMeshIndex(footprint.Count, vertCount);
            var meshData = new MeshData(meshIndex, vertCount);
            emptyWallBuilder.SetMeshData(meshData);

            for (int i = startIndex; i < endIndex; i++)
            {
                var nextIndex = i == (footprint.Count - 1) ? 0 : i + 1;
                var start = footprint[i];
                var end = footprint[nextIndex];

                var startVector = new Vector3((float)start.X, elevation, (float)start.Y);
                var endVector = new Vector3((float)end.X, elevation, (float)end.Y);
                var somePointOnPlane = new Vector3((float)end.X, elevation + 10, (float)end.Y);

                meshIndex.AddPlane(startVector, endVector, somePointOnPlane, meshData.NextIndex);
                emptyWallBuilder
                    .SetStartIndex(meshData.NextIndex)
                    .Build(startVector, endVector);
            }

            return meshData;
        }
    }
}