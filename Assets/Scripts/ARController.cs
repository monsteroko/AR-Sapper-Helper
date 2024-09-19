using ARLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ARController : MonoBehaviour
{
    public GameObject UICamera, AR;

    public GameObject MainUI, ARUI, Map, player;

    public MapController MapController;

    public List<GameObject> minesModels;

    public void OpenAR()
    {
        MainUI.SetActive(false);
        ARUI.SetActive(true);
        UICamera.SetActive(false);
        player.SetActive(false);
        Map.SetActive(false);
        AR.SetActive(true);
        foreach(GameObject point in GameObject.FindGameObjectsWithTag("Point"))
            point.transform.localScale = new Vector3(0, 0, 0);
        foreach(UXO mine in MapController.UXOs)
        {
            foreach(GameObject mineinstance in minesModels)
            {
                if(mineinstance.name == mine.type[0].type)
                {
                    var instance = Instantiate(mineinstance);
                    instance.name = "Mine " + mine.type[0].type;
                    instance.tag = "Mine";
                    float yRotation = Camera.main.transform.eulerAngles.y;
                    instance.transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
                    instance.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text = LocalizationSettings.StringDatabase.GetLocalizedString("Menu Locaization Table", "Types & probabilities:");
                    foreach(MineProbability info in mine.type)
                        instance.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text += info.type + "=>" + info.probability + "%; ";
                    instance.transform.GetChild(1).GetChild(1).GetComponent<TextMesh>().text = "GPS: " + mine.latitude + ";" + mine.longitude;
                    instance.transform.GetChild(1).GetChild(2).GetComponent<TextMesh>().text = LocalizationSettings.StringDatabase.GetLocalizedString("Menu Locaization Table", "Depth:") + Math.Abs(mine.depth);
                    instance.GetComponent<PlaceAtLocation>().Location = new Location(mine.latitude, mine.longitude, 0);
                    instance.SetActive(true);
                }
            }

        }
        var KH = Instantiate(minesModels[2]);
        KH.name = "Mine " + minesModels[2].name;
        KH.tag = "Mine";
        float khRotation = Camera.main.transform.eulerAngles.y;
        KH.transform.eulerAngles = new Vector3(transform.eulerAngles.x, khRotation, transform.eulerAngles.z);
        KH.transform.GetChild(1).GetChild(0).GetComponent<TextMesh>().text = minesModels[2].name + "=>" + "83" + "%";
        KH.transform.GetChild(1).GetChild(1).GetComponent<TextMesh>().text = "GPS: 50.03244;36.211151";
        KH.transform.GetChild(1).GetChild(2).GetComponent<TextMesh>().text = "Depth: 0";
        KH.GetComponent<PlaceAtLocation>().Location = new Location(50.032446, 36.211151, 0);
        KH.SetActive(true);
    }
    public void CloseAR()
    {
        foreach (GameObject mines in GameObject.FindGameObjectsWithTag("Mine"))
            mines.Destroy();
        ARUI.SetActive(false);
        MainUI.SetActive(true);
        AR.SetActive(false);
        Map.SetActive(true);
        player.SetActive(true);
        UICamera.SetActive(true);
        foreach (GameObject point in GameObject.FindGameObjectsWithTag("Point"))
            point.transform.localScale = new Vector3(MapController.minespointsSpawn._spawnScale, MapController.minespointsSpawn._spawnScale, MapController.minespointsSpawn._spawnScale);
    }
}
