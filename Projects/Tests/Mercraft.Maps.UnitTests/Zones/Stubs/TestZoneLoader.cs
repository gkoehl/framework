﻿using System.Collections.Generic;
using System.Linq;
using Mercraft.Core;
using Mercraft.Core.MapCss;
using Mercraft.Core.Scene;
using Mercraft.Core.Tiles;
using Mercraft.Core.Zones;
using Mercraft.Infrastructure.Dependencies;

namespace Mercraft.Maps.UnitTests.Zones.Stubs
{
    public class TestZoneLoader : ZoneLoader
    {
        [Dependency]
        public TestZoneLoader(TileProvider tileProvider,IStylesheetProvider stylesheetProvider,  
             IGameObjectBuilder sceneModelVisitor) :
            base(tileProvider, stylesheetProvider, sceneModelVisitor)
        {
        }

        public override void OnGeoPositionChanged(GeoCoordinate position)
        {
            base.OnGeoPositionChanged(position);
        }

        public override void OnMapPositionChanged(UnityEngine.Vector2 position)
        {
            base.OnMapPositionChanged(position);
        }

        public IList<Zone> ZoneCollection
        {
            get
            {
                return Zones.Values.ToList();
            }
        }
    }
}
