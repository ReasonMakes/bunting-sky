using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeighlinerEntry : MonoBehaviour
{
    [System.NonSerialized] public Control control;
    [System.NonSerialized] public GameObject parentPlanet;
    [System.NonSerialized] public GameObject exitNode;
    public GameObject mapLineModelPrefab;
    [System.NonSerialized] public GameObject mapLineModel;
    [System.NonSerialized] public bool isDiscovered = false;

    private void Start()
    {
        //Set parent
        transform.parent.parent = control.generation.moons.transform;

        //Set name
        transform.parent.GetComponent<NameHuman>().title = "Heighliner";
    }

    private void OnTriggerEnter(Collider other)
    {
        //Collision is with player
        if (other.gameObject.name == "Player Collider")
        {
            Player playerScript = control.GetPlayerScript();

            if (playerScript.recentTeleport)
            {
                //Reset recent teleport detection (this will occur when arriving at the new heighliner as we teleport
                //INTO the trigger volume that would otherwise teleport the player)
                control.GetPlayerScript().recentTeleport = false;
            }
            else
            {
                //Teleport
                //----------------
                //Discovered
                isDiscovered = true;
                exitNode.GetComponentInChildren<HeighlinerEntry>().isDiscovered = true;

                //Tutorial
                playerScript.tutorialHasUsedHeighliner = true;

                //Remember recent teleport so we don't trigger the exit node teleporter
                playerScript.recentTeleport = true;

                //Ignore collision damage briefly to avoid glitch caused by (I think) floating point precision errors
                playerScript.collisionDamageInvulnerabilityTemporary = 1f;

                //Teleport player to exit node
                playerScript.rb.position = exitNode.transform.position;
            }
        }
    }

    public bool Setup()
    {
        if (exitNode != null)
        {
            //Get values from exit node position
            float distanceToExitNode = Vector3.Distance(transform.position, exitNode.transform.position);
            Quaternion rotationToLookAtExitNode = Quaternion.LookRotation((exitNode.transform.position - transform.position).normalized);

            //ROTATION
            //Rotate to point toward exit node
            transform.parent.rotation = rotationToLookAtExitNode;

            //MAP UI LINE
            //Instantiate
            mapLineModel = Instantiate(mapLineModelPrefab);
            mapLineModel.transform.parent = transform.parent;
            mapLineModel.transform.rotation = transform.parent.rotation;

            //Scale the distance
            mapLineModel.transform.localScale = new Vector3(
                10f,               //width
                1f,                //height
                distanceToExitNode //length
            );
            mapLineModel.transform.localScale /= 40f;
            
            //Offset position to be in between the two objects
            mapLineModel.transform.position = (transform.position + exitNode.transform.position)/2f;
            
            //Render above everything except the player
            mapLineModel.transform.position = new Vector3(
                mapLineModel.transform.position.x,
                (control.GetPlayerScript().mapCam.GetComponent<Camera>().farClipPlane / 2f) - 300f, //control.GetPlayerScript().mapCam.transform.position.y - 100f,
                mapLineModel.transform.position.z
            );
            
            //Set as inactive so it's invisible until the player looks at their map
            mapLineModel.SetActive(false);
            
            return true;
        }
        else
        {
            Debug.LogError("No exit node set");
            return false;
        }
    }
}