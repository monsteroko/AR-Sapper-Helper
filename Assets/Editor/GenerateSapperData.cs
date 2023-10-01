using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GenerateSapperData : MonoBehaviour
{

    [MenuItem("Window/Make Mines JSON")]
    static void MakeJSON()
    {

        UXO[] mines = MinesController.DataGenerate();
        string data = JsonHelper.ToJson(mines, true);
        string fileName = "Assets/Locations.json";
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (FileStream fs = File.Create(fileName))
            {
                Byte[] title = new UTF8Encoding(true).GetBytes(data);
                fs.Write(title, 0, title.Length);
                Debug.Log("Mines JSON generated");
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex.ToString());
        }
    }

}
