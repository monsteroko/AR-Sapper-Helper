using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class NavToGameObject : MonoBehaviour
{
    public UnityEvent unityEvent = new UnityEvent();

    public Color standartColor;
    public Color pushedColor;

    private MapController MapController;
    private GameObject point;
    void Start()
    {
        MapController = GameObject.FindObjectOfType<MapController>();
        point = this.gameObject;
    }

    void Update()
    {
        GameObject camera = GameObject.Find("UICamera");
        if (camera != null)
        {
            Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == point.transform.GetChild(1).gameObject)
                {
                    MapController.DeletePointsText();
                    MapController.MineInfo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = LocalizationSettings.StringDatabase.GetLocalizedString("Menu Locaization Table", "Types & probabilities:") + point.transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMesh>().text;
                    MapController.MineInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "GPS: \n" + point.transform.GetChild(2).GetChild(1).gameObject.GetComponent<TextMesh>().text + LocalizationSettings.StringDatabase.GetLocalizedString("Menu Locaization Table", "Depth:") +
                        point.transform.GetChild(2).GetChild(2).gameObject.GetComponent<TextMesh>().text;
                    point.transform.GetChild(1).GetComponent<SpriteRenderer>().color = pushedColor;
                    MapController.MineInfo.SetActive(true);
                    //unityEvent.Invoke();
                }
            }
        }
    }
}