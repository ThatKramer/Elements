using Elements.Geometry;
using Elements.Search;
using Xunit;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Elements.Tests
{
    public class NetworkTests : ModelTest
    {
        private readonly ITestOutputHelper _output;

        public NetworkTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
            this.GenerateJson = false;
        }

        [Fact]
        public void BranchNodes()
        {
            var network = new Network<object>();
            var a = network.AddVertex();
            var b = network.AddVertex();
            var c = network.AddVertex();
            network.AddEdgeOneWay(a, b, null);
            network.AddEdgeOneWay(b, c, null);
            Assert.Equal(2, network.BranchNodes().Count);
        }

        [Fact]
        public void LeafNodes()
        {
            var network = new Network<object>();
            var a = network.AddVertex();
            var b = network.AddVertex();
            var c = network.AddVertex();
            network.AddEdgeOneWay(a, b, null);
            network.AddEdgeOneWay(b, c, null);
            Assert.Single(network.LeafNodes());
        }

        [Fact]
        public void CrossingLines()
        {
            this.Name = nameof(CrossingLines);

            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(0, -2), new Vector3(0, 2));

            var network = Network<Line>.FromSegmentableItems(new[] { a, b }, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);

            this.Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));

            Assert.Equal(5, allNodeLocations.Count);
        }

        [Fact]
        public void ElevatedLines()
        {
            var a = new Line(new Vector3(-2, 0, 1), new Vector3(2, 0, 1));
            var b = new Line(new Vector3(0, -2, 1), new Vector3(0, 2, 1));
            var pts = new[] { a, b }.Intersections();
            Assert.Single(pts);
        }

        [Fact]
        public void DuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = a;
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void ReversedDuplicateLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(2, 0), new Vector3(-2, 0));
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void OverlappingLines()
        {
            var a = new Line(new Vector3(-2, 0), new Vector3(2, 0));
            var b = new Line(new Vector3(-1, 0), new Vector3(1, 0)); ;
            var pts = new[] { a, b }.Intersections();
            Assert.Empty(pts);
        }

        [Fact]
        public void LineSweepSucceedsWithCoincidentPoints()
        {
            this.Name = nameof(LineSweepSucceedsWithCoincidentPoints);

            var ngon = Polygon.Ngon(4, 1);
            var lines = new List<Line>(ngon.Segments());

            // Lines with coincident left-most points, and a vertical line.
            var a = new Line(new Vector3(-2, 0.1), new Vector3(2, 0.1));
            var b = new Line(new Vector3(-0.1, -2), new Vector3(-0.1, 2));
            var c = new Line(new Vector3(-2, -0.1), new Vector3(2, -0.1));
            lines.Add(a);
            lines.Add(b);
            lines.Add(c);

            var network = Network<Line>.FromSegmentableItems(lines, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);

            Assert.Equal(18, allNodeLocations.Count);

            this.Model.AddElements(network.ToModelText(allNodeLocations, Colors.Black));
        }

        [Fact]
        public void TriangleIntersections()
        {
            var ngon = Polygon.Ngon(4, 1);
            var segs = ngon.Segments();
            var lines = new List<Line>() { segs[1], segs[2] };

            // Lines with coincident left-most points, and a vertical line.
            var b = new Line(new Vector3(-0.1, -2), new Vector3(-0.1, 2));
            lines.Add(b);

            var pts = lines.Intersections();
            Assert.Equal(3, pts.Count);
        }

        [Fact]
        public void MultipleLinesIntersect()
        {
            this.Name = nameof(MultipleLinesIntersect);

            var r = new Random();
            var scale = 15;

            var lines = new List<Line>();
            for (var i = 0; i < 100; i++)
            {
                var start = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                var end = new Vector3(r.NextDouble() * scale, r.NextDouble() * scale, 0);
                lines.Add(new Line(start, end));
            }

            var sw = new Stopwatch();
            sw.Start();
            var pts = lines.Intersections();
            sw.Stop();
            _output.WriteLine($"{sw.ElapsedMilliseconds}ms for finding {pts.Count()} intersections.");
            sw.Reset();

            var network = Network<Line>.FromSegmentableItems(lines, (l) => { return l; }, out List<Vector3> allNodeLocations, out _);
            var arrows = network.ToModelArrows(allNodeLocations, Colors.Red);
            this.Model.AddElement(arrows);
        }


        [Fact]
        public void IntersectingWallLines()
        {
            this.Name = nameof(IntersectingWallLines);

            var json = File.ReadAllText("../../../models/Geometry/IntersectingWalls.json");
            var model = Model.FromJson(json);
            var wallGroups = model.AllElementsOfType<WallByProfile>().GroupBy(w => w.Centerline.Start.Z);
            foreach (var group in wallGroups)
            {
                var network = Network<WallByProfile>.FromSegmentableItems(group.ToList(),
                                                                          (wall) => { return wall.Centerline; },
                                                                          out List<Vector3> allNodeLocations,
                                                                          out _);
                this.Model.AddElement(network.ToModelArrows(allNodeLocations, Colors.Black));
            }
        }

        [Fact]
        public void FigureEight()
        {
            this.Name = nameof(FigureEight);
            var t = new Transform();
            t.Rotate(Vector3.ZAxis, 0.0);

            var a = new Line(t.OfPoint(Vector3.Origin), t.OfPoint(new Vector3(10, 0, 0)));
            var b = new Line(t.OfPoint(new Vector3(0, 5, 0)), t.OfPoint(new Vector3(10, 5, 0)));
            var c = new Line(t.OfPoint(new Vector3(0, 10, 0)), t.OfPoint(new Vector3(10, 10, 0)));
            var d = new Line(t.OfPoint(new Vector3(5, 0, 0)), t.OfPoint(new Vector3(5, 5, 0)));
            var e = new Line(t.OfPoint(new Vector3(5, 5, 0)), t.OfPoint(new Vector3(5, 10, 0)));
            var f = new Line(t.OfPoint(Vector3.Origin), t.OfPoint(new Vector3(0, 10, 0)));
            var g = new Line(t.OfPoint(new Vector3(10, 0, 0)), t.OfPoint(new Vector3(10, 10, 0)));
            var network = Network<Line>.FromSegmentableItems(new[] { a, b, c, d, e, f, g }, (o) => { return o; }, out List<Vector3> allNodeLocations, out _, true);
            Assert.Equal(9, network.BranchNodes().Count());
            this.Model.AddElement(network.ToModelArrows(allNodeLocations, Colors.Black));
        }

        [Fact]
        public void SingleLeafTraversesOutsideAndClosedLoop()
        {
            // A vertical line with a triangle pointing to the right.
            var a = new Line(Vector3.Origin, new Vector3(0, 10, 0));
            var b = new Line(new Vector3(0, 5, 0), new Vector3(5, 3, 0));
            var c = new Line(new Vector3(5, 3, 0), new Vector3(0, 0, 0));
            var network = Network<Line>.FromSegmentableItems(new[] { a, b, c }, (o) => { return o; }, out List<Vector3> allNodeLocations, out _, true);

            var leafIndices = new List<int>();
            for (var i = 0; i < network.NodeCount(); i++)
            {
                if (network.EdgesAt(i).Count() == 1)
                {
                    leafIndices.Add(i);
                }
            }

            Assert.Single(leafIndices);

            var visitedEdges = new List<LocalEdge>();
            foreach (var leafIndex in leafIndices)
            {
                var path = network.Traverse(leafIndex, Network<Line>.TraverseSmallestPlaneAngle, allNodeLocations, visitedEdges, out List<int> visited);
                Assert.Equal(6, path.Count);
                _output.WriteLine(string.Join(',', visited));
            }
        }

        [Fact]
        public void FindAllClosedRegions()
        {
            this.Name = nameof(FindAllClosedRegions);

            var r = new Random(23);
            var size = 1000.0;
            var lines = new List<Line>(200);
            for (var i = 0; i < 50; i++)
            {
                var start = new Vector3(r.NextDouble() * size, r.NextDouble() * size, 0);
                var end = new Vector3(r.NextDouble() * size, r.NextDouble() * size, 0);
                var line = new Line(start, end);
                lines.Add(line);
                this.Model.AddElement(new ModelCurve(line));
            }

            var network = Network<Line>.FromSegmentableItems(lines, (item) => { return item; }, out var allNodeLocations, out _);
            var closedRegions = network.FindAllClosedRegions(allNodeLocations);

            foreach (var region in closedRegions)
            {
                var p = new Panel(region, r.NextMaterial());
                this.Model.AddElement(p);
            }
        }
    }
}