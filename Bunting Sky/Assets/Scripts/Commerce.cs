using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Commerce : MonoBehaviour
{
    public static bool menuOpen = false;
    public static bool menuLocked = false;

    [SerializeField] private Control control;

    public GameObject menuContainer;

    public Button menuButtonSellAllPlatinoid;
    public Button menuButtonSellAllPreciousMetal;
    public Button menuButtonSellAllWater;

    public Button menuButtonUpgrade1;
    public Button menuButtonUpgrade2;
    public Button menuButtonUpgrade3;
    public Button menuButtonUpgrade4;

    public Button menuButtonRepair;
    public Button menuButtonRefuel;

    private float pricePlatinoid = 30f;
    private float pricePreciousMetal = 60f;
    private float priceWater = 10f;

    private float priceUpgrade1 = 2000f;
    private float priceUpgrade2 = 200f;
    private float priceUpgrade3 = 500f;
    private float priceUpgrade4 = 10000f;

    private float priceRepair = 10f;
    private float priceRefuel = 10f;
    
    private void Update()
    {
        //ESC (default) only closes the menu - it does not open it
        if (!Menu.menuOpenAndGamePaused && menuOpen && control.binds.GetInputDown(control.binds.bindToggleMenu))
        {
            MenuToggle();
        }
    }

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

        menuButtonSellAllPlatinoid.interactable = playerScript.ore[0] > 0.0;
        menuButtonSellAllPreciousMetal.interactable = playerScript.ore[1] > 0.0;
        menuButtonSellAllWater.interactable = playerScript.ore[2] > 0.0;

        menuButtonUpgrade1.interactable = playerScript.currency >= priceUpgrade1;
        menuButtonUpgrade2.interactable = playerScript.currency >= priceUpgrade2;
        menuButtonUpgrade3.interactable = playerScript.currency >= priceUpgrade3;
        menuButtonUpgrade4.interactable = playerScript.currency >= priceUpgrade4;

        menuButtonRepair.interactable = (playerScript.currency >= priceRepair) && (playerScript.vitalsHealth < playerScript.vitalsHealthMax);
        menuButtonRefuel.interactable = (playerScript.currency >= priceRefuel) && (playerScript.vitalsFuel < playerScript.vitalsFuelMax);
    }

    public void SellAllPlatinoid()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[0] * pricePlatinoid;
        control.textCurrency.text = playerScript.currency.ToString("F2");

        playerScript.ore[0] = 0.0;
        control.textWater.text = playerScript.ore[0].ToString("F2");

        UpdateUI();
    }

    public void SellAllPreciousMetal()
    {
        Player playerScript = control.instancePlayer.GetComponentInChildren<Player>();

        playerScript.currency += playerScript.ore[1] * pricePreciousMetal;
        control.textCurrency.text = playerScript.currency.ToString("F2");

        playerScript.ore[1] = 0.0;
        control.textWater.text = playerScript.ore[1].ToString("F2");

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
