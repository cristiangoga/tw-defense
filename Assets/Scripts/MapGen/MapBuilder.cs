using Delaunay;
using Delaunay.Geo;
using System;
using System.Collections.Generic;
using UnityEngine;
using Graph;

namespace MapGen
{
    public class MapBuilder
    {
        private float width;
        private float height;
        private int pointsCount;

        // These store the graph data
        private List<Vector2> points;  // Only useful during map construction
        private List<Center> _centers;
        private List<Corner> _corners;
        private List<DoubleEdge> _edges;

        public const int NUM_LLOYD_ITERATIONS = 2;

        public List<Center> Centers
        {
            get
            {
                return _centers;
            }
        }

        public List<Corner> Corners
        {
            get
            {
                return _corners;
            }
        }

        public List<DoubleEdge> Edges
        {
            get
            {
                return _edges;
            }
        }

        public MapBuilder(float width, float height, int pointsCount)
        {
            this.width = width;
            this.height = height;
            this.pointsCount = pointsCount;
            ResetLists();
            BuildGraph();
        }

        private void ResetLists()
        {
            if (points == null)
            {
                points = new List<Vector2>();
            }
            else
            {
                points.Clear();
            }
            if (_centers == null)
            {
                _centers = new List<Center>();
            }
            else
            {
                _centers.Clear();
            }
            if (_corners == null)
            {
                _corners = new List<Corner>();
            }
            else
            {
                _corners.Clear();
            }

            if (_edges == null)
            {
                _edges = new List<DoubleEdge>();
            }
            else
            {
                _edges.Clear();
            }
        }

        // Build graph data structure in 'edges', 'centers', 'corners',
        // based on information in the Voronoi results: point.neighbors
        // will be a list of neighboring points of the same type (corner
        // or center); point.edges will be a list of edges that include
        // that point. Each edge connects to four points: the Voronoi edge
        // edge.{v0,v1} and its dual Delaunay triangle edge edge.{d0,d1}.
        // For boundary polygons, the Delaunay edge will have one null
        // point, and the Voronoi edge may be null.
        private void BuildGraph()
        {
            Voronoi voronoi = new Voronoi(pointsCount, new Rect(0, 0, width, height), NUM_LLOYD_ITERATIONS);
            points = voronoi.GetPoints();
            // Workaround for Voronoi lib bug: we need to call region()
            // before Edges or neighboringSites are available
            foreach (var center in _centers)
            {
                voronoi.Region(center.point);
            }

            List<Delaunay.Edge> libedges = voronoi.Edges();

            Dictionary<Vector2, Center> centerLookup = new Dictionary<Vector2, Center>();

            // Build Center objects for each of the points, and a lookup map
            // to find those Center objects again as we build the graph
            foreach (var point in points)
            {
                Center p = new Center();
                p.index = _centers.Count;
                p.point = point;
                p.neighbors = new List<Center>();
                p.borders = new List<DoubleEdge>();
                p.corners = new List<Corner>();
                _centers.Add(p);
                centerLookup[point] = p;
            }

            Dictionary<int, List<Corner>> _cornerMap = new Dictionary<int, List<Corner>>();
            foreach (Delaunay.Edge libedge in libedges)
            {
                LineSegment dedge = libedge.DelaunayLine();
                LineSegment vedge = libedge.VoronoiEdge();

                // Fill the graph data. Make an Edge object corresponding to
                // the edge from the voronoi library.
                DoubleEdge edge = new DoubleEdge();
                edge.index = _edges.Count;
                edge.river = 0;
                if (vedge.p0 != null && vedge.p1 != null)
                {
                    edge.midpoint = Vector2.Lerp((Vector2)vedge.p0, (Vector2)vedge.p1, 0.5f);
                }

                // Edges point to corners. Edges point to centers. 
                edge.v0 = MakeCorner(vedge.p0, _cornerMap);
                edge.v1 = MakeCorner(vedge.p1, _cornerMap);
                Center findCenter = null;
                if (centerLookup.TryGetValue((Vector2)dedge.p0, out findCenter))
                {
                    edge.d0 = findCenter;
                }
                else
                {
                    edge.d0 = null;
                }
                if (centerLookup.TryGetValue((Vector2)dedge.p1, out findCenter))
                {
                    edge.d1 = centerLookup[(Vector2)dedge.p1];
                }
                else
                {
                    edge.d1 = null;
                }

                // Centers point to edges. Corners point to edges.
                if (edge.d0 != null) { edge.d0.borders.Add(edge); }
                if (edge.d1 != null) { edge.d1.borders.Add(edge); }
                if (edge.v0 != null) { edge.v0.protrudes.Add(edge); }
                if (edge.v1 != null) { edge.v1.protrudes.Add(edge); }

                // Centers point to centers.
                if (edge.d0 != null && edge.d1 != null)
                {
                    AddToGenericList(edge.d0.neighbors, edge.d1);
                    AddToGenericList(edge.d1.neighbors, edge.d0);
                }

                // Corners point to corners
                if (edge.v0 != null && edge.v1 != null)
                {
                    AddToGenericList(edge.v0.adjacent, edge.v1);
                    AddToGenericList(edge.v1.adjacent, edge.v0);
                }

                // Centers point to corners
                if (edge.d0 != null)
                {
                    AddToGenericList(edge.d0.corners, edge.v0);
                    AddToGenericList(edge.d0.corners, edge.v1);
                }
                if (edge.d1 != null)
                {
                    AddToGenericList(edge.d1.corners, edge.v0);
                    AddToGenericList(edge.d1.corners, edge.v1);
                }

                // Corners point to centers
                if (edge.v0 != null)
                {
                    AddToGenericList(edge.v0.touches, edge.d0);
                    AddToGenericList(edge.v0.touches, edge.d1);
                }
                if (edge.v1 != null)
                {
                    AddToGenericList(edge.v1.touches, edge.d0);
                    AddToGenericList(edge.v1.touches, edge.d1);
                }
                _edges.Add(edge);
            }
        }

        // The Voronoi library generates multiple Point objects for
        // corners, and we need to canonicalize to one Corner object.
        // To make lookup fast, we keep an dictionary of Points, bucketed by
        // x value, and then we only have to look at other Points in
        // nearby buckets. When we fail to find one, we'll create a new
        // Corner object.
        private Corner MakeCorner(Nullable<Vector2> point, Dictionary<int, List<Corner>> cornerMap)
        {
            int bucket;
            if (point == null) return null;
            for (bucket = (int)(point.Value.x) - 1; bucket <= (int)(point.Value.x) + 1; bucket++)
            {
                List<Corner> bucketList;
                if (cornerMap.TryGetValue(bucket, out bucketList))
                {
                    foreach (Corner corner in bucketList)
                    {
                        float dx = point.Value.x - corner.point.x;
                        float dy = point.Value.y - corner.point.y;
                        if (dx * dx + dy * dy < 1e-6)
                        {
                            return corner;
                        }
                    }
                }
            }
            bucket = (int)(point.Value.x);
            if (!cornerMap.ContainsKey(bucket))
            {
                cornerMap[bucket] = new List<Corner>();
            }
            Corner q = new Corner();
            q.index = _corners.Count;
            q.point = point.Value;
            q.border = (point.Value.x == 0 || point.Value.x == width
                            || point.Value.y == 0 || point.Value.y == height);
            q.touches = new List<Center>();
            q.protrudes = new List<DoubleEdge>();
            q.adjacent = new List<Corner>();
            _corners.Add(q);
            cornerMap[bucket].Add(q);
            return q;
        }

        private void AddToGenericList<T>(List<T> list, T x)
        {
            if (x != null && list.Find(y => y.Equals(x)) == null)
            {
                list.Add(x);
            }
        }
    }
}
