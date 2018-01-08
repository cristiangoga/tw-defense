using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    /**
        This represents the polygon "corner" in the Voronoi diagram and
        also a triangle "center" in the Delaunay triangulation
    **/

    public class Corner
    {
        public int index;

        public Vector2 point; // location 
        public bool water; // lake or ocean
        public bool ocean; // ocean
        public bool coast; // touches ocean and land polygons
        public bool border;  // at the edge of the map
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        public List<Center> touches; //set of polygons touching this corner
        public List<DoubleEdge> protrudes; //set of edges touching the corner
        public List<Corner> adjacent; //set of corners connected to this one

        public int river;  // 0 if no river, or volume of water in river
        public Corner downslope;  // pointer to adjacent corner most downhill
        public Corner watershed;  // pointer to coastal corner, or null
        public int watershed_size;
    }
}