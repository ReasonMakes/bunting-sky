using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationDocking : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private StationName stationName;

    private bool host = false;

    [System.NonSerialized] public string[,] upgradeButton;
    
    [System.NonSerialized] public float pricePlatinoid = 30f;
    [System.NonSerialized] public float pricePreciousMetal = 60f;
    [System.NonSerialized] public float priceWater = 10f;

    private void Start()
    {
        //Get station name
        stationName = transform.parent.GetComponent<StationName>();

        //Choose ore purchase offers
        pricePlatinoid = Random.Range(1, 7) * 10f;
        pricePreciousMetal = Random.Range(2, 13) * 10f;
        priceWater = Random.Range(1, 16);

        //Choose which upgrades this station has for sale
        upgradeButton = new string[4, 3];
        for (int i = 0; i < upgradeButton.GetLength(0); i++)
        {
            //Generate random index
            int upgradeSelected = Random.Range(1, control.commerce.upgradeDictionary.GetLength(0));

            //Assign upgrade at index
            upgradeButton[i, 0] = control.commerce.upgradeDictionary[upgradeSelected, 0]; //Name
            upgradeButton[i, 1] = control.commerce.upgradeDictionary[upgradeSelected, 1]; //Price
            upgradeButton[i, 2] = control.commerce.upgradeDictionary[upgradeSelected, 2]; //Description

            //Check if upgrade is a duplicate of a previous upgrade
            if (i > 0) //if we are at the first iteration, it is impossible to be a duplicate
            {
                for (int i2 = 0; i2 < i; i2++)
                {
                    if (upgradeButton[i, 0] == upgradeButton[i2, 0])
                    {
                        upgradeButton[i, 0] = "Sold out";                                       //Name
                        upgradeButton[i, 1] = control.commerce.priceUpgradeMax.ToString();      //Price
                        upgradeButton[i, 2] = "Item is out of stock";                           //Description
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Enter: " + other.gameObject.name);

        if (!Commerce.menuOpen && other.gameObject.name == "Body")
        {
            //Is host
            host = true;

            //Send name to commerce script
            control.commerce.stationName.text = stationName.title + "'s Drydock";
            control.commerce.pricePlatinoid = pricePlatinoid;
            control.commerce.pricePreciousMetal = pricePreciousMetal;
            control.commerce.priceWater = priceWater;

            //Send ore purchase offers to commerce script
            control.commerce.pricePlatinoid = pricePlatinoid;
            control.commerce.pricePreciousMetal = pricePreciousMetal;
            control.commerce.priceWater = priceWater;

            //Send upgrades to commerce script
            SendUpgradeButtonsToCommerce(0, control.commerce.menuButtonUpgrade1, out control.commerce.priceUpgrade1);
            SendUpgradeButtonsToCommerce(1, control.commerce.menuButtonUpgrade2, out control.commerce.priceUpgrade2);
            SendUpgradeButtonsToCommerce(2, control.commerce.menuButtonUpgrade3, out control.commerce.priceUpgrade3);
            SendUpgradeButtonsToCommerce(3, control.commerce.menuButtonUpgrade4, out control.commerce.priceUpgrade4);
            
            /*
            control.commerce.menuButtonUpgrade2.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeButton[1, 0];                        //Name
            control.commerce.menuButtonUpgrade2.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + upgradeButton[1, 1] + " ea";  //Price display
            control.commerce.priceUpgrade2 = float.Parse(upgradeButton[1, 1]);                                                                            //Price internal
            
            control.commerce.menuButtonUpgrade3.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeButton[2, 0];                        //Name
            control.commerce.menuButtonUpgrade3.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + upgradeButton[2, 1] + " ea";  //Price display
            control.commerce.priceUpgrade3 = float.Parse(upgradeButton[2, 1]);                                                                            //Price internal

            control.commerce.menuButtonUpgrade4.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeButton[3, 0];                        //Name
            control.commerce.menuButtonUpgrade4.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + upgradeButton[3, 1] + " ea";  //Price display
            control.commerce.priceUpgrade4 = float.Parse(upgradeButton[3, 1]);                                                                            //Price internal
            */

            //Open commerce menu
            control.commerce.MenuToggle();
        }
    }

    private void OnTriggerEXit(Collider other)
    {
        //Debug.Log("Exit: " + other.gameObject.name);

        if(other.gameObject.name == "Body")
        {
            //No longer host
            host = false;

            //Unlock commerce menu
            Commerce.menuLocked = false;
        }
    }

    private void Update()
    {
        if (Commerce.menuOpen && host)
        {
            control.instancePlayer.transform.Find("Body").transform.position = transform.position;
        }
        else
        {
            host = false;
        }
    }

    private void SendUpgradeButtonsToCommerce(int stationUpgradeIndex, Button commerceButton, out float commercePrice)
    {
        //Type
        commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeButton[stationUpgradeIndex, 0];
        
        //Text color and additions
        TextMeshProUGUI priceDisplay = commerceButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>();
        if (float.Parse(upgradeButton[stationUpgradeIndex, 1]) >= control.commerce.priceUpgradeMax)
        {
            commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = control.commerce.colorTextDisabled;
            priceDisplay.text = "N/A";
            priceDisplay.color = control.commerce.colorTextDisabled;
        }
        else
        {
            commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = control.commerce.colorTextEnabled;
            priceDisplay.text = "$" + upgradeButton[stationUpgradeIndex, 1] + " ea";
            priceDisplay.color = control.commerce.colorTextEnabled;
        }

        //Price internal
        commercePrice = float.Parse(upgradeButton[stationUpgradeIndex, 1]);
    }
}