using Mapbox.Directions;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.Unity.Utilities;
using Mapbox.Unity;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Mapbox.Examples;
using Unity.VisualScripting;
using System;

public class RouteBuilder : MonoBehaviour
{
    /// <summary>
    /// Type of route (line, curve)
    /// </summary>
    [SerializeField]
    MeshModifier[] MeshModifiers;

    /// <summary>
    /// Material of route
    /// </summary>
    [SerializeField]
    Material _material;


    //public GameObject _GPXMarkPrefab;
    /// <summary>
    /// Waypoints of gameobjects
    /// </summary>
    public List<Point> points = new List<Point>();

    /// <summary>
    /// Frequency of map updates
    /// </summary>
    [SerializeField]
    [Range((float)0.1, 10)]
    private float UpdateFrequency = (float)1;

    /// <summary>
    /// Gameobject of path
    /// </summary>
    GameObject _directionsGO;

    bool isModifiersInitialized = false;

    /// <summary>
    /// Size of marker
    /// </summary>
    public float _spawnScale = 0.3f;

    public class MinefieldDimensions
    {
        public static (float width, float height) GetFieldDimensions(List<Mine> mines)
        {
            if (mines == null || mines.Count == 0)
            {
                return (-1,-1);
            }

            float minX = mines.Min(mine => mine.Position.X);
            float maxX = mines.Max(mine => mine.Position.X);
            float minY = mines.Min(mine => mine.Position.Y);
            float maxY = mines.Max(mine => mine.Position.Y);

            float width = maxX - minX + 1;

            float height = maxY - minY + 1;

            return (width, height);
        }
    }

    public class MinePoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MinePoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(MinePoint other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            MinePoint p = (MinePoint)obj;
            return X == p.X && Y == p.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }
    }

    public class Mine
    {
        public MinePoint Position { get; set; }
        public double BlastRadius { get; set; }

        public Mine(MinePoint position, double blastRadius)
        {
            Position = position;
            BlastRadius = blastRadius;
        }

        public bool IsInBlastRadius(MinePoint point)
        {
            return Position.DistanceTo(point) <= BlastRadius;
        }
    }

    public class Node
    {
        public MinePoint Position { get; set; }
        public double GCost { get; set; } // Cost from start
        public double HCost { get; set; } // Heuristic cost to end
        public double FCost => GCost + HCost; // Total cost

        public Node Parent { get; set; }

        public Node(MinePoint position)
        {
            Position = position;
        }
    }

    public class Minefield
    {
        private List<Mine> mines;
        private float width;
        private float height;

        public Minefield(float width, float height, List<Mine> mines)
        {
            this.width = width;
            this.height = height;
            this.mines = mines;
        }

        public bool IsSafe(MinePoint point)
        {
            foreach (var mine in mines)
            {
                if (mine.IsInBlastRadius(point))
                    return false;
            }
            return true;
        }

        public List<MinePoint> GetNeighbors(MinePoint point)
        {
            var neighbors = new List<MinePoint>();

            var potentialNeighbors = new List<MinePoint>
        {
            new MinePoint(point.X + 1, point.Y),
            new MinePoint(point.X - 1, point.Y),
            new MinePoint(point.X, point.Y + 1),
            new MinePoint(point.X, point.Y - 1)
        };

            foreach (var neighbor in potentialNeighbors)
            {
                if (neighbor.X >= 0 && neighbor.X < width && neighbor.Y >= 0 && neighbor.Y < height && IsSafe(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public List<MinePoint> FindSafePath(MinePoint start, MinePoint end)
        {
            var openList = new List<Node>();
            var closedList = new HashSet<MinePoint>();

            var startNode = new Node(start) { GCost = 0, HCost = start.DistanceTo(end) };
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                openList.Sort((nodeA, nodeB) => nodeA.FCost.CompareTo(nodeB.FCost));
                var currentNode = openList[0];
                Debug.Log("aaa");
                if (currentNode.Position.Equals(end))
                    return ReconstructPath(currentNode);

                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);

                foreach (var neighbor in GetNeighbors(currentNode.Position))
                {
                    if (closedList.Contains(neighbor))
                        continue;

                    var neighborNode = new Node(neighbor)
                    {
                        GCost = currentNode.GCost + currentNode.Position.DistanceTo(neighbor),
                        HCost = neighbor.DistanceTo(end),
                        Parent = currentNode
                    };

                    var existingNode = openList.Find(n => n.Position.Equals(neighbor));

                    if (existingNode == null)
                    {
                        openList.Add(neighborNode);
                    }
                    else if (neighborNode.GCost < existingNode.GCost)
                    {
                        existingNode.GCost = neighborNode.GCost;
                        existingNode.Parent = currentNode;
                    }
                }
            }

            return null; // Path not found
        }

        private List<MinePoint> ReconstructPath(Node endNode)
        {
            var path = new List<MinePoint>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
    /// <summary>
    /// Build GPX route
    /// </summary>
    public void BuildRoute(List<Vector2> data)
    {
        DestroyPaths();

        points.Clear();
        foreach (var coord in data)
        {
            Point point = new Point();
            point.location = new Vector2d(coord.x,coord.y);
            points.Add(point);
        }
        Query();
    }

    /// <summary>
    /// Build Google GPX route
    /// </summary>

    private IEnumerator UpdateGPXCoords()
    {
        while (true)
        {
            if (gameObject.GetComponent<MapController>()._map.gameObject.activeSelf)
            {
                GameObject.Find("GPXLine").Destroy();

                GameObject myLine = new GameObject();
                myLine.name = "GPXLine";
                myLine.transform.localPosition = gameObject.GetComponent<MapController>()._map.GeoToWorldPosition(new Vector2d(points[0].location.x, points[0].location.y), true);
                myLine.AddComponent<LineRenderer>();
                LineRenderer lineRenderer = myLine.GetComponent<LineRenderer>();
                lineRenderer.material = _material;
                lineRenderer.startWidth = 0.04f;
                lineRenderer.endWidth = 0.04f;
                int seg = points.Count;
                Vector3[] vP = new Vector3[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    vP[i] = gameObject.GetComponent<MapController>()._map.GeoToWorldPosition(new Vector2d(points[i].location.x, points[i].location.y), true);
                }
                for (int i = 0; i < seg; i++)
                {
                    float t = i / (float)seg;
                    lineRenderer.SetVertexCount(seg);
                    lineRenderer.SetPositions(vP);
                }
                yield return null;
            }
            else
            {
                yield break;
            }
        }
    }
    /// <summary>
    /// Destroy all paths
    /// </summary>
    public void DestroyPaths()
    {
        StopCoroutine(UpdateGPXCoords());
        if (GameObject.FindGameObjectsWithTag("Path") != null)
        {
            foreach (GameObject path in GameObject.FindGameObjectsWithTag("Path"))
                path.Destroy();
        }
        /*gameObject.GetComponent<MapController>()._map.OnInitialized -= Query;
        gameObject.GetComponent<MapController>()._map.OnUpdated -= Query;*/
    }
    void Query()
    {
        RenderGPXPoints();
        //BuildGPXRoute();
    }
    void RenderGPXPoints()
    {
        /*foreach (Point p in points)
        {
            var instance = Instantiate(_GPXMarkPrefab);
            instance.name = "GPX point " + points.IndexOf(p) + 1;
            instance.tag = "Path";
            instance.transform.localPosition = gameObject.GetComponent<MapController>()._map.GeoToWorldPosition(new Vector2d(p.location.x, p.location.y), true);
            instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            instance.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = (points.IndexOf(p) + 1).ToString();
            instance.transform.GetChild(0).gameObject.SetActive(false);
            p.pointObject = instance;
        }*/

        GameObject.Find("GPXLine").Destroy();

        GameObject myLine = new GameObject();
        myLine.name = "GPXLine";
        myLine.tag = "Path";
        myLine.transform.localPosition = gameObject.GetComponent<MapController>()._map.GeoToWorldPosition(new Vector2d(points[0].location.x, points[0].location.y), true);
        myLine.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = myLine.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = _material;
        int seg = points.Count;
        Vector3[] vP = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            vP[i] = gameObject.GetComponent<MapController>()._map.GeoToWorldPosition(new Vector2d(points[i].location.x, points[i].location.y), true);
        }
        for (int i = 0; i < seg; i++)
        {
            float t = i / (float)seg;
            lineRenderer.SetVertexCount(seg);
            lineRenderer.SetPositions(vP);
        }

        StartCoroutine(UpdateGPXCoords());
    }
    void BuildGPXRoute()
    {
        var meshData = new MeshData();
        var dat = new List<Vector3>();
        foreach (var point in points)
        {
            Vector2d posXZ = Conversions.GeoToWorldPosition(new Vector2d(point.location.x, point.location.y), gameObject.GetComponent<MapController>()._map.CenterMercator, gameObject.GetComponent<MapController>()._map.WorldRelativeScale);
            dat.Add(new Vector3((float)posXZ.x, -98, (float)posXZ.y));
        }

        var feat = new VectorFeatureUnity();
        feat.Points.Add(dat);

        foreach (MeshModifier mod in MeshModifiers.Where(x => x.Active))
        {
            mod.Run(feat, meshData, gameObject.GetComponent<MapController>()._map.Zoom / 160);
        }
        CreateGameObject(meshData);

    }

    /// <summary>
    /// Create mesh gameobject
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    GameObject CreateGameObject(MeshData data)
    {
        if (_directionsGO != null)
        {
            _directionsGO.Destroy();
        }

        _directionsGO = new GameObject("Path from " + gameObject.GetComponent<MapController>().player.name + " to point");
        _directionsGO.tag = "Path";
        if (gameObject.GetComponent<MapController>()._map != null)
        {
            _directionsGO.transform.SetParent(gameObject.GetComponent<MapController>()._map.transform);
        }

        var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
        mesh.subMeshCount = data.Triangles.Count;

        mesh.SetVertices(data.Vertices);
        int _counter = data.Triangles.Count;
        for (int i = 0; i < _counter; i++)
        {
            var triangle = data.Triangles[i];
            mesh.SetTriangles(triangle, i);
        }

        _counter = data.UV.Count;
        for (int i = 0; i < _counter; i++)
        {
            var uv = data.UV[i];
            mesh.SetUVs(i, uv);
        }

        mesh.RecalculateNormals();
        _directionsGO.AddComponent<MeshRenderer>().material = _material;
        return _directionsGO;
    }
}