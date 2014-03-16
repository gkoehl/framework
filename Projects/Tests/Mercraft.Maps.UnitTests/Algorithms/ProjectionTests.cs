﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mercraft.Core.Algorithms;
using Mercraft.Core;
using NUnit.Framework;
using UnityEngine;

namespace Mercraft.Maps.UnitTests.Algorithms
{
    [TestFixture]
    public class ProjectionTests
    {
        private const double Percision = 0.000001;

        /// <summary>
        /// Berlin, Mitte (Eichendorffstraße)
        /// </summary>
        private GeoCoordinate _center = new GeoCoordinate(52.529814, 13.388015);

        /// <summary>
        /// Berlin, Tegel
        /// </summary>
        private GeoCoordinate _target = new GeoCoordinate(52.582922, 13.282957);

        [Test]
        public void CanConvertToMap()
        {
            var mapCoordinate = GeoProjection.ToMapCoordinate(_center, _target);

            Assert.AreEqual(-7114, Math.Truncate(mapCoordinate.x));
            Assert.AreEqual(5902, Math.Truncate(mapCoordinate.y));

            Assert.AreEqual(9244, Math.Truncate(Distance(new Vector2(0, 0), mapCoordinate)));
        }

        [Test]
        public void CanConvertToGeo()
        {
            // Arrange
            var mapCoordinate = GeoProjection.ToMapCoordinate(_center, _target);

            // Act
            var geoCoordinate = GeoProjection.ToGeoCoordinate(_center, mapCoordinate);

            Assert.True(Math.Abs(52.582922 - geoCoordinate.Latitude) < Percision);
            Assert.True(Math.Abs(13.282957 - geoCoordinate.Longitude) < Percision);
        }

        [Test(Description = "Tests correctness of traversal order sorting logic")]
        public void CanReverseVertices()
        {
            var center = new GeoCoordinate(52.529814, 13.388015);
            var geoCoordinates = new List<GeoCoordinate>()
            {
                new GeoCoordinate(52.5295083, 13.3889532),
                new GeoCoordinate(52.5291505, 13.3891865),
                new GeoCoordinate(52.5291244, 13.3891088),
                new GeoCoordinate(52.5291819, 13.389071),
                new GeoCoordinate(52.5291502, 13.3889361),
                new GeoCoordinate(52.529244, 13.3888741),
                new GeoCoordinate(52.5292772, 13.3890143),
                new GeoCoordinate(52.529354, 13.3889638),
                new GeoCoordinate(52.5293253, 13.3888356),
                new GeoCoordinate(52.5294599, 13.3887466),
                new GeoCoordinate(52.5295083, 13.3889532),
            };

            var originalOrder = geoCoordinates.Select(g => GeoProjection.ToMapCoordinate(center, g)).ToArray();

            // direct order
            var points = PolygonHelper.GetVerticies2D(center, geoCoordinates);
            Assert.IsTrue(points.SequenceEqual(originalOrder));

            // reversed
            geoCoordinates.Reverse();
            points = PolygonHelper.GetVerticies2D(center, geoCoordinates);

            Assert.IsTrue(points.SequenceEqual(originalOrder));


        }

        private static double Distance(Vector2 p1, Vector2 p2)
        {
            var diffX = p1.x - p2.x;
            var diffY = p1.y - p2.y;

            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
