using UnityEngine;
using System.Collections.Generic;
using Graph;

public class VoronoiDemo : MonoBehaviour
{
    [SerializeField]
    private int
        m_pointCount = 300;

    private float m_mapWidth = 100;
    private float m_mapHeight = 50;
    private List<Edge> m_edges;
    private List<Center> m_centers;
    private List<Corner> m_corners;

    void Awake()
    {
        Demo();
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            Demo();
        }
    }

    private void Demo()
    {
        MyMap map = new MyMap(m_mapWidth, m_mapHeight, m_pointCount);
        m_edges = map.Edges;
        m_centers = map.Centers;
        m_corners = map.Corners;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (m_centers != null)
        {
            for (int i = 0; i < m_centers.Count; i++)
            {
                Gizmos.DrawSphere(m_centers[i].point, 0.2f);
            }
        }

        Gizmos.color = Color.blue;
        if (m_corners != null)
        {
            for (int i = 0; i < m_corners.Count; i++)
            {
                Gizmos.DrawSphere(m_corners[i].point, 0.2f);
            }
        }

        if (m_edges != null)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < m_edges.Count; i++)
            {
                if (m_edges[i].v0 != null && m_edges[i].v1 != null)
                {
                    Vector2 left = m_edges[i].v0.point;
                    Vector2 right = m_edges[i].v1.point;
                    Gizmos.DrawLine((Vector3)left, (Vector3)right);
                }
            }
        }

        Gizmos.color = Color.black;
        if (m_edges != null)
        {
            for (int i = 0; i < m_edges.Count; i++)
            {
                Vector2 left = m_edges[i].d0.point;
                Vector2 right = m_edges[i].d1.point;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, m_mapHeight));
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(m_mapWidth, 0));
        Gizmos.DrawLine(new Vector2(m_mapWidth, 0), new Vector2(m_mapWidth, m_mapHeight));
        Gizmos.DrawLine(new Vector2(0, m_mapHeight), new Vector2(m_mapWidth, m_mapHeight));
    }
}