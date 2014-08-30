﻿using System.Collections.Generic;
using System.Linq;
using Mercraft.Core;
using Mercraft.Core.Elevation;
using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;
using Mercraft.Explorer;
using Mercraft.Explorer.Builders;
using Mercraft.Explorer.Helpers;
using Mercraft.Infrastructure.Dependencies;
using Mercraft.Models.Terrain;

namespace Mercraft.Maps.UnitTests.Zones.Stubs
{
    public class TestFlatModelBuilder : ModelBuilder
    {
        public override string Name
        {
            get { return "flat"; }
        }

        [Dependency]
        public TestFlatModelBuilder(IGameObjectFactory gameObjectFactory)
            : base(gameObjectFactory)
        {
        }

        public override IGameObject BuildArea(GeoCoordinate center, HeightMap heightMap, Rule rule, Area area)
        {
            base.BuildArea(center, heightMap, rule, area);
            IGameObject gameObjectWrapper = GameObjectFactory.CreateNew("");
            BuildModel(center, gameObjectWrapper, rule, area.Points.ToList());
            var floor = rule.GetZIndex();
            return gameObjectWrapper;
        }

        public override IGameObject BuildWay(GeoCoordinate center, HeightMap heightMap, Rule rule, Way way)
        {
            base.BuildWay(center, heightMap, rule, way);
            IGameObject gameObjectWrapper = GameObjectFactory.CreateNew("");
            BuildModel(center, gameObjectWrapper, rule, way.Points.ToList());
            var width = rule.GetWidth();
            var zIndex = rule.GetZIndex();
            return gameObjectWrapper;
        }

        private void BuildModel(GeoCoordinate center, IGameObject gameObject, Rule rule,
            IList<GeoCoordinate> coordinates)
        {
        }
    }
}