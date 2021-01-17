using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StationName : MonoBehaviour
{
    public string title = "Error: could not load";
    string[] lines;
    string prefix = "";
    string main = "";
    string suffix = "";

    void Start()
    {
        /*
        prefix = "";
        main = "";
        suffix = "";
        */

        //Prefix
        if (Random.value >= 0.8f)
        {
            float prefixFile = Random.value;

            if (prefixFile >= 0f && prefixFile < 0.333f)
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Prefixes/numbers.txt");
            }
            else if (prefixFile >= 0.333f && prefixFile < 0.666f)
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Prefixes/letters.txt");
            }
            else //0.666f and 1f
            {
                //Main/modern instead of prefix/general
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/modern.txt");
            }

            prefix = lines[(int)Random.Range(0f, lines.Length - 1f)] + " ";
        }

        //Main
        float mainFile = Random.value;

        if (mainFile >= 0f && mainFile < 0.2f)
        {
            lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/ancient.txt");
        }
        else if (mainFile >= 0.2f && mainFile < 0.4f)
        {
            lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/massEffect.txt");
        }
        else if (mainFile >= 0.4f && mainFile < 0.6f)
        {
            lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/modern.txt");
        }
        else if (mainFile >= 0.6f && mainFile < 0.8f)
        {
            lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/starWars.txt");
        }
        else //0.8f and 1f
        {
            lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Main/theoretical.txt");
        }

        main = lines[(int)Random.Range(0f, lines.Length - 1f)];

        //Suffix
        if (Random.value >= 0.8f)
        {
            float suffixFile = Random.value;

            if (suffixFile >= 0f && suffixFile < 0.25f)
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Suffixes/general.txt");
            }
            else if (suffixFile >= 0.25f && suffixFile < 0.5f)
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Suffixes/greek.txt");
            }
            else if (suffixFile >= 0.5f && suffixFile < 0.75f)
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Suffixes/phoenician.txt");
            }
            else //0.75f and 1f
            {
                lines = System.IO.File.ReadAllLines("Assets/Names/Stars/Suffixes/romanNumerals.txt");
            }

            suffix = " " + lines[(int)Random.Range(0f, lines.Length - 1f)];
        }

        //Compile
        title = prefix + main + suffix;
    }
}
