using Mapbox.Unity.Map;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Drawing;
using System;
using System.IO;

public class MapController : MonoBehaviour
{
    /// <summary>
    /// Map instance
    /// </summary>
    public AbstractMap _map;

    public UIController UIController;

    public minespointsSpawn minespointsSpawn;

    public FocusOnPoint FocusOnPoint;

    public LocationFinder LocationFinder;

    public Vector2d standartLocation = new Vector2d(52.5244, 13.4105);

    public GameObject player;

    static MapController _instance;

    public GameObject MineInfo;

    public ImagerySourceType imagerySource = ImagerySourceType.MapboxStreets;

    public UXO[] UXOs;

    private void Start()
    {
       minespointsSpawn = GetComponent<minespointsSpawn>();
       FocusOnPoint = GetComponent<FocusOnPoint>();
    }

    /// <summary>
    /// Get instance of map contoller
    /// </summary>
    public static MapController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("MapController").GetComponent<MapController>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Open map for testing without markers
    /// </summary>
    public void OpenMapWithoutMarker()
    {
        StartCoroutine(InitializeMap());
    }

    /// <summary>
    /// Open map with several markers at it
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitute"></param>
    /// <param name="pointName"></param>
    public void OpenMapWithMarkers(UXO[] minesarray)
    {
        UXOs = minesarray;
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            minespointsSpawn.DeleteAllminespoints();
            minespointsSpawn.DeleteAllpathpoints();
            StartCoroutine(InitializeMap());
            for (int i = 0; i < minesarray.Length; i++)
            {
                minespointsSpawn.SetMine(minesarray[i]);
            }
            DeleteminespointsText();
            FocusOnPoint.FocusAtAll();
        }
    }

    /// <summary>
    /// Close map and back to main menu
    /// </summary>
    public void CloseMap()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            minespointsSpawn.DeleteAllminespoints();
            minespointsSpawn.DeleteAllpathpoints();
            _map.gameObject.SetActive(false);
            player.SetActive(false);
            StopCoroutine(UpdatePosition());
            StopCoroutine(GetGPSPosition());
        }
        UIController.DisableMapPage();
    }

    /// <summary>
    /// Initialize map and open it
    /// </summary>
    IEnumerator InitializeMap()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIController.EnableMapPage();
        }
        else
        {
            LocationFinder.GetLocation();
            UIController.EnableMapPage();
            _map.gameObject.SetActive(true);
            StartCoroutine(UpdatePosition());
            StartCoroutine(GetGPSPosition());
            player.SetActive(true);
            Vector2d Berlin = new Vector2d(52.51667, 13.38333);
            _map.Initialize(Berlin, 10);
            _map.gameObject.SetActive(true);
            MineInfo.SetActive(false);
            minespointsSpawn.DeleteAllminespoints();
            minespointsSpawn.DeleteAllpathpoints();
            yield return null;
        }
    }

    public void DeleteminespointsText()
    {
        MineInfo.SetActive(false);
        GameObject[] minespointsObjects = GameObject.FindGameObjectsWithTag("Point");
        foreach (GameObject point in minespointsObjects)
        {
            point.transform.GetChild(0).GetComponent<SpriteRenderer>().color = point.GetComponent<NavToGameObject>().standartColor;
            point.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void ChangeVisualisation()
    {
        imagerySource = (imagerySource == ImagerySourceType.MapboxSatelliteStreet) ? ImagerySourceType.MapboxStreets : imagerySource + 1;
        _map.ImageLayer.SetLayerSource(imagerySource);
    }


    IEnumerator UpdatePosition()
    {
        while (true)
        {
            if (_map.gameObject.activeSelf)
            {
                if (PlayerPrefs.GetFloat("lat", 0) != 0 && PlayerPrefs.GetFloat("lon", 0) != 0)
                    player.transform.localPosition = _map.GeoToWorldPosition(new Vector2d(PlayerPrefs.GetFloat("lat", 0), PlayerPrefs.GetFloat("lon", 0)));
                else
                    player.transform.localPosition = _map.GeoToWorldPosition(standartLocation);


            }
            yield return null;
        }
    }

    IEnumerator GetGPSPosition()
    {
        while (true)
        {
            if (_map.gameObject.activeSelf)
            {
                LocationFinder.GetLocation();
            }
            yield return new WaitForSecondsRealtime(15f);
        }
    }

    public float calculateKilometersFromCoordinates(float lat0, float lon0, float lat1, float lon1)
    {
        float R = 6371; // Radius of the earth in km
        float dLat = (float)(lat0 - lat1) * Mathf.Deg2Rad; // deg2rad below
        float dLon = (float)(lon0 - lon1) * Mathf.Deg2Rad;
        float a =
            Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2)
            + Mathf.Cos(lat1 * Mathf.Deg2Rad)
                * Mathf.Cos(lat0 * Mathf.Deg2Rad)
                * Mathf.Sin(dLon / 2)
                * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        float d = R * c; // Distance in km
        return d;
    }
}
