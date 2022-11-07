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
                //Teleport
                //----------------
                //Debug.Log("Teleporting: " + GetParentMoonName() + " -> " + exitNode.GetComponentInChildren<HeighlinerEntry>().GetParentMoonName());

                //Is discoverd
                isDiscovered = true;

                //Set exit node as discovered too
                exitNode.GetComponentInChildren<HeighlinerEntry>().isDiscovered = true;

                //Remember recent teleport
                control.GetPlayerScript().recentTeleport = true;

                //Ignore collision damage briefly to avoid glitch caused by (I think) floating point precision errors
                control.GetPlayerScript().collisionDamageInvulnerabilityTemporary = 1f;

                //Remap velocity to exit node's orientation
                //Vector3 playerVelocityDirection = control.GetPlayerScript().rb.velocity.normalized;
                float playerVelocityMagnitude = control.GetPlayerScript().rb.velocity.magnitude;
                control.GetPlayerScript().rb.velocity = exitNode.transform.forward * playerVelocityMagnitude;

                //Teleport player to exit node
                control.GetPlayerTransform().position = exitNode.transform.position;
            }
        }
    }

    //public string GetParentMoonName()
    //{
    //    //This script is in a child object of the actual heighliner, so we need to work with parents and grandparents
    //    int siblingIndex = transform.parent.GetSiblingIndex();
    //
    //    //The parent moon is always directly "above" the heighliner/station (siblingIndex - 1)
    //    return transform.parent.parent.GetChild(siblingIndex - 1).gameObject.GetComponent<NameCelestial>().title;
    //}
    //
    //public Moon GetParentMoonScript()
    //{
    //    //This script is in a child object of the actual heighliner, so we need to work with parents and grandparents
    //    int siblingIndex = transform.parent.GetSiblingIndex();
    //
    //    //The parent moon is always directly "above" the heighliner/station (siblingIndex - 1)
    //    return transform.parent.parent.GetChild(siblingIndex - 1).gameObject.GetComponent<Moon>();
    //}

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