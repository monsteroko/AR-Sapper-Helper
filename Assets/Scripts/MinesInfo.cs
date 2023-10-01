using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinesInfo : MonoBehaviour
{
    public void NextMine()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                transform.GetChild(i).gameObject.SetActive(false);
                if (i==transform.childCount-1)
                    transform.GetChild(0).gameObject.SetActive(true);
                else
                    transform.GetChild(i+1).gameObject.SetActive(true);
                break;
            }
        }
    }

    public void PrevMine()
    {
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                transform.GetChild(i).gameObject.SetActive(false);
                if (i == 0)
                    transform.GetChild(transform.childCount-1).gameObject.SetActive(true);
                else
                    transform.GetChild(i - 1).gameObject.SetActive(true);
                break;
            }
        }
    }
}
