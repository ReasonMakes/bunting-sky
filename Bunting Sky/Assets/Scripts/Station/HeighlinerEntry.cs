using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeighlinerEntry : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public GameObject exitNode = null;

    private void OnTriggerEnter(Collider other)
    {
        //Collision is with player
        if (other.gameObject.name == "Player Collider")
        {
            //Debug.Log("Entered heighliner " + GetParentMoonName());

            if (control.generation.instancePlayer.transform.Find("Body").GetComponent<Player>().recentTeleport)
            {
                //Trying to teleport immediately after just teleporting
                //----------------

                //Debug.Log("Recent teleport detected; not teleporting again");

                //Reset recent teleport detection (this will occur when arriving at the new heighliner as we teleport INTO the trigger volume that would otherwise teleport the player)
                control.GetPlayerScript().recentTeleport = false;
            }
            else
            {
                if (exitNode == null)
                {
                    Debug.LogError("Exit node for " + GetParentMoonName() + " is null; nowhere to teleport to");
                }
                else
                {
                    //Teleport
                    //----------------
                    //Log
                    //Debug.Log("Teleporting: " + GetParentMoonName() + " -> " + exitNode.GetComponentInChildren<HeighlinerEntry>().GetParentMoonName());

                    //Remember recent teleport
                    control.GetPlayerScript().recentTeleport = true;

                    //Ignore collisions briefly to avoid glitch caused by (I think) floating point precision errors
                    control.GetPlayerScript().collisionImmunity = 1f;

                    //Teleport player to exit node
                    control.GetPlayerTransform().position = exitNode.transform.position + new Vector3(0f, 0f, 0f);
                }
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    //Collision is with player
    //    if (other.gameObject.name == "Body")
    //    {
    //        Debug.Log("Exited heighliner " + GetParentMoonName());
    //    }
    //}

    public string GetParentMoonName()
    {
        //This script is in a child object of the actual heighliner, so we need to work with parents and grandparents
        int siblingIndex = transform.parent.GetSiblingIndex();

        //The parent moon is always directly "above" the heighliner/station (siblingIndex - 1)
        return transform.parent.parent.GetChild(siblingIndex - 1).gameObject.GetComponent<NameCelestial>().title;
    }
}