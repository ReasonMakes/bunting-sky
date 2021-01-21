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
    public float priceUpgrade1 = 2000f;
    public float priceUpgrade2 = 200f;
    public float priceUpgrade3 = 500f;
    public float priceUpgrade4 = 10000f;

    public Button menuButtonRepair;
    public Button menuButtonRefuel;
    private float priceRepair = 10f;
    private float priceRefuel = 10f;

    private void Start()
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
        upgradeDictionary = new string[7, 3];
        //Name                                              Price                             Description
        upgradeDictionary[0, 0] = "Fuel efficiency";        upgradeDictionary[0, 1] = "1000"; upgradeDictionary[0, 2] = "Fuel is consumed at half the standard rate for the same resultant thrust";
        upgradeDictionary[1, 0] = "Titan fuel tank";        upgradeDictionary[1, 1] = "500";  upgradeDictionary[1, 2] = "Double sized fuel tank with a superior design resulting in the same net weight";
        upgradeDictionary[2, 0] = "Reinforced hull";        upgradeDictionary[2, 1] = "500";  upgradeDictionary[2, 2] = "Double strength outer hull";
        upgradeDictionary[3, 0] = "Raptor engines";         upgradeDictionary[3, 1] = "1000"; upgradeDictionary[3, 2] = "1.5x greater maximum forward thrust output";
        upgradeDictionary[4, 0] = "Dual batteries";         upgradeDictionary[4, 1] = "200";  upgradeDictionary[4, 2] = "Laser weapon has double maximum ammunition per cycle";
        upgradeDictionary[5, 0] = "In situ fuel refinery";  upgradeDictionary[5, 1] = "5000"; upgradeDictionary[5, 2] = "Automatically process water into usable jet fuel anywhere";
        upgradeDictionary[6, 0] = "Warp drive";             upgradeDictionary[6, 1] = "3000"; upgradeDictionary[6, 2] = "Enables extra-dimensional interstellar travel through the bulk";
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
        UpdateUI();

        //Toggle cursor lock
        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;

        //Toggle reticle
        control.reticle.SetActive(!menuOpen);
    }

    private void UpdateUI()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        //Sell
        menuButtonSellAllPlatinoid.interactable = playerScript.ore[0] > 0.0;
        menuButtonSellAllPreciousMetal.interactable = playerScript.ore[1] > 0.0;
        menuButtonSellAllWater.interactable = playerScript.ore[2] > 0.0;

        menuButtonSellAllPlatinoid.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + pricePlatinoid.ToString() + " / g";
        menuButtonSellAllPreciousMetal.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + pricePreciousMetal.ToString() + " / g";
        menuButtonSellAllWater.transform.parent.Find("Price").GetComponent<TextMeshProUGUI>().text = "$" + priceWater.ToString() + " / g";

        //Upgrade
        menuButtonUpgrade1.interactable = playerScript.currency >= priceUpgrade1;
        menuButtonUpgrade2.interactable = playerScript.currency >= priceUpgrade2;
        menuButtonUpgrade3.interactable = playerScript.currency >= priceUpgrade3;
        menuButtonUpgrade4.interactable = playerScript.currency >= priceUpgrade4;

        //Refuel & repair
        menuButtonRepair.interactable = (playerScript.currency >= priceRepair) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax);
        menuButtonRefuel.interactable = (playerScript.currency >= priceRefuel) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax);
    }

    public void SellAllPlatinoid()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[0] * pricePlatinoid;
        control.textCurrency.text = playerScript.currency.ToString("F2");

        playerScript.ore[0] = 0.0;
        control.textPlatinoid.text = playerScript.ore[0].ToString("F2");

        UpdateUI();
    }

    public void SellAllPreciousMetal()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[1] * pricePreciousMetal;
        control.textCurrency.text = playerScript.currency.ToString("F2");

        playerScript.ore[1] = 0.0;
        control.textPreciousMetal.text = playerScript.ore[1].ToString("F2");

        UpdateUI();
    }

    public void SellAllWater()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[2] * priceWater;
        control.textCurrency.text = playerScript.currency.ToString("F2");

        playerScript.ore[2] = 0.0;
        control.textWater.text = playerScript.ore[2].ToString("F2");

        UpdateUI();
    }

    public void Repair()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= priceRepair) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax))
        {
            playerScript.currency -= priceRepair;
            control.textCurrency.text = playerScript.currency.ToString("F2");

            playerScript.vitalsHealth = playerScript.vitalsHealthMax;
        }

        UpdateUI();
    }

    public void Refuel()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        if ((playerScript.currency >= priceRefuel) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax))
        {
            playerScript.currency -= priceRefuel;
            control.textCurrency.text = playerScript.currency.ToString("F2");

            playerScript.vitalsFuel = playerScript.vitalsFuelMax;
        }

        UpdateUI();
    }
}
