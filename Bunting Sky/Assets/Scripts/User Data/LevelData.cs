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
        [SerializeField] public byte controlPlanetoidQuantity;
        [SerializeField] public float[,] controlPlanetoidPosition;
        [SerializeField] public float[,] controlPlanetoidVelocity;
        [SerializeField] public string[] controlPlanetoidName;
        [SerializeField] public bool[] controlPlanetoidHasStation;
        [SerializeField] public string controlCentreStarName;
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
    }

    public static Data LoadGame(string path)
    {
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
            return null;
        }
    }
}