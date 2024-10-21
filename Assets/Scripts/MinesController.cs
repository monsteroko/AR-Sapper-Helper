using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using ARLocation;

public class MinesController : MonoBehaviour
{
    static List<string> mineTypes = new List<string>() { "“Ã-62", "PFM-1", "POMZ", "POM-3", "MON-50", "PMN", "MON-200"};

    public MapController MapController;

    public TMP_InputField filePath;

    public void OpenLocalFileWithMines()
    {
        MapController.OpenMapWithMarkers(DataGenerate());
    }

    public void OpenWebFileWithMines()
    {
        System.Net.WebClient wc = new System.Net.WebClient();
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            try
            {
                string webData = wc.DownloadString(filePath.text);
                if (webData != null)
                {
                    UXO[] data = JsonHelper.FromJson<UXO>(webData);
                    MapController.OpenMapWithMarkers(data);
                }
            }
            catch (Exception e)
            {
                filePath.text = LocalizationSettings.StringDatabase.GetLocalizedString("Menu Locaization Table", "Wrong path!");
            }


        }
    }

    public static UXO[] DataGenerate()
    {
        UXO[] mines = new UXO[UnityEngine.Random.Range(3, 6)];
        for (int i = 0; i < mines.Length; i++)
        {
            UXO mine = new UXO();
            List<MineProbability> keyValues = new List<MineProbability>();
            for (int j = 0; j < UnityEngine.Random.Range(1, 5); j++)
            {
                string key = mineTypes[UnityEngine.Random.Range(0, mineTypes.Count - 1)];
                bool isNew = true;
                foreach (MineProbability keyVal in keyValues)
                {
                    if (keyVal.type == key)
                        isNew = false;
                }
                if (isNew)
                {
                    MineProbability mineProbability = new MineProbability();
                    mineProbability.type = key;
                    mineProbability.probability = UnityEngine.Random.Range(0, 100);
                    keyValues.Add(mineProbability);
                }

            }
            mine.type = keyValues;
            mines[i] = mine;
        }
        return mines;
    }
}

[Serializable]
public class UXO
{
    public List<MineProbability> type = new List<MineProbability> ();
    public float latitude = UnityEngine.Random.Range((float)50.14, (float)50.15);
    public float longitude= UnityEngine.Random.Range((float)36.27, (float)36.30);
    public float depth = UnityEngine.Random.Range(-3, 0);
    public float radius = UnityEngine.Random.Range(50f, 200f);
}

[Serializable]
public class MineProbability
{
    public string type;
    public float probability;
}