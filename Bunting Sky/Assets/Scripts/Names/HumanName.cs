using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HumanName : MonoBehaviour
{
    private string path = "Names/Humans/";
    [System.NonSerialized] public string title = "Error: could not load"; //default title
    private string[] lines;
    private string partOne = "";
    private string partTwo = "";

    public TextAsset prefixesEnTitlesBusiness;
    public TextAsset prefixesEnTitlesCivilian;
    public TextAsset prefixesEnTitlesScience;
    public TextAsset prefixesEnTitlesReligious;
    public TextAsset prefixesEnTitlesMilitary;
    public TextAsset mainEnFirstFemale;
    public TextAsset mainEnFirstMale;
    public TextAsset mainEnFirstNB;
    public TextAsset suffixesEnLast;

    public void GenerateName()
    {
        /*
        TextAsset prefixesEnTitlesBusiness    = Resources.Load<TextAsset>(path + "Prefixes/enTitlesBusiness.txt");
        TextAsset prefixesEnTitlesCivilian    = Resources.Load<TextAsset>(path + "Prefixes/enTitlesCivilian.txt");
        TextAsset prefixesEnTitlesScience     = Resources.Load<TextAsset>(path + "Prefixes/enTitlesScience.txt");
        TextAsset prefixesEnTitlesReligious   = Resources.Load<TextAsset>(path + "Prefixes/enTitlesReligious.txt");
        TextAsset prefixesEnTitlesMilitary    = Resources.Load<TextAsset>(path + "Prefixes/enTitlesMilitary.txt");
        TextAsset mainEnFirstFemale           = Resources.Load<TextAsset>(path + "Main/enFirstFemale.txt");
        TextAsset mainEnFirstMale             = Resources.Load<TextAsset>(path + "Main/enFirstMale.txt");
        TextAsset mainEnFirstNB               = Resources.Load<TextAsset>(path + "Main/enFirstNB.txt");
        TextAsset suffixesEnLast              = Resources.Load<TextAsset>(path + "Suffixes/enLast.txt");
        */

        Debug.Log("Generating human name");

        //Part one
        if (Random.Range(0, 2) >= 1)
        {
            //Title
            switch (Random.Range(0, 8))
            {
                case 0:
                    lines = prefixesEnTitlesBusiness.text.Split('\n');
                    break;
                case 1:
                    lines = prefixesEnTitlesCivilian.text.Split('\n');
                    break;
                case 2:
                    lines = prefixesEnTitlesScience.text.Split('\n');
                    break;
                case 3:
                    lines = prefixesEnTitlesReligious.text.Split('\n');
                    break;
                //Weight military titles heavily
                case 4:
                case 5:
                case 6:
                case 7:
                    lines = prefixesEnTitlesMilitary.text.Split('\n');
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
                    lines = mainEnFirstFemale.text.Split('\n');
                    break;
                case 1:
                    lines = mainEnFirstMale.text.Split('\n');
                    break;
                case 2:
                    lines = mainEnFirstNB.text.Split('\n');
                    break;
                default:
                    Debug.Log("Out of range!");
                    break;
            }
        }

        partOne = lines[Random.Range(0, lines.Length)];

        //Part two: last name
        lines = suffixesEnLast.text.Split('\n');

        partTwo = lines[Random.Range(0, lines.Length)];

        //Compile
        title = partOne + " " + partTwo;
    }
}