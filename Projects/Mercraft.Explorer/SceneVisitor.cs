﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercraft.Core;
using Mercraft.Core.Algorithms;
using Mercraft.Core.MapCss.Domain;
using Mercraft.Core.Scene;
using Mercraft.Core.Scene.Models;
using Mercraft.Core.Unity;
using Mercraft.Core.World.Roads;
using Mercraft.Explorer.Helpers;
using Mercraft.Maps.Osm.Helpers;
using Mercraft.Models.Areas;
using Mercraft.Models.Terrain;
using UnityEngine;

namespace Mercraft.Explorer
{
    public class SceneVisitor : ISceneVisitor
    {
        private readonly IGameObjectFactory _goFactory;
        private readonly TerrainBuilder _terrainBuilder;
        private readonly IEnumerable<IModelBuilder> _builders;
        private readonly IEnumerable<IModelBehaviour> _behaviours;

        private List<AreaSettings> _areas = new List<AreaSettings>();
        private List<RoadElement> _roadElements = new List<RoadElement>();

        public SceneVisitor(IGameObjectFactory goFactory,
            IEnumerable<IModelBuilder> builders,
            IEnumerable<IModelBehaviour> behaviours)
        {
            _goFactory = goFactory;
            _builders = builders.ToList();
            _behaviours = behaviours.ToList();

            _terrainBuilder = new TerrainBuilder();
        }

        #region ISceneVisitor implementation

        public bool VisitCanvas(GeoCoordinate center, IGameObject parent, Rule rule, Canvas canvas)
        {
            var tile = canvas.Tile;

            var roads = RoadElementComposer.Compose(_roadElements).Select(reList => new Road()
            {
                Elements = reList,
                GameObject = _goFactory.CreateNew(reList.Aggregate(new StringBuilder("road "),
                    (sb, re) => sb.AppendFormat("[{0}] {1}/ ", re.Id, re.Address)).ToString(), parent)
            }).ToArray();

            _terrainBuilder.Build(parent, new TerrainSettings()
            {
                AlphaMapSize = rule.GetAlphaMapSize(),
                CenterPosition = new Vector2(tile.TileMapCenter.X, tile.TileMapCenter.Y),
                TerrainSize = tile.Size,
                TextureParams = rule.GetTextureParams(),
                Areas = _areas,
                Roads = roads
            });

            // NOTE not ideal solution to make class ready for next request
            _areas = new List<AreaSettings>();
            _roadElements = new List<RoadElement>();

            return true;
        }

        public bool VisitArea(GeoCoordinate center, IGameObject parent, Rule rule, Area area)
        {
            if (rule.IsSkipped())
            {
                CreateSkipped(parent, area);
                return true;
            }

            if (rule.IsTerrain())
            {
                _areas.Add(new AreaSettings()
                {
                    ZIndex = rule.GetZIndex(),
                    SplatIndex = rule.GetSplatIndex(),
                    Points = area.Points.Select(p => GeoProjection.ToMapCoordinate(center, p)).ToArray()
                });
                // TODO in future we want to build some special object which will be
                // invisible as it's part of terrain but useful to provide some OSM info
                // which is associated with it
                return false;
            }

            var builder = rule.GetModelBuilder(_builders);
            var gameObjectWrapper = builder.BuildArea(center, rule, area);
            gameObjectWrapper.Name = String.Format("{0} {1}", builder.Name, area);
            gameObjectWrapper.Parent = parent;

            ApplyBehaviour(gameObjectWrapper, rule, area);

            return true;
        }

        public bool VisitWay(GeoCoordinate center, IGameObject parent, Rule rule, Way way)
        {
            if (rule.IsSkipped())
            {
                CreateSkipped(parent, way);
                return true;
            }

            // TODO refactor this: ideally, no special cases should be there
            // NOTE Road should be processed with Terrain as it has dependencies on:
            // 1. its heightmap (so far not important as we have flat map)
            // 2. we should join roads (important)
            if (rule.IsRoad())
            {
                _roadElements.Add(new RoadElement()
                {
                    Id = way.Id,
                    Address = AddressExtractor.Extract(way.Tags),
                    Width = (int)Math.Round(rule.GetWidth() / 2),
                    Points = way.Points.Select(p => GeoProjection.ToMapCoordinate(center, p)).ToArray()
                });
                // this game object should be initialized inside of TerrainBuilder's logic
                return true;
            }

            var builder = rule.GetModelBuilder(_builders);
            var gameObjectWrapper = builder.BuildWay(center, rule, way);
            gameObjectWrapper.Name = String.Format("{0} {1}", builder.Name, way);
            gameObjectWrapper.Parent = parent;

            ApplyBehaviour(gameObjectWrapper, rule, way);

            return true;
        }

        #endregion

        private void CreateSkipped(IGameObject parent, Model model)
        {
            // TODO 
            var skippedGameObject = _goFactory.CreateNew(String.Format("skip {0}", model));
            skippedGameObject.Parent = parent;
        }

        private void ApplyBehaviour(IGameObject target, Rule rule, Model model)
        {
            var behaviour = rule.GetModelBehaviour(_behaviours);
            if (behaviour != null)
                behaviour.Apply(target, model);
        }
    }
}