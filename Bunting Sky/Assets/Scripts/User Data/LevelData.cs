using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class LevelData
{
    [System.Serializable]
    public class Data
    {
        //World properties
        //System
        [SerializeField] public string centreStarName;
        //Verse space
        [SerializeField] public float[] controlVerseSpacePosition;
        //Player
        [SerializeField] public float[] playerPosition;

        //Player properties
        [SerializeField] public float playerThrustEngineWarmupMultiplierMax;

        [SerializeField] public double playerVitalsHealth;
        [SerializeField] public double playerVitalsHealthMax;
        [SerializeField] public bool playerDestroyed;

        [SerializeField] public double playerVitalsFuel;
        [SerializeField] public double playerVitalsFuelMax;
        [SerializeField] public double playerVitalsFuelConsumptionRate;

        [SerializeField] public double playerCurrency;
        [SerializeField] public double[] playerOre;
    }

    public static void SaveGame(string path, Data data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("Saved at " + Time.time);
    }

    public static Data LoadGame(string path)
    {
        Debug.Log("Loaded at " + Time.time);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            Data data = formatter.Deserialize(stream) as Data;
            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("No save file found in " + path);
            return null;
        }
    }
}