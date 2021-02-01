using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CelestialName: MonoBehaviour
{
    //private string path = "Names/Stars/";
    [System.NonSerialized] public string title = "Error: could not load"; //default title
    string[] lines;
    string prefix = "";
    string main = "";
    string suffix = "";

    public TextAsset prefixesNumbers;
    public TextAsset prefixesLetters;
    public TextAsset mainModern;
    public TextAsset mainAncient;
    public TextAsset mainMassEffect;
    public TextAsset mainStarWars;
    public TextAsset mainTheoretical;
    public TextAsset suffixesGeneral;
    public TextAsset suffixesGreek;
    public TextAsset suffixesPhonecian;
    public TextAsset suffixesRomanNumerals;

    public void GenerateName()
    {
        /*
        TextAsset prefixesNumbers         = Resources.Load<TextAsset>(path + "Prefixes/numbers.txt");
        TextAsset prefixesLetters         = Resources.Load<TextAsset>(path + "Prefixes/letters.txt");
        TextAsset mainModern              = Resources.Load<TextAsset>(path + "Main/modern.txt");
        TextAsset mainAncient             = Resources.Load<TextAsset>(path + "Main/ancient.txt");
        TextAsset mainMassEffect          = Resources.Load<TextAsset>(path + "Main/massEffect.txt");
        TextAsset mainStarWars            = Resources.Load<TextAsset>(path + "Main/starWars.txt");
        TextAsset mainTheoretical         = Resources.Load<TextAsset>(path + "Main/theoretical.txt");
        TextAsset suffixesGeneral         = Resources.Load<TextAsset>(path + "Suffixes/general.txt");
        TextAsset suffixesGreek           = Resources.Load<TextAsset>(path + "Suffixes/greek.txt");
        TextAsset suffixesPhonecian       = Resources.Load<TextAsset>(path + "Suffixes/phoenician.txt");
        TextAsset suffixesRomanNumerals   = Resources.Load<TextAsset>(path + "Suffixes/romanNumerals.txt");
        */

        //Debug.Log("Generating celestial name" + prefixesNumbers.text);

        //Prefix
        if (Random.value >= 0.8f)
        {
            float prefixFile = Random.value;

            if (prefixFile >= 0f && prefixFile < 0.333f)
            {
                lines = prefixesNumbers.text.Split('\n');
            }
            else if (prefixFile >= 0.333f && prefixFile < 0.666f)
            {
                lines = prefixesLetters.text.Split('\n');
            }
            else //0.666f and 1f
            {
                //Main/modern instead of prefix/general
                lines = mainModern.text.Split('\n');
            }

            prefix = lines[(int)Random.Range(0f, lines.Length - 1f)].TrimStart('\r', '\n').TrimEnd('\r', '\n') + " ";
        }

        //Main
        float mainFile = Random.value;

        if (mainFile >= 0f && mainFile < 0.2f)
        {
            lines = mainAncient.text.Split('\n');
        }
        else if (mainFile >= 0.2f && mainFile < 0.4f)
        {
            lines = mainMassEffect.text.Split('\n');
        }
        else if (mainFile >= 0.4f && mainFile < 0.6f)
        {
            lines = mainModern.text.Split('\n');
        }
        else if (mainFile >= 0.6f && mainFile < 0.8f)
        {
            lines = mainStarWars.text.Split('\n');
        }
        else //0.8f and 1f
        {
            lines = mainTheoretical.text.Split('\n');
        }

        main = lines[(int)Random.Range(0f, lines.Length - 1f)].TrimStart('\r', '\n').TrimEnd('\r', '\n');

        //Suffix
        if (Random.value >= 0.8f)
        {
            float suffixFile = Random.value;

            if (suffixFile >= 0f && suffixFile < 0.25f)
            {
                lines = suffixesGeneral.text.Split('\n');
            }
            else if (suffixFile >= 0.25f && suffixFile < 0.5f)
            {
                lines = suffixesGreek.text.Split('\n');
            }
            else if (suffixFile >= 0.5f && suffixFile < 0.75f)
            {
                lines = suffixesPhonecian.text.Split('\n');
            }
            else //0.75f and 1f
            {
                lines = suffixesRomanNumerals.text.Split('\n');
            }

            suffix = " " + lines[(int)Random.Range(0f, lines.Length - 1f)].TrimStart('\r', '\n').TrimEnd('\r', '\n');
        }

        //Compile
        title = prefix + main + suffix;

        //Debug.Log(title);
    }
}
