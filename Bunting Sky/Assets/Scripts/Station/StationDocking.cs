using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationDocking : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    private HumanName humanName;

    private bool host = false;

    [System.NonSerialized] public int[] upgradeIndexOfButton;
    [System.NonSerialized] public static int upgradeButtons = 4;

    [System.NonSerialized] public float pricePlatinoid = 30f;
    [System.NonSerialized] public float pricePreciousMetal = 60f;
    [System.NonSerialized] public float priceWater = 10f;

    private bool initialized = false;

    private void Awake()
    {
        //Init upgrade array
        upgradeIndexOfButton = new int[upgradeButtons];
    }

    private void Start()
    {
        //Get station name
        humanName = transform.parent.GetComponent<HumanName>();

        //Remember is initialized
        initialized = true;
    }

    public void GenerateCommerceOffers()
    {
        //Choose ore purchase offers
        pricePlatinoid = Random.Range(1, 7) * 10f;
        pricePreciousMetal = Random.Range(2, 13) * 10f;
        priceWater = Random.Range(1, 16);

        //Choose which upgrades this station has for sale
        for (int i = 0; i < upgradeButtons; i++)
        {
            //Generate random index
            upgradeIndexOfButton[i] = Random.Range(1, control.commerce.upgradeDictionary.GetLength(0));
            //Check if upgrade is a duplicate of a previous upgrade
            if (i > 0) //if we are at the first iteration, it is impossible to be a duplicate
            {
                for (int i2 = 0; i2 < i; i2++)
                {
                    if (upgradeIndexOfButton[i] == upgradeIndexOfButton[i2])
                    {
                        //Sold out
                        upgradeIndexOfButton[i] = 0;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (initialized)
        {
            //Debug.Log("Enter: " + other.gameObject.name);

            if (!Commerce.menuOpen && other.gameObject.name == "Body")
            {
                //Is host
                host = true;

                //Send name to commerce script
                control.commerce.stationName.text = humanName.title + "'s Drydock";
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

                //Open commerce menu
                control.commerce.MenuToggle();
            }
        }
    }

    /*
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
    */

    private void Update()
    {
        if (Commerce.menuOpen && host)
        {
            control.generation.instancePlayer.transform.Find("Body").transform.position = transform.position;
        }
        else
        {
            host = false;
        }
    }

    private void SendUpgradeButtonsToCommerce(int buttonIndex, Button commerceButton, out float commercePrice)
    {
        //Type
        commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = control.commerce.upgradeDictionary[upgradeIndexOfButton[buttonIndex], 0];
        
        //Text color and additions
        TextMeshProUGUI priceDisplay = commerceButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>();
        if (upgradeIndexOfButton[buttonIndex] == 0) //Sold out
        {
            commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = control.commerce.colorTextDisabled;
            priceDisplay.text = "N/A";
            priceDisplay.color = control.commerce.colorTextDisabled;

            //Price internal
            commercePrice = control.commerce.PRICE_UPGRADE_MAX;
        }
        else //Not sold out
        {
            commerceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = control.commerce.colorTextEnabled;
            priceDisplay.text = "$" + control.commerce.upgradeDictionary[upgradeIndexOfButton[buttonIndex], 1] + " ea";
            priceDisplay.color = control.commerce.colorTextEnabled;

            //Price internal
            //Debug.LogFormat("buttonIndex: {0}, upgradeIndexOfButton[buttonIndex]: {1}, control.commerce.upgradeDictionary[upgradeIndexOfButton[buttonIndex], 1]: {2}", buttonIndex, upgradeIndexOfButton[buttonIndex], control.commerce.upgradeDictionary[upgradeIndexOfButton[buttonIndex], 1]);
            commercePrice = float.Parse(control.commerce.upgradeDictionary[upgradeIndexOfButton[buttonIndex], 1]);
        }
    }
}