﻿using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial.AdaptiveGrid;
using Elements.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements
{

    public class ObstacleTests : ModelTest
    {

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestDefaultConstructor(Polyline polyline, bool expectedResult, int testNumber)
        {
            var rectangle = Polygon.Rectangle(10, 10);
            var obstacle = new Obstacle(rectangle, 10, 0, false, false, null);

            var result = obstacle.Intersects(polyline);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestDefaultConstructorWithOffsetAndTransform(Polyline polyline, bool expectedResult, int testNumber)
        {
            var rectangle = Polygon.Rectangle(8, 8);
            var transform = new Transform(0, 0, 1);
            var obstacle = new Obstacle(rectangle, 8, 1, false, false, transform);

            var result = obstacle.Intersects(polyline);

            Model.AddElement(obstacle);
            Model.AddElements(polyline.Segments().Select(x => new ModelCurve(x, BuiltInMaterials.XAxis)));

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestFromBBox(Polyline polyline, bool expectedResult, int testNumber)
        {
            var bbox = new BBox3(new Vector3(-4, -4, 1), new Vector3(4, 4, 9));
            var obstacle = Obstacle.FromBBox(bbox, 1);

            var result = obstacle.Intersects(polyline);

            Model.AddElement(obstacle);
            Model.AddElements(polyline.Segments().Select(x => new ModelCurve(x, BuiltInMaterials.XAxis)));

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> GetIntersectsData()
        {
            var smallPolygon = Polygon.Rectangle(5, 5);
            //Polygon fully inside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, 2)), true, 1 };
            //Polygon fully outside below 
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, -2)), false, 2 };
            //Polygon fully outside on side 
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform().Rotated(Vector3.YAxis, 90).Moved(7, 0, 5)), false, 3 };
            //Only one vertex inside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(5, 5, 2)), true, 4 };
            //Vertex on perimeter
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(10, 10, 2)), false, 5 };

            var bigPolygon = Polygon.Rectangle(20, 20);
            //Obstacle inside polygon 
            yield return new object[] { bigPolygon.TransformedPolygon(new Transform(0, 0, 2)), false, 6 };
            //One segment intersecting with obstacle
            yield return new object[] { bigPolygon.TransformedPolygon(new Transform(10, 0, 2)), true, 7 };

            //Polyline on bottom plane of obstacle
            yield return new object[] { new Polyline(new Vector3(-10, 0), new Vector3(10, 0)), true, 8 };
            //Polyline on top plane of obstacle
            yield return new object[] { new Polyline(new Vector3(-10, 0, 10), new Vector3(10, 0, 10)), true, 9 };
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0),
                new Vector3(-10, 10),
                new Vector3(10, 10),
                new Vector3(10, 0)
            ), false, 10};
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0, -2),
                new Vector3(0, 0, -2),
                new Vector3(0, 0, 12),
                new Vector3(10, 0, 12)
            ), true, 11};
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0, -2),
                new Vector3(-6, 0, -2),
                new Vector3(-6, 0, 12),
                new Vector3(10, 0, 12)
            ), false, 12};
        }

        [Fact]
        public void IntersectsRotatedPolygon()
        {
            var polygon = new Polygon(Vector3.Origin, new Vector3(5, 5), new Vector3(0, 10), new Vector3(-5, 5));
            var obstacle = new Obstacle(polygon, 5, 0, false, false, null);
            var polyline = new Polyline(new Vector3(-4, 6), new Vector3(-1, 9));

            var result = obstacle.Intersects(polyline);

            Assert.False(result);
        }

        [Fact]
        public void IntersectsObstacleFromLine()
        {
            var offset = 0.1;
            var horizontalLine = new Line(Vector3.Origin, new Vector3(0, 10));
            var horizontalObstacle = Obstacle.FromLine(horizontalLine, offset);

            Assert.True(horizontalObstacle.Intersects(horizontalLine));

            var horizontalLineOnTop = horizontalLine.TransformedLine(new Transform(0, 0, offset));
            Assert.True(horizontalObstacle.Intersects(horizontalLineOnTop));

            var horizontalLineOnBottom = horizontalLine.TransformedLine(new Transform(0, 0, -offset));
            Assert.True(horizontalObstacle.Intersects(horizontalLineOnBottom));

            var horizontalLineOnSide = horizontalLine.TransformedLine(new Transform(offset, 0, 0));
            Assert.False(horizontalObstacle.Intersects(horizontalLineOnSide));

            var horizontalLineIntersecting = horizontalLine.TransformedLine(new Transform(0, offset, 0));
            Assert.True(horizontalObstacle.Intersects(horizontalLineIntersecting));

            var verticalLine = new Line(Vector3.Origin, new Vector3(0, 0, 10));
            var verticalObstacle = Obstacle.FromLine(verticalLine);

            Assert.True(verticalObstacle.Intersects(verticalLine));

            var verticalLineOnMainTop = verticalLine.TransformedLine(new Transform(0, offset, 0));
            Assert.True(verticalObstacle.Intersects(verticalLineOnMainTop));

            var verticalLineOnMainBottom = verticalLine.TransformedLine(new Transform(0, -offset, 0));
            Assert.True(verticalObstacle.Intersects(verticalLineOnMainBottom));

            var lineIntersectingVerticalObstacle = verticalLine.TransformedLine(new Transform().RotatedAboutPoint(new Vector3(0, 0, 5), Vector3.YAxis, 30));
            Assert.True(verticalObstacle.Intersects(lineIntersectingVerticalObstacle));

            var angledLine = new Line(Vector3.Origin, new Vector3(10, 10, 10));
            var angledObstacle = Obstacle.FromLine(angledLine);

            Assert.True(angledObstacle.Intersects(angledLine));

            var lineIntersectingAngleObstacle = new Line(new Vector3(5, 5), new Vector3(5, 5, 10));
            Assert.True(angledObstacle.Intersects(lineIntersectingAngleObstacle));

            var offsetedLine = angledLine.TransformedLine(new Transform(offset, 0, 0));
            Assert.True(angledObstacle.Intersects(offsetedLine));
        }
    }
}
