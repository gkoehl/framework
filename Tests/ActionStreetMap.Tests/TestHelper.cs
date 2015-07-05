﻿using System;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core.Geometry.Triangle;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Triangle.Meshing;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Explorer;
using ActionStreetMap.Explorer.Geometry;
using ActionStreetMap.Explorer.Geometry.ThickLine;
using ActionStreetMap.Explorer.Infrastructure;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Infrastructure.Utilities;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Unity.IO;

namespace ActionStreetMap.Tests
{
    public static class TestHelper
    {
        public static readonly GeoCoordinate BerlinTestFilePoint = new GeoCoordinate(52.54994964,13.35064315);
        public static readonly GeoCoordinate BerlinInvalidenStr = new GeoCoordinate(52.531036, 13.384866);
        public static readonly GeoCoordinate BerlinHauptBanhoff = new GeoCoordinate(52.5254967, 13.3733636);
        public static readonly GeoCoordinate BerlinTiergarten = new GeoCoordinate(52.516809, 13.367598);
        public static readonly GeoCoordinate BerlinVolksPark = new GeoCoordinate(52.526437, 13.432122);

        public static readonly GeoCoordinate MoscowRedSquare = new GeoCoordinate(55.7537315, 37.6198537);

        // these values are defined in header.txt of test element source
        public static readonly GeoCoordinate TestMinPoint = new GeoCoordinate(52.54, 13.346);
        public static readonly GeoCoordinate TestMaxPoint = new GeoCoordinate(52.552, 13.354);

        public const string ConfigTestRootFile = "test.json";
        public const string ConfigAppRootFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\settings.json";

        public const string TestNmeaFilePath = @"..\..\..\..\Tests\TestAssets\Nmea\berlin_seestr_speed_increasing.nme";
        public const string TestIndexSettingsPath = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\index.json";

        public const string MapDataPath = @"Maps\osm";

        public const string TestThemeFile = @"..\..\..\..\Tests\TestAssets\Themes\theme.json";
        public const string TestBaseMapcssFile = @"..\..\..\..\Tests\TestAssets\Mapcss\base.mapcss";
        public const string DefaultMapcssFile = @"..\..\..\..\Tests\TestAssets\DemoResources\Config\default.mapcss";

        public const string BerlinXmlData = @"..\..\..\..\Tests\TestAssets\Osm\berlin_xml_api.osm";

        public static GameRunner GetGameRunner(bool bootstrap = true)
        {
            return GetGameRunner(new Container(), bootstrap);
        }

        public static GameRunner GetGameRunner(IContainer container, bool bootstrap = true)
        {
            var jsonConfigSection = GetJsonConfig(ConfigAppRootFile);
            return GetGameRunner(container, jsonConfigSection, bootstrap);
        }

        public static GameRunner GetGameRunner(IContainer container, IConfigSection config, bool bootstrap = true)
        {
            // this three service should be registered before game runner is started
            container.Register(Component.For<ITrace>().Use<ConsoleTrace>().Singleton());
            container.Register(Component.For<IPathResolver>().Use<TestPathResolver>().Singleton());
            container.Register(Component.For<IMessageBus>().Use<MessageBus>().Singleton());
            container.Register(Component.For<IFileSystemService>().Use<FileSystemService>().Singleton());

            var runner = new GameRunner(container, config)
                .RegisterPlugin<TestBootstrapperPlugin>("test");
            return bootstrap ? runner.Bootstrap() : runner;
        }

        public static JsonConfigSection GetJsonConfig(string configPath)
        {
            return new JsonConfigSection(new FileSystemService(new TestPathResolver(), new DefaultTrace())
                .ReadText(configPath));
        }

        internal static IndexSettings GetIndexSettings()
        {
            var jsonContent = GetFileSystemService().ReadText(TestIndexSettingsPath);
            var node = JSON.Parse(jsonContent);
            return new IndexSettings().ReadFromJson(node);
        }

        public static IFileSystemService GetFileSystemService()
        {
            return new FileSystemService(new TestPathResolver(), new ConsoleTrace());
        }

        public static IObjectPool GetObjectPool()
        {
            return new ObjectPool()
                .RegisterObjectType<MeshTriangle>(() => new MeshTriangle())
                .RegisterListType<MeshTriangle>(32)
                .RegisterListType<Point>(32)
                .RegisterObjectType<Clipper>(() => new Clipper())
                .RegisterObjectType<ClipperOffset>(() => new ClipperOffset())
                .RegisterListType<Tuple<Surface, Action<Mesh>>>(32)
                .RegisterListType<RoadElement>(32)
                .RegisterListType<Surface>(32)
                .RegisterListType<GeoCoordinate>(256)
                .RegisterListType<MapPoint>(256)
                .RegisterListType<LineElement>(32)
                .RegisterListType<int>(256);
        }
    }
}
