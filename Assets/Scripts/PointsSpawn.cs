using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;

public class PointsSpawn : MonoBehaviour
{

    public AbstractMap _map;
    public GameObject _markerPrefab;

    /// <summary>
    /// Size of marker
    /// </summary>
    public float _spawnScale = 0.3f;

    public GameObject focusAtAll;


    public List<Point> points;

    private int numberofPoints = 0;

    public GameObject dataPrefab;


    private void Start()
    {
        points = new List<Point>();
    }

    public void SetMarker(UXO mine)
    {
        string pointName = mine.type[0].type;
        Point point = new Point();
        if (pointName != "")
            point.name = pointName;
        else
        {
            numberofPoints++;
            point.name = "Point " + (numberofPoints);
        }
        Vector2d pointCoordinates = new Vector2d(mine.latitude, mine.longitude);
        point.location = pointCoordinates;
        var instance = Instantiate(_markerPrefab);
        instance.name = "Point " + point.name;
        instance.tag = "Point";
        instance.transform.localPosition = _map.GeoToWorldPosition(pointCoordinates, true);
        instance.transform.localScale = new Vector3(_spawnScale, _spawnScale , _spawnScale);
        instance.transform.GetChild(3).gameObject.GetComponent<TextMesh>().text = point.name;
        instance.transform.GetChild(2).localScale = new Vector3(mine.radius, mine.radius, 0.1f);
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
        points.Add(point);
        if (points.Count > 0)
            focusAtAll.SetActive(true);
    }

    private void Update()
    {
        if (_map.gameObject.activeSelf)
        {
            foreach (Point p in points)
            {
                p.pointObject.transform.localPosition = _map.GeoToWorldPosition(p.location, true);
                p.pointObject.transform.localScale = new Vector3(_spawnScale, _spawnScale , _spawnScale);
            }
        }
    }


    public void DeletePoint(Point pointData)
    {
        points.Remove(pointData);
        GameObject pointLine = GameObject.Find(pointData.name + " data");
        pointLine.Destroy();
        pointData.pointObject.Destroy();
        if (points.Count == 0)
            focusAtAll.SetActive(false);
    }

    public void DeleteAllPoints()
    {
        points.RemoveRange(0, points.Count);
        GameObject[] pointsGameObjects = GameObject.FindGameObjectsWithTag("Point");
        foreach (GameObject point in pointsGameObjects)
        {
            point.Destroy();
        }
        
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