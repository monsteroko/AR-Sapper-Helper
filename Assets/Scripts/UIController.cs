using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System;
using UnityEngine.Events;
using ARLocation;

public class UIController : MonoBehaviour
{
    public GameObject MainUI, OptionsUI, FileUI, MapUI, Pages, MinesUI;

    private void Start()
    {
        if (PlayerPrefs.GetString("selected-locale") == null)
        {
            PlayerPrefs.SetString("selected-locale", "en");
        }
        else
        {
            if (PlayerPrefs.GetString("selected-locale") == "uk-UA")
            {
                Debug.Log(PlayerPrefs.GetString("selected-locale"));
                SelectUkrainian();
            }
            if (PlayerPrefs.GetString("selected-locale") == "en")
            {
                SelectEnglish();
            }
        }
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.ACCESS_FINE_LOCATION"))
            {
                OptionsUI.transform.GetChild(3).gameObject.SetActive(true);
            }
    }

    public void SelectEnglish()
    {
        PlayerPrefs.SetString("selected-locale", "en");
        LoadLocale("en");
    }

    public void SelectUkrainian()
    {
        PlayerPrefs.SetString("selected-locale", "uk-UA");
        LoadLocale("uk-UA");
    }

    private void LoadLocale(string languageIdentifier)
    {
        LocalizationSettings settings = LocalizationSettings.Instance;
        LocaleIdentifier localeCode = new LocaleIdentifier(languageIdentifier);
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
            LocaleIdentifier anIdentifier = aLocale.Identifier;
            if (anIdentifier == localeCode)
            {
                LocalizationSettings.SelectedLocale = aLocale;
            }
        }
    }
    public void Exit()
    {
        Application.Quit();
    }

    public void OpenMenuUI()
    {
        MainUI.SetActive(true);
        OptionsUI.SetActive(false);
        FileUI.SetActive(false);
    }

    public void OpenFileUI()
    {
        MainUI.SetActive(false);
        OptionsUI.SetActive(false);
        FileUI.SetActive(true);
    }

    public void OpenOptionsUI()
    {
        MainUI.SetActive(false);
        OptionsUI.SetActive(true);
        FileUI.SetActive(false);
    }
    public void EnableMapPage()
    {
        Pages.SetActive(false);
        MapUI.SetActive(true);
    }
    public void DisableMapPage()
    {
        MapUI.SetActive(false);
        Pages.SetActive(true);
    }
    public void EnableMinesPage()
    {
        Pages.SetActive(false);
        MinesUI.SetActive(true);
    }
    public void DisableMinesPage()
    {
        MinesUI.SetActive(false);
        Pages.SetActive(true);
    }

    public void GiveGPSAccess() 
    {
        StopCoroutine(CloseGPSButton());
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.ACCESS_FINE_LOCATION"))
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
        }
        StartCoroutine(CloseGPSButton());
    }
    IEnumerator CloseGPSButton()
    {
        yield return new WaitUntil(() => (UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.ACCESS_FINE_LOCATION")));
        OptionsUI.transform.GetChild(3).gameObject.SetActive(false);
    }
}
