using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Commerce : MonoBehaviour
{
    [System.NonSerialized] public static bool menuOpen = false;
    [System.NonSerialized] public static bool menuLocked = false;

    [SerializeField] private Control control;

    public TextMeshProUGUI stationName;

    public GameObject tooltip;

    public GameObject menuContainer;

    public Button menuButtonSellAllPlatinoid;
    public Button menuButtonSellAllPreciousMetal;
    public Button menuButtonSellAllWater;
    [System.NonSerialized] public float pricePlatinoid = 30f;
    [System.NonSerialized] public float pricePreciousMetal = 60f;
    [System.NonSerialized] public float priceWater = 10f;

    [System.NonSerialized] public string[,] upgradeDictionary;

    [System.NonSerialized] public readonly int UPGRADE_NAME = 0;
    [System.NonSerialized] public readonly int UPGRADE_PRICE = 1;
    [System.NonSerialized] public readonly int UPGRADE_DESCRIPTION = 2;
    [System.NonSerialized] public readonly int UPGRADE_MAX_LEVEL = 3;

    [System.NonSerialized] public readonly int UPGRADE_SOLD_OUT = 0;
    [System.NonSerialized] public readonly int UPGRADE_FUEL_EFFICIENCY = 1;
    [System.NonSerialized] public readonly int UPGRADE_TITAN_FUEL_TANK = 2;
    [System.NonSerialized] public readonly int UPGRADE_REINFORCED_HULL = 3;
    [System.NonSerialized] public readonly int UPGRADE_RAPTOR_ENGINES = 4;
    [System.NonSerialized] public readonly int UPGRADE_DUAL_BATTERIES = 5;
    [System.NonSerialized] public readonly int UPGRADE_IN_SITU_FUEL_REFINERY = 6;
    [System.NonSerialized] public readonly int UPGRADE_SEISMIC_CHARGES = 7;

    [System.NonSerialized] public int[] upgradeIndexAtButton;
    public Button menuButtonUpgrade0;
    public Button menuButtonUpgrade1;
    public Button menuButtonUpgrade2;
    public Button menuButtonUpgrade3;
    [System.NonSerialized] public readonly float UPGRADE_PRICE_MAX = 1e6f; //Setting to this price will be interpretted as an unavailable item
    [System.NonSerialized] public float priceUpgrade0 = 42069f;
    [System.NonSerialized] public float priceUpgrade1 = 42069f;
    [System.NonSerialized] public float priceUpgrade2 = 42069f;
    [System.NonSerialized] public float priceUpgrade3 = 42069f;

    public Button menuButtonRepair;
    public Button menuButtonRefuel;
    private readonly float PRICE_REPAIR = 10f;
    private readonly float PRICE_REFUEL = 10f;

    private void Awake()
    {
        DefineUpgrades();
    }

    private void Update()
    {
        if (!Menu.menuOpenAndGamePaused && menuOpen)
        {
            if (control.binds.GetInputDown(control.binds.bindToggleMenu))
            {
                //ESC (default) only closes the menu - it does not open it
                MenuToggle();
            }

            UpdateTooltipPosition();
        }
    }

    //GENERAL
    private void DefineUpgrades()
    {
        //Initializations
        upgradeDictionary = new string[8, 4];
        upgradeIndexAtButton = new int[upgradeDictionary.GetLength(0)];

        //Definitions
        //Name
        upgradeDictionary[UPGRADE_SOLD_OUT,                 UPGRADE_NAME] = "Sold out";
        upgradeDictionary[UPGRADE_FUEL_EFFICIENCY,          UPGRADE_NAME] = "Fuel efficiency";
        upgradeDictionary[UPGRADE_TITAN_FUEL_TANK,          UPGRADE_NAME] = "Titan fuel tank";
        upgradeDictionary[UPGRADE_REINFORCED_HULL,          UPGRADE_NAME] = "Reinforced hull";
        upgradeDictionary[UPGRADE_RAPTOR_ENGINES,           UPGRADE_NAME] = "Raptor engines";
        upgradeDictionary[UPGRADE_DUAL_BATTERIES,           UPGRADE_NAME] = "Dual batteries";
        upgradeDictionary[UPGRADE_IN_SITU_FUEL_REFINERY,    UPGRADE_NAME] = "In-situ fuel refinery";
        upgradeDictionary[UPGRADE_SEISMIC_CHARGES,          UPGRADE_NAME] = "Seismic charges";
        //upgradeDictionary[7, UPGRADE_NAME] = "Warp drive";

        //Price
        upgradeDictionary[UPGRADE_SOLD_OUT,                 UPGRADE_PRICE] = UPGRADE_PRICE_MAX.ToString();
        upgradeDictionary[UPGRADE_FUEL_EFFICIENCY,          UPGRADE_PRICE] = "500";
        upgradeDictionary[UPGRADE_TITAN_FUEL_TANK,          UPGRADE_PRICE] = "2000";
        upgradeDictionary[UPGRADE_REINFORCED_HULL,          UPGRADE_PRICE] = "750";
        upgradeDictionary[UPGRADE_RAPTOR_ENGINES,           UPGRADE_PRICE] = "1000";
        upgradeDictionary[UPGRADE_DUAL_BATTERIES,           UPGRADE_PRICE] = "2000";
        upgradeDictionary[UPGRADE_IN_SITU_FUEL_REFINERY,    UPGRADE_PRICE] = "4000";;
        upgradeDictionary[UPGRADE_SEISMIC_CHARGES,          UPGRADE_PRICE] = "6000";

        //Description
        upgradeDictionary[UPGRADE_SOLD_OUT,                 UPGRADE_DESCRIPTION] = "Item is out of stock";
        upgradeDictionary[UPGRADE_FUEL_EFFICIENCY,          UPGRADE_DESCRIPTION] = "25% less fuel consumption for the same resultant thrust";
        upgradeDictionary[UPGRADE_TITAN_FUEL_TANK,          UPGRADE_DESCRIPTION] = "1.5x larger fuel tank made of lighter material, resulting in the same weight";
        upgradeDictionary[UPGRADE_REINFORCED_HULL,          UPGRADE_DESCRIPTION] = "Doubles outer hull integrity";
        upgradeDictionary[UPGRADE_RAPTOR_ENGINES,           UPGRADE_DESCRIPTION] = "Increases maximum forward thrust output by 1.5x";
        upgradeDictionary[UPGRADE_DUAL_BATTERIES,           UPGRADE_DESCRIPTION] = "Doubles the mining laser's maximum ammunition per cycle";
        upgradeDictionary[UPGRADE_IN_SITU_FUEL_REFINERY,    UPGRADE_DESCRIPTION] = "Automatically processes water ice cargo into usable jet fuel (toggle in settings)";
        upgradeDictionary[UPGRADE_SEISMIC_CHARGES,          UPGRADE_DESCRIPTION] = "Explosive weapon. Useful for mining clusters of small asteroids";
        //upgradeDictionary[7, UPGRADE_DESCRIPTION] = "Enables extra-dimensional interstellar travel through the bulk";

        //Max level
        upgradeDictionary[UPGRADE_SOLD_OUT,                 UPGRADE_MAX_LEVEL] = "0";
        upgradeDictionary[UPGRADE_FUEL_EFFICIENCY,          UPGRADE_MAX_LEVEL] = "2";
        upgradeDictionary[UPGRADE_TITAN_FUEL_TANK,          UPGRADE_MAX_LEVEL] = "1";
        upgradeDictionary[UPGRADE_REINFORCED_HULL,          UPGRADE_MAX_LEVEL] = "1";
        upgradeDictionary[UPGRADE_RAPTOR_ENGINES,           UPGRADE_MAX_LEVEL] = "1";
        upgradeDictionary[UPGRADE_DUAL_BATTERIES,           UPGRADE_MAX_LEVEL] = "1";
        upgradeDictionary[UPGRADE_IN_SITU_FUEL_REFINERY,    UPGRADE_MAX_LEVEL] = "1";
        upgradeDictionary[UPGRADE_SEISMIC_CHARGES,          UPGRADE_MAX_LEVEL] = "1";
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

    public void UpdateCommerceMenuUI()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //SELL
        TextMeshProUGUI sellAllPlatinoidText        = menuButtonSellAllPlatinoid.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sellAllPlatinoidPrice       = menuButtonSellAllPlatinoid.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sellAllPreciousMetalText    = menuButtonSellAllPreciousMetal.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sellAllPreciousMetalPrice   = menuButtonSellAllPreciousMetal.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sellAllWaterText            = menuButtonSellAllWater.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI sellAllWaterPrice           = menuButtonSellAllWater.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>();

        //Interactable
        menuButtonSellAllPlatinoid.interactable     = playerScript.ore[playerScript.ORE_PLATINOID] > 0.0;
        menuButtonSellAllPreciousMetal.interactable = playerScript.ore[playerScript.ORE_PRECIOUS_METAL] > 0.0;
        menuButtonSellAllWater.interactable         = playerScript.ore[playerScript.ORE_WATER] > 0.0;

        //Display
        //Text
        sellAllPlatinoidText.text       = playerScript.ore[playerScript.ORE_PLATINOID] > 0.0 ? "Sell all" : "Sell all\n(none in cargo)";
        sellAllPreciousMetalText.text   = playerScript.ore[playerScript.ORE_PRECIOUS_METAL] > 0.0 ? "Sell all" : "Sell all\n(none in cargo)";
        sellAllWaterText.text           = playerScript.ore[playerScript.ORE_WATER] > 0.0 ? "Sell all" : "Sell all\n(none in cargo)";

        //Price
        sellAllPlatinoidPrice.text      = "$" + pricePlatinoid + " / kg";
        sellAllPreciousMetalPrice.text  = "$" + pricePreciousMetal + " / kg";
        sellAllWaterPrice.text          = "$" + priceWater + " / kg";

        //Colour
        sellAllPlatinoidText.color      = playerScript.ore[playerScript.ORE_PLATINOID] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;
        sellAllPlatinoidPrice.color     = playerScript.ore[playerScript.ORE_PLATINOID] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;

        sellAllPreciousMetalText.color  = playerScript.ore[playerScript.ORE_PRECIOUS_METAL] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;
        sellAllPreciousMetalPrice.color = playerScript.ore[playerScript.ORE_PRECIOUS_METAL] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;

        sellAllWaterText.color          = playerScript.ore[playerScript.ORE_WATER] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;
        sellAllWaterPrice.color         = playerScript.ore[playerScript.ORE_WATER] > 0.0 ? Control.colorTextEnabled : Control.colorTextDisabled;

        //UPGRADE
        //Interactable
        menuButtonUpgrade0.interactable = GetIfButtonUpgradeIsInteractable(0);
        menuButtonUpgrade1.interactable = GetIfButtonUpgradeIsInteractable(1);
        menuButtonUpgrade2.interactable = GetIfButtonUpgradeIsInteractable(2);
        menuButtonUpgrade3.interactable = GetIfButtonUpgradeIsInteractable(3);

        //Display
        for (int i = 0; i < 4; i++)
        {
            //Set to defaults
            SetButtonUpgradeToDefaults(i);

            //Apply overrides
            if (!GetIfButtonUpgradeIsAffordable(i))
            {
                SetButtonUpgradeToTooExpensive(i);
            }

            if (!GetIfButtonUpgradeIsNotMaxLevel(i))
            {
                SetButtonUpgradeToMaxed(i);
            }

            if (!GetIfButtonUpgradeIsInStock(i))
            {
                SetButtonUpgradeToSoldOut(i);
            }
        }

        //REFUEL & REPAIR
        //Interactable
        menuButtonRepair.interactable = playerScript.currency >= PRICE_REPAIR && playerScript.vitalsHealth < playerScript.vitalsHealthMax;
        menuButtonRefuel.interactable = playerScript.currency >= PRICE_REFUEL && playerScript.vitalsFuel < playerScript.vitalsFuelMax;

        //Display
        //Colour and text
        //Hull repair
        if (playerScript.vitalsHealth >= playerScript.vitalsHealthMax)
        {
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsComplete;
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Repair all damages\n(no damages to repair)";
        }
        else if (playerScript.currency < PRICE_REPAIR)
        {
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsQuest;
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Repair all damages\n(insufficient funds)";
        }
        else
        {
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextEnabled;
            menuButtonRepair.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Repair all damages";
        }
        //Refuel
        if (playerScript.vitalsFuel >= playerScript.vitalsFuelMax)
        {
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsComplete;
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Refuel to full\n(tank at maximum capacity)";
        }
        else if (playerScript.currency < PRICE_REFUEL)
        {
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsQuest;
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Refuel to full\n(insufficient funds)";
        }
        else
        {
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextEnabled;
            menuButtonRefuel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Refuel to full";
        }
    }

    public void EnableTooltip()
    {
        tooltip.SetActive(true);
    }

    public void DisableTooltip()
    {
        tooltip.SetActive(false);
    }

    private void UpdateTooltipPosition()
    {
        tooltip.GetComponent<RectTransform>().position = new Vector3(Input.mousePosition.x + 24f, Input.mousePosition.y, 0f);
    }

    public void SetTooltipToUpgradeButton(int buttonIndex)
    {
        //Set text
        tooltip.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_DESCRIPTION];
        
        //Set background to approximate width of text
        float aproxFontWidth = 7.28f;
        tooltip.GetComponent<RectTransform>().sizeDelta = new Vector2(
            aproxFontWidth * upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_DESCRIPTION].Length,
            tooltip.GetComponent<RectTransform>().sizeDelta.y
        );

        //Correct next frame-ish
        Invoke("UpdateTooltipBackgroundWidth", Time.deltaTime);
    }

    private void UpdateTooltipBackgroundWidth()
    {
        tooltip.GetComponent<RectTransform>().sizeDelta = new Vector2(tooltip.transform.Find("Text").GetComponent<TextMeshProUGUI>().textBounds.size.x + 10f, 23f);
    }

    #region Upgrade buttons
    private Button GetUpgradeButtonFromIndex(int buttonIndex)
    {
        Button menuButton;

             if (buttonIndex == 0) menuButton = menuButtonUpgrade0;
        else if (buttonIndex == 1) menuButton = menuButtonUpgrade1;
        else if (buttonIndex == 2) menuButton = menuButtonUpgrade2;
        else if (buttonIndex == 3) menuButton = menuButtonUpgrade3;
        else { return null; }

        return menuButton;
    }

    public void SetButtonUpgradeToDefaults(int buttonIndex)
    {
        Button menuButton = GetUpgradeButtonFromIndex(buttonIndex);

        //GENERATE LEVEL TEXT
        //Init and references
        string level = "";
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //Get upgrade information
        int upgradeIndex = upgradeIndexAtButton[buttonIndex];
        int upgradeMaxLevel = int.Parse(upgradeDictionary[upgradeIndex, UPGRADE_MAX_LEVEL]);

        //Concatenate upgrade level for sale
        if (playerScript.upgradeLevels[upgradeIndex] < upgradeMaxLevel && playerScript.upgradeLevels[upgradeIndex] > 0)
        {
            level = "\n(level " + (playerScript.upgradeLevels[upgradeIndex] + 1).ToString() + ")";
        }

        //ASSIGN
        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_NAME] + level;
        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextEnabled;
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_PRICE] + " ea";
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().color = Control.colorTextEnabled;
    }

    public void SetButtonUpgradeToSoldOut(int buttonIndex)
    {
        Button menuButton = GetUpgradeButtonFromIndex(buttonIndex);

        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeDictionary[UPGRADE_SOLD_OUT, UPGRADE_NAME];
        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabled;
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "N/A";
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabled;
    }

    public void SetButtonUpgradeToTooExpensive(int buttonIndex)
    {
        Button menuButton = GetUpgradeButtonFromIndex(buttonIndex);

        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsQuest;
        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_NAME] + "\n(insufficient funds)";
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabled;
    }

    public void SetButtonUpgradeToMaxed(int buttonIndex)
    {
        Button menuButton = GetUpgradeButtonFromIndex(buttonIndex);

        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = upgradeDictionary[upgradeIndexAtButton[buttonIndex], UPGRADE_NAME] + "\n(already owned at max level)";
        menuButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabledAsComplete;
        menuButton.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().color = Control.colorTextDisabled;
    }

    private bool GetIfButtonUpgradeIsInStock(int buttonIndex)
    {
        //Get upgrade information
        int upgradeIndex = upgradeIndexAtButton[buttonIndex];
        int upgradePrice = int.Parse(upgradeDictionary[upgradeIndex, UPGRADE_PRICE]);

        //Check if can
        bool isInStock = upgradePrice < UPGRADE_PRICE_MAX;

        return isInStock;
    }

    private bool GetIfButtonUpgradeIsAffordable(int buttonIndex)
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //Get upgrade information
        int upgradeIndex = upgradeIndexAtButton[buttonIndex];
        int upgradePrice = int.Parse(upgradeDictionary[upgradeIndex, UPGRADE_PRICE]);

        //Check if can
        bool canAffordIt = playerScript.currency >= upgradePrice;

        return canAffordIt;
    }

    private bool GetIfButtonUpgradeIsNotMaxLevel(int buttonIndex)
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        //Get upgrade information
        int upgradeIndex = upgradeIndexAtButton[buttonIndex];
        int upgradeMaxLevel = int.Parse(upgradeDictionary[upgradeIndex, UPGRADE_MAX_LEVEL]);

        //Check if can
        bool isNotMaxLevel = playerScript.upgradeLevels[upgradeIndex] < upgradeMaxLevel;

        return isNotMaxLevel;
    }

    private bool GetIfButtonUpgradeIsInteractable(int buttonIndex)
    {
        //Check if can
        bool isInStock = GetIfButtonUpgradeIsInStock(buttonIndex);
        bool canAffordIt = GetIfButtonUpgradeIsAffordable(buttonIndex);
        bool isNotMaxLevel = GetIfButtonUpgradeIsNotMaxLevel(buttonIndex);

        return isInStock && canAffordIt && isNotMaxLevel;
    }
    #endregion

    public void SellAllPlatinoid()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[playerScript.ORE_PLATINOID] * pricePlatinoid;
        playerScript.ore[playerScript.ORE_PLATINOID] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void SellAllPreciousMetal()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[playerScript.ORE_PRECIOUS_METAL] * pricePreciousMetal;
        playerScript.ore[playerScript.ORE_PRECIOUS_METAL] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void SellAllWater()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[playerScript.ORE_WATER] * priceWater;
        playerScript.ore[playerScript.ORE_WATER] = 0.0;

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void PurchaseUpgradeFromButton(int buttonIndex)
    {
        if (GetIfButtonUpgradeIsInteractable(buttonIndex))
        {
            Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

            //Get upgrade information
            int upgradeIndex = upgradeIndexAtButton[buttonIndex];

            //Increment level for upgrade purchased
            playerScript.upgradeLevels[upgradeIndex]++;

            //Decrement currency by price of upgrade
            playerScript.currency -= int.Parse(upgradeDictionary[upgradeIndex, UPGRADE_PRICE]);

            //Update UI
            playerScript.UpdateUpgrades();
            UpdatePlayerResourcesAndCommerceMenuUI();
        }
    }

    public void Repair()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= PRICE_REPAIR) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax))
        {
            playerScript.currency -= PRICE_REPAIR;
            playerScript.vitalsHealth = playerScript.vitalsHealthMax;
        }

        UpdatePlayerResourcesAndCommerceMenuUI();
    }

    public void Refuel()
    {
        Player playerScript = control.generation.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= PRICE_REFUEL) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax))
        {
            playerScript.currency -= PRICE_REFUEL;
            playerScript.vitalsFuel = playerScript.vitalsFuelMax;
        }

        UpdatePlayerResourcesAndCommerceMenuUI();
    }
}