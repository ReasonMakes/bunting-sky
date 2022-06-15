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
        //Star
        [SerializeField] public string starName;

        //Planets
        [SerializeField] public byte planetQuantity;
        [SerializeField] public byte[] planetarySystemMoonQuantity;
        [SerializeField] public float[,] planetPosition;
        [SerializeField] public string[] planetName;

        //Moons
        [SerializeField] public byte moonQuantity;
        [SerializeField] public byte moonIndex;
        [SerializeField] public float[,] moonPosition;
        [SerializeField] public string[] moonName;
        [SerializeField] public bool[] moonHasStation;

        //Moons: stations
        [SerializeField] public string[] stationTitle;
        [SerializeField] public float[] stationPricePlatinoid;
        [SerializeField] public float[] stationPricePreciousMetal;
        [SerializeField] public float[] stationPriceWater;
        [SerializeField] public int[,] stationUpgradeIndex;

        //Asteroids
        [SerializeField] public int asteroidQuantity;
        [SerializeField] public float[,] asteroidPosition;
        [SerializeField] public float[,] asteroidVelocity;
        [SerializeField] public int[] asteroidSize;
        [SerializeField] public byte[] asteroidType;
        [SerializeField] public byte[] asteroidHealth;
        
        //Verse space
        [SerializeField] public float[] verseSpacePosition;

        //Player
        [SerializeField] public float[] playerPosition;

        [SerializeField] public double playerVitalsHealth;
        [SerializeField] public bool playerDestroyed;
        [SerializeField] public double playerVitalsFuel;

        [SerializeField] public int[] playerUpgrades;
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