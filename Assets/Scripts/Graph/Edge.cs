using UnityEngine;

namespace Graph
{
    /**
        Edges point to four nodes: two polygon centers and two corners,
        because every triangle in the Delaunay triangulation corresponds to a polygon corner in the Voronoi diagram and
        every polygon in the Voronoi diagram corresponds to a corner of a Delaunay triangle. 
    **/
    public class Edge
    {
        public int index;
        public Center d0, d1; // Delaunay edge
        public Corner v0, v1; // Voronoi edge
        public Vector2 midpoint; // halfway between v0,v1
        public int river; // volume of water, or 0
    }
}