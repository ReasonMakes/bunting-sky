using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Commerce : MonoBehaviour
{
    public static bool menuOpen = false;
    public static bool menuLocked = false;

    [SerializeField] private Control control;

    public TextMeshProUGUI stationName;

    public Color colorTextDisabled = new Color(1f, 1f, 1f, 0.1f);
    public Color colorTextEnabled = new Color(1f, 1f, 1f, 1f);

    public GameObject menuContainer;

    public Button menuButtonSellAllPlatinoid;
    public Button menuButtonSellAllPreciousMetal;
    public Button menuButtonSellAllWater;
    public float pricePlatinoid = 30f;
    public float pricePreciousMetal = 60f;
    public float priceWater = 10f;

    [System.NonSerialized] public string[,] upgradeDictionary;
    public Button menuButtonUpgrade1;
    public Button menuButtonUpgrade2;
    public Button menuButtonUpgrade3;
    public Button menuButtonUpgrade4;
    public readonly float PRICE_UPGRADE_MAX = 1e6f; //Setting to this price will be interpretted as an unavailable item
    public float priceUpgrade1 = 42069f;
    public float priceUpgrade2 = 42069f;
    public float priceUpgrade3 = 42069f;
    public float priceUpgrade4 = 42069f;

    public Button menuButtonRepair;
    public Button menuButtonRefuel;
    private float priceRepair = 10f;
    private float priceRefuel = 10f;

    private void Awake()
    {
        DefineUpgrades();
    }

    private void Update()
    {
        //ESC (default) only closes the menu - it does not open it
        if (!Menu.menuOpenAndGamePaused && menuOpen && control.binds.GetInputDown(control.binds.bindToggleMenu))
        {
            MenuToggle();
        }
    }

    //GENERAL
    private void DefineUpgrades()
    {
        //Initializations
        upgradeDictionary = new string[8, 4];
        
        //Definitions
        //Name                                              Price                                                     Description                                                                                                   Max level
        upgradeDictionary[0, 0] = "Sold out";               upgradeDictionary[0, 1] = PRICE_UPGRADE_MAX.ToString();   upgradeDictionary[0, 2] = "Item is out of stock";                                                             upgradeDictionary[0, 3] = "1";
        upgradeDictionary[1, 0] = "Fuel efficiency";        upgradeDictionary[1, 1] = "1000";                         upgradeDictionary[1, 2] = "Fuel is consumed at half the standard rate for the same resultant thrust";         upgradeDictionary[1, 3] = "1";
        upgradeDictionary[2, 0] = "Titan fuel tank";        upgradeDictionary[2, 1] = "500";                          upgradeDictionary[2, 2] = "Double sized fuel tank with a superior design resulting in the same net weight";   upgradeDictionary[2, 3] = "1";
        upgradeDictionary[3, 0] = "Reinforced hull";        upgradeDictionary[3, 1] = "500";                          upgradeDictionary[3, 2] = "Double strength outer hull";                                                       upgradeDictionary[3, 3] = "1";
        upgradeDictionary[4, 0] = "Raptor engines";         upgradeDictionary[4, 1] = "1000";                         upgradeDictionary[4, 2] = "1.5x greater maximum forward thrust output";                                       upgradeDictionary[4, 3] = "1";
        upgradeDictionary[5, 0] = "Dual batteries";         upgradeDictionary[5, 1] = "200";                          upgradeDictionary[5, 2] = "Laser weapon has double maximum ammunition per cycle";                             upgradeDictionary[5, 3] = "1";
        upgradeDictionary[6, 0] = "In situ fuel refinery";  upgradeDictionary[6, 1] = "5000";                         upgradeDictionary[6, 2] = "Automatically process water into usable jet fuel anywhere";                        upgradeDictionary[6, 3] = "1";
        upgradeDictionary[7, 0] = "Warp drive";             upgradeDictionary[7, 1] = "3000";                         upgradeDictionary[7, 2] = "Enables extra-dimensional interstellar travel through the bulk";                   upgradeDictionary[7, 3] = "1";
    }

    //MENU
    public void MenuToggle()
    {
        //Toggle menu
        menuOpen = !menuOpen;

        //Lock closed (resets when player re-enters docking area)
        if (!menuOpen)
        {
            menuLocked = true;
        }

        //Toggle UI
        menuContainer.SetActive(menuOpen);

        //Update UI
        UpdateCommerceMenuUI();

        //Toggle cursor lock
        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;

        //Toggle reticle
        control.ui.cameraReticle.SetActive(!menuOpen);
    }

    private void UpdatePlayerResourcesAndCommerceMenuUI()
    {
        control.ui.UpdateAllPlayerResourcesUI();
        UpdateCommerceMenuUI();
    }

    private void UpdateCommerceMenuUI()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //Sell
        menuButtonSellAllPlatinoid.interactable = playerScript.ore[0] > 0.0;
        menuButtonSellAllPreciousMetal.interactable = playerScript.ore[1] > 0.0;
        menuButtonSellAllWater.interactable = playerScript.ore[2] > 0.0;

        menuButtonSellAllPlatinoid.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + pricePlatinoid.ToString() + " / g";
        menuButtonSellAllPreciousMetal.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + pricePreciousMetal.ToString() + " / g";
        menuButtonSellAllWater.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + priceWater.ToString() + " / g";

        //Upgrade
        menuButtonUpgrade1.interactable = (priceUpgrade1 < PRICE_UPGRADE_MAX) && (playerScript.currency >= priceUpgrade1);
        menuButtonUpgrade2.interactable = (priceUpgrade2 < PRICE_UPGRADE_MAX) && (playerScript.currency >= priceUpgrade2);
        menuButtonUpgrade3.interactable = (priceUpgrade3 < PRICE_UPGRADE_MAX) && (playerScript.currency >= priceUpgrade3);
        menuButtonUpgrade4.interactable = (priceUpgrade4 < PRICE_UPGRADE_MAX) && (playerScript.currency >= priceUpgrade4);

        

        //Refuel & repair
        menuButtonRepair.interactable = (playerScript.currency >= priceRepair) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax);
        menuButtonRefuel.interactable = (playerScript.currency >= priceRefuel) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax);
    }

    public void SellAllPlatinoid()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[0] * pricePlatinoid;
        playerScript.ore[0] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void SellAllPreciousMetal()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[1] * pricePreciousMetal;
        playerScript.ore[1] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void SellAllWater()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[2] * priceWater;
        playerScript.ore[2] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void Repair()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= priceRepair) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax))
        {
            playerScript.currency -= priceRepair;
            playerScript.vitalsHealth = playerScript.vitalsHealthMax;
        }

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void Refuel()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= priceRefuel) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax))
        {
            playerScript.currency -= priceRefuel;
            playerScript.vitalsFuel = playerScript.vitalsFuelMax;
        }

        UpdatePlayerResourcesAndCommerceMenuUI();
    }
}