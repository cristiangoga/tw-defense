using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    /**
        This represents the polygon "center" in the Voronoi diagram and
        also a triangle "corner" in the Delaunay triangulation
    **/
    public class Center
    {
        public int index;

        public Vector2 point; // location 
        public bool water; // lake or ocean
        public bool ocean; // ocean
        public bool coast; // land polygon touching an ocean
        public bool border;  // at the edge of the map
        public string biome;  // biome type (see article)
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        public List<Center> neighbors; //set of adjacent polygons
        public List<Edge> borders; //set of bordering edges
        public List<Corner> corners; //set of polygon corners
    }
}
