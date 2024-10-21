using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using NUnit.Framework;
using ARLocation;
using Unity.VisualScripting;
using static RouteBuilder;
using System.Collections;
using static VoronoiPathFinding;

public class minespointsSpawn : MonoBehaviour
{

    public AbstractMap _map;
    public GameObject _minePrefab;
    public GameObject _pathPointPrefab;

    /// <summary>
    /// Size of marker
    /// </summary>
    public float _spawnScale = 0.3f;

    public GameObject focusAtAll;


    public List<Point> minespoints;
    public List<Point> pathpoints;

    private int numberofminespoints = 0;

    private void Start()
    {
        minespoints = new List<Point>();
        pathpoints = new List<Point>();
    }

    public void SetMine(UXO mine)
    {
        string pointName = mine.type[0].type;
        Point point = new Point();
        if (pointName != "")
            point.name = pointName;
        else
        {
            numberofminespoints++;
            point.name = "Point " + (numberofminespoints);
        }
        Vector2d pointCoordinates = new Vector2d(mine.latitude, mine.longitude);
        point.location = pointCoordinates;
        var instance = Instantiate(_minePrefab);
        instance.name = "Point " + point.name;
        instance.tag = "Point";
        instance.transform.localPosition = _map.GeoToWorldPosition(pointCoordinates, true);
        instance.transform.localScale = new Vector3((float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4));
        instance.transform.GetChild(3).gameObject.GetComponent<TextMesh>().text = point.name;
        instance.transform.GetChild(2).localScale = new Vector3(mine.radius / 1000, mine.radius / 1000, 0.1f);
        instance.transform.GetChild(2).gameObject.SetActive(true);
        string types="";
        foreach(MineProbability probability in mine.type) 
        {
            if (mine.type.IndexOf(probability) != mine.type.Count-1)
                types += probability.type + " -> " + probability.probability + "% / ";
            else
                types += probability.type + " -> " + probability.probability + "%";
        }
        instance.transform.GetChild(1).GetChild(0).gameObject.GetComponent<TextMesh>().text = types;
        instance.transform.GetChild(1).GetChild(1).gameObject.GetComponent<TextMesh>().text = mine.latitude + " " + mine.longitude;
        instance.transform.GetChild(1).GetChild(2).gameObject.GetComponent<TextMesh>().text = Math.Abs(mine.depth).ToString();
        point.pointObject = instance;
        minespoints.Add(point);
        if (minespoints.Count > 0)
            focusAtAll.SetActive(true);
    }

    public void SetRouteMarkers()
    {
        DeleteAllpathpoints();
        pathpoints = new List<Point>();
        MapController.instance.LocationFinder.GetLocation();
        //GPSPoint startCoords = new GPSPoint(MapController.instance.LocationFinder._latitude, MapController.instance.LocationFinder._longitude);
        GPSPoint startCoords = new GPSPoint(50.145,36.265);
        var mines = new List<Mine>();
        foreach (UXO mine in MapController.instance.UXOs.ToList())
        {
            mines.Add(new Mine(new GPSPoint(mine.latitude, mine.longitude), Math.Round(mine.radius)));
        }
        gameObject.GetComponent<VoronoiPathFinding>().mines = mines;
        gameObject.GetComponent<VoronoiPathFinding>().startPoint = startCoords;
        gameObject.GetComponent<VoronoiPathFinding>().StartPathfinding();
    }

    private void Update()
    {
        if (_map.gameObject.activeSelf)
        {
            foreach (Point p in minespoints)
            {
                p.pointObject.transform.localPosition = _map.GeoToWorldPosition(p.location, true);
                p.pointObject.transform.localScale = new Vector3((float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4));
            }
            foreach (Point p in pathpoints)
            {
                p.pointObject.transform.localPosition = _map.GeoToWorldPosition(p.location, true);
                p.pointObject.transform.localScale = new Vector3((float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4), (float)0.000002 * (float)Math.Pow(_map.Zoom, 4));
            }
        }
    }


    public void DeletePoint(Point pointData)
    {
        minespoints.Remove(pointData);
        GameObject pointLine = GameObject.Find(pointData.name + " data");
        pointLine.Destroy();
        pointData.pointObject.Destroy();
        if (minespoints.Count == 0)
            focusAtAll.SetActive(false);
    }

    public void DeleteAllminespoints()
    {
        foreach (Point point in minespoints)
        {
            point.pointObject.Destroy();
        }
        minespoints.Clear();
        focusAtAll.SetActive(false);
    }

    public void DeleteAllpathpoints()
    {
        foreach (Point point in pathpoints)
        {
            point.pointObject.Destroy();
        }
        pathpoints.Clear();
        focusAtAll.SetActive(false);
    }

}





public class Point
{
    /// <summary>
    /// Object of point
    /// </summary>
    public GameObject pointObject { get; set; }
    /// <summary>
    /// Point map location
    /// </summary>
    public Vector2d location { get; set; }
    /// <summary>
    /// Name of point
    /// </summary>
    public string name { get; set; }
}