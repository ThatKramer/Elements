﻿using Elements;
using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// AdaptiveGrid obstacle represented by a set of points with extra parameters.
    /// Points are used to created bounding box that is aligned with transformation parameter
    /// with extra offset. Since offset is applied on the box, distance on corners is even larger.
    /// Can be constructed from different objects.
    /// </summary>
    public class Obstacle
    {
        /// <summary>
        /// Create an obstacle from a column.
        /// </summary>
        /// <param name="column">Column to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromColumn(Column column, double offset = 0, bool perimeter = false)
        {
            var p = column.Profile.Perimeter.TransformedPolygon(
                new Transform(column.Location));
            List<Vector3> points = new List<Vector3>();
            points.AddRange(p.Vertices);
            points.AddRange(p.Vertices.Select(
                v => new Vector3(v.X, v.Y, v.Z + column.Height)));
            return new Obstacle(points, offset, perimeter, null);
        }

        /// <summary>
        /// Create an obstacle from a wall.
        /// </summary>
        /// <param name="wall">Wall to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromWall(StandardWall wall, double offset = 0, bool perimeter = false)
        {
            var ortho = wall.CenterLine.Direction().Cross(Vector3.ZAxis);
            List<Vector3> points = new List<Vector3>();
            points.Add(wall.CenterLine.Start + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.Start - ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End - ortho * wall.Thickness / 2);
            points.AddRange(points.Select(v => new Vector3(v.X, v.Y, v.Z + wall.Height)).ToArray());
            var transfrom = new Transform(Vector3.Origin,
                wall.CenterLine.Direction(), ortho, Vector3.ZAxis);
            return new Obstacle(points, offset, perimeter, transfrom);
        }

        /// <summary>
        /// Create an obstacle from a bounding box.
        /// </summary>
        /// <param name="box">Bounding box to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromBBox(BBox3 box, double offset = 0, bool perimeter = false)
        {
            return new Obstacle(box.Corners(), offset, perimeter, null);
        }

        /// <summary>
        /// Create an obstacle from a 2d polygon and height.
        /// </summary>
        /// <param name="polyon">2d polygon to avoid.</param>
        /// <param name="height">Height of the obstacle.</param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle From2DPolygon(Polygon polyon, double height, double offset = 0, bool perimeter = false)
        {
            List<Vector3> points = new List<Vector3>();
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y)));
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y, height)));
            return new Obstacle(points, offset, perimeter, null);
        }

        /// <summary>
        /// Create an obstacle from a line.
        /// </summary>
        /// <param name="line">Line to avoid.</param>
        /// <param name="offset">Extra space around obstacle bounding box. Should be larger than 0.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <returns>New obstacle object.</returns>
        public static Obstacle FromLine(Line line, double offset = 0.1, bool perimeter = false)
        {
            if (offset < Vector3.EPSILON)
            {
                throw new ArgumentException("Offset should be larger then zero.");
            }

            List<Vector3> points = new List<Vector3>();
            points.Add(line.Start);
            points.Add(line.End);

            Transform frame = null;
            var forward = line.Direction();
            if (!forward.IsParallelTo(Vector3.ZAxis))
            {
                var rigth = forward.Cross(Vector3.ZAxis);
                var up = forward.Cross(rigth);
                frame = new Transform(Vector3.Origin, forward, rigth, up);
            }

            return new Obstacle(points, offset, perimeter, frame);
        }

        /// <summary>
        /// Create an obstacle from a list of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset">Extra space around obstacle bounding box.</param>
        /// <param name="perimeter">Should edges be created around obstacle.</param>
        /// <param name="transformation">Transformation of the obstacle.</param>
        public Obstacle(List<Vector3> points, double offset, bool perimeter, Transform transformation)
        {
            Points = points;
            Offset = offset;
            Perimeter = perimeter;
            Transform = transformation;
        }

        /// <summary>
        /// List of points defining obstacle.
        /// </summary>
        public List<Vector3> Points { get; set; }

        /// <summary>
        /// Offset of bounding box created from the list of points.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// Should edges be created around obstacle.
        /// If false - any intersected edges are just discarded.
        /// If true - intersected edges are cut to obstacle and perimeter edges are inserted.
        /// </summary>
        public bool Perimeter { get; set; }

        /// <summary>
        /// Transformation of bounding box created from the list of points.
        /// </summary>
        public Transform Transform { get; set; }
    }
}
