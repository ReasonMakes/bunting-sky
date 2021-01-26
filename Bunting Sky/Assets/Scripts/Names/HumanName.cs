using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HumanName : MonoBehaviour
{
    private string path = "Assets/Names/Humans/";
    [System.NonSerialized] public string title = "Error: could not load"; //default title
    private string[] lines;
    private string partOne = "";
    private string partTwo = "";

    public void GenerateName()
    {
        //Part one
        if(Random.Range(0, 2) >= 1)
        {
            //Title
            switch (Random.Range(0, 8))
            {
                case 0:
                    lines = System.IO.File.ReadAllLines(path + "Prefixes/enTitlesBusiness.txt");
                    break;
                case 1:
                    lines = System.IO.File.ReadAllLines(path + "Prefixes/enTitlesCivilian.txt");
                    break;
                case 2:
                    lines = System.IO.File.ReadAllLines(path + "Prefixes/enTitlesScience.txt");
                    break;
                case 3:
                    lines = System.IO.File.ReadAllLines(path + "Prefixes/enTitlesReligious.txt");
                    break;
                //Weight military titles heavily
                case 4:
                case 5:
                case 6:
                case 7:
                    lines = System.IO.File.ReadAllLines(path + "Prefixes/enTitlesMilitary.txt");
                    break;
                default:
                    Debug.Log("Out of range!");
                    break;
            }
        }
        else
        {
            //First name
            switch (Random.Range(0, 3))
            {
                case 0:
                    lines = System.IO.File.ReadAllLines(path + "Main/enFirstFemale.txt");
                    break;
                case 1:
                    lines = System.IO.File.ReadAllLines(path + "Main/enFirstMale.txt");
                    break;
                case 2:
                    lines = System.IO.File.ReadAllLines(path + "Main/enFirstNB.txt");
                    break;
                default:
                    Debug.Log("Out of range!");
                    break;
            }
        }

        partOne = lines[Random.Range(0, lines.Length)];

        //Part two: last name
        lines = System.IO.File.ReadAllLines(path + "Suffixes/enLast.txt");

        partTwo = lines[Random.Range(0, lines.Length)];

        //Compile
        title = partOne + " " + partTwo;
    }
}