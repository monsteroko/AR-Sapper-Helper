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

    public PointsSpawn PointsSpawn;

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
       PointsSpawn = GetComponent<PointsSpawn>();
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
            PointsSpawn.DeleteAllPoints();
            StartCoroutine(InitializeMap());
            for (int i = 0; i < minesarray.Length; i++)
            {
                PointsSpawn.SetMarker(minesarray[i]);
            }
            DeletePointsText();
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
            PointsSpawn.DeleteAllPoints();
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
            _map.Initialize(Berlin, 7);
            _map.gameObject.SetActive(true);
            MineInfo.SetActive(false);
            PointsSpawn.DeleteAllPoints();
            yield return null;
        }
    }

    public void DeletePointsText()
    {
        MineInfo.SetActive(false);
        GameObject[] pointsObjects = GameObject.FindGameObjectsWithTag("Point");
        foreach (GameObject point in pointsObjects)
        {
            point.transform.GetChild(1).GetComponent<SpriteRenderer>().color = point.GetComponent<NavToGameObject>().standartColor;
            point.transform.GetChild(2).gameObject.SetActive(false);
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
}
