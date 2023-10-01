using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Rendering;

public class LocationFinder : MonoBehaviour
{

    public float _latitude = 0f;
    public float _longitude = 0f;

    public int isLocationEnabled = -1;

    public bool debugData = false;

    bool myLocationscreen = false;


    IEnumerator LocationCoroutine()
    {
        //Uncomment if you want to test with Unity Remote
#if UNITY_EDITOR
        yield return new WaitWhile(() => !UnityEditor.EditorApplication.isRemoteConnected);
        yield return new WaitForSecondsRealtime(5f);
#endif
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        // if (
        //     !UnityEngine.Android.Permission.HasUserAuthorizedPermission(
        //         UnityEngine.Android.Permission.CoarseLocation
        //     )
        // )
        // {
        //     UnityEngine.Android.Permission.RequestUserPermission(
        //         UnityEngine.Android.Permission.CoarseLocation
        //     );
        // }

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser)
        {
            // TODO Failure
            ////deb.text = "Location service is not enabled";
            Debug.LogFormat("Android and Location not enabled");
            isLocationEnabled = 0;
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser)
        {
            // TODO Failure
            isLocationEnabled = 0;
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
#endif
        // Start service before querying location
        UnityEngine.Input.location.Start(5f, 5f);

        ////deb.text = UnityEngine.Input.location.status.ToString();

        // Wait until service initializes
        int maxWait = 15;
        while (
            UnityEngine.Input.location.status == LocationServiceStatus.Initializing && maxWait > 0
        )
        {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // Editor has a bug which doesn't set the service status to Initializing. So extra wait in Editor.
#if UNITY_EDITOR
        int editorMaxWait = 15;
        while (
            UnityEngine.Input.location.status == LocationServiceStatus.Stopped && editorMaxWait > 0
        )
        {
            yield return new WaitForSecondsRealtime(1);
            editorMaxWait--;
        }
#endif

        // Service didn't initialize in 15 seconds
        if (maxWait < 1)
        {
            isLocationEnabled = 0;
            if (debugData)
                Debug.LogFormat("Timed out");
            if (myLocationscreen)
            {
                StartCoroutine(MyLocationCoroutine());
                myLocationscreen = false;
            }
            yield break;
        }

        // Connection has failed
        //if (UnityEngine.Input.location.status != LocationServiceStatus.Running)
        if (UnityEngine.Input.location.status == LocationServiceStatus.Failed)
        {
            isLocationEnabled = 0;
            if (debugData)
                Debug.LogFormat(
                    "Unable to determine device location. Failed with status {0}",
                    UnityEngine.Input.location.status
                );
            if (myLocationscreen)
            {
                StartCoroutine(MyLocationCoroutine());
                myLocationscreen = false;
            }
            yield break;
        }
        else
        {
            if (debugData)
                Debug.LogFormat("Location service live. status {0}", UnityEngine.Input.location.status);
            if (debugData)
                Debug.LogFormat(
                    "Location: "
                        + UnityEngine.Input.location.lastData.latitude
                        + " "
                        + UnityEngine.Input.location.lastData.longitude
                        + " "
                        + UnityEngine.Input.location.lastData.altitude
                        + " "
                        + UnityEngine.Input.location.lastData.horizontalAccuracy
                        + " "
                        + UnityEngine.Input.location.lastData.timestamp
                );
            if (UnityEngine.Input.location.isEnabledByUser && UnityEngine.Input.location.status == LocationServiceStatus.Running)
            {
                _latitude = UnityEngine.Input.location.lastData.latitude;
                PlayerPrefs.SetFloat("lat", _latitude);
                _longitude = UnityEngine.Input.location.lastData.longitude;
                PlayerPrefs.SetFloat("lon", _longitude);
            }

            if (myLocationscreen)
            {
                StartCoroutine(MyLocationCoroutine());
                myLocationscreen = false;
            }
            isLocationEnabled = 1;
        }
        UnityEngine.Input.location.Stop();
    }

    public void GetLocation()
    {
        if (PlayerPrefs.GetString("city", "") == "")
            StartCoroutine(LocationCoroutine());
        else
        {
            _latitude = PlayerPrefs.GetFloat("lat");
            _longitude = PlayerPrefs.GetFloat("lon");
        }
    }
    public void MyLocation()
    {
        PlayerPrefs.SetString("city", "");
#if UNITY_ANDROID
        if (
            UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.CoarseLocation
            )
        )
        {
#endif
            myLocationscreen = true;
            StartCoroutine(LocationCoroutine());
            StartCoroutine(MyLocationCoroutine());

#if UNITY_ANDROID
        }
#endif
    }

    IEnumerator MyLocationCoroutine()
    {
        yield return new WaitUntil(() => !myLocationscreen || !UnityEngine.Input.location.isEnabledByUser);
    }
}

