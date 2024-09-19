using UnityEngine;
using System.Text.RegularExpressions;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;
using Mapbox.Examples;
using Mapbox.Geocoding;
using TMPro;
using System;
using System.Drawing;
using System.Linq;

public class FocusOnPoint : MonoBehaviour
{

    public Camera _camera;
    public AbstractMap _map;
    public GameObject _player;

    private minespointsSpawn minespointsSpawn;

    private void Start()
    {
        minespointsSpawn = GetComponent<minespointsSpawn>();
    }


    public void FocusAt(Vector2d coordinates)
    {
        float zoom = _map.Zoom;
        _map.UpdateMap(coordinates, zoom);
    }

    public void FocusAtAll()
    {
        var x_query = from Point p in minespointsSpawn.minespoints select p.location.x;
        double xmin = x_query.Min();
        double xmax = x_query.Max();

        var y_query = from Point p in minespointsSpawn.minespoints select p.location.y;
        double ymin = y_query.Min();
        double ymax = y_query.Max();

        Rect bordersbox = new Rect((float)xmin, (float)ymin, (float)(xmax - xmin), (float)(ymax - ymin));
        Vector2d midpoint = new Vector2d(bordersbox.center.x, bordersbox.center.y);
        double zoom = 7 - Math.Log(Math.Max(bordersbox.width, bordersbox.height), 2);
        if (zoom < 14)
            _map.UpdateMap(midpoint, (float)zoom);
        else
            _map.UpdateMap(midpoint, 14);
    }

    public void FocusOnPlayer()
    {
        Vector2d playerCoordinates = _map.WorldToGeoPosition(_player.transform.position);
        FocusAt(playerCoordinates);
    }
}
