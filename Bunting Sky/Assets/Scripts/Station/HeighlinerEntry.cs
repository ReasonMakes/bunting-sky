using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeighlinerEntry : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public GameObject exitNode = null;
    public GameObject mapLineModelPrefab;
    [System.NonSerialized] public GameObject mapLineModel;
    [System.NonSerialized] public bool isDiscovered = false;

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
                    //Debug.Log("Teleporting: " + GetParentMoonName() + " -> " + exitNode.GetComponentInChildren<HeighlinerEntry>().GetParentMoonName());

                    //Is discoverd
                    isDiscovered = true;

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

    public string GetParentMoonName()
    {
        //This script is in a child object of the actual heighliner, so we need to work with parents and grandparents
        int siblingIndex = transform.parent.GetSiblingIndex();

        //The parent moon is always directly "above" the heighliner/station (siblingIndex - 1)
        return transform.parent.parent.GetChild(siblingIndex - 1).gameObject.GetComponent<NameCelestial>().title;
    }

    public void SpawnMapLineModel()
    {
        if (exitNode != null)
        {
            //Get values from exit node position
            float distanceToExitNode = Vector3.Distance(transform.position, exitNode.transform.position);
            Quaternion rotationToLookAtExitNode = Quaternion.LookRotation((exitNode.transform.position - transform.position).normalized);

            //Instantiate
            mapLineModel = Instantiate(mapLineModelPrefab, transform.position, rotationToLookAtExitNode);
            mapLineModel.transform.parent = transform;

            //Scale the distance
            mapLineModel.transform.localScale = new Vector3(
                10f,               //width
                1f,                //height
                distanceToExitNode //length
            );
            mapLineModel.transform.localScale /= 40f;

            //Offset position to be in between the two objects
            mapLineModel.transform.position = (transform.position + exitNode.transform.position)/2f;

            mapLineModel.transform.position = new Vector3(
                mapLineModel.transform.position.x,
                (control.GetPlayerScript().mapCam.GetComponent<Camera>().farClipPlane / 2f) - 300f, //control.GetPlayerScript().mapCam.transform.position.y - 100f,
                mapLineModel.transform.position.z
            );

            //Set as inactive so it's invisible until the player looks at their map
            mapLineModel.SetActive(false);
        }
        else
        {
            //Debug.Log("Unable to instantiate map line model as this heighliner has no exit node set!");
        }
    }
}