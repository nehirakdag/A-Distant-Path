﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    //External Objects
    public CharacterController cc;
    public GameObject pivotPoint;
    public GameObject cursor;

    public Animator anim;

    //Parameters
    //  Movement
    private float speed = 2.5f;

    //  Cursor
    private float actualCursorRange;
    private float cursorRange = 5f;

    private float joystickThreshold = 0.3f;

    private GameObject selectedTile;
    private Tile prevSelectedTile;


    public float jumpForce = 20f;
    public float gravity = -30f;
    public float verticalVelocity;

    private bool isShooting = false;
    private float timeSinceLastShot = 0.0f;

    private Vector3 moveDirection = Vector3.zero;
    private float previous_y;


    // 0 is fire, 1 is water
    private int currentAmmo = 0;

    private int numTypes = 3;
    

    private Player playerScript;
    private bool climbing = false;

    // Use this for initialization
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerScript = gameObject.GetComponent<Player>();
        actualCursorRange = 1.8f;
    }



    // Update is called once per frame
    void Update()
    {
        move();
        orient();

        if (cc.isGrounded)
        {
            anim.SetBool("isJumping", false);
        }
        
        if (Input.GetButtonDown("AButton"))
        {
            if (LevelManager.instance.TimeState == TimeStates.Past || LevelManager.instance.TimeState == TimeStates.Offline)
            {
                if (!climbing)
                {
                    climb();
                }
            }
            else if (cc.isGrounded)
            {
                //Debug.Log("JUMP");
                previous_y += jumpForce;
                anim.SetBool("isJumping", true);
            }
        }

        moveDirection.y = previous_y;
        moveDirection.y += this.gravity * Time.deltaTime;
        previous_y = moveDirection.y;

        if (Input.GetButtonDown("YButton"))
        {
            currentAmmo = (currentAmmo + 1) % 2;
        }
        
        cc.Move(this.moveDirection * Time.deltaTime);
        
        actualCursorRange = 1.8f;
        
        /*
        if (Input.GetAxis("RightTrigger") >= 0.9 || mode == TriggerType.directInteract) {

            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray(cursor.transform.position + new Vector3(0, 50, 0), Vector3.down);

            if (Physics.Raycast(ray, out hit)) {
                prevSelectedTile = selectedTile;
                selectedTile = hit.collider.gameObject;
                if (selectedTile != null) {
                    if (prevSelectedTile != selectedTile) {
                        Tile curtile = selectedTile.GetComponent<Tile>();

                        if(curtile != null){
                            selectedTile.GetComponent<Tile>().unHighlight();
                        }

                        if (prevSelectedTile != null) {
                            Tile prevtile = prevSelectedTile.GetComponent<Tile>();

                            if(prevtile != null){
                                prevSelectedTile.GetComponent<Tile>().unHighlight();
                            }
                        }
                    }
                }
            }
        } else {
            if (selectedTile != null) {
                Tile tile = selectedTile.GetComponent<Tile>();

                if(tile != null){
                    selectedTile.GetComponent<Tile>().unHighlight();
                }
            }
        }
        */


        Tile curTile = getTileUnderCursor();

        if (curTile != prevSelectedTile)
        {
            if (prevSelectedTile != null)
            {

                prevSelectedTile.SetHighlight(false);
            }

            if (curTile != null)
            {
                float heightDiff = curTile.gameObject.transform.position.y - this.gameObject.transform.position.y;

                if (heightDiff <= 2f)
                    curTile.SetHighlight(true);
            }
        }

        if (curTile == playerScript.getCurTile() && curTile !=null) {
            curTile.SetHighlight(false);
        }

        prevSelectedTile = curTile;

        if (Input.GetAxisRaw("RightTrigger") >= 0.9 && !isShooting)
        {
            isShooting = true;

            playerScript.interactInFront(curTile, currentAmmo);

        }
        else if (Input.GetAxisRaw("RightTrigger") < 0.9)
        {
            isShooting = false;
        }
    }

    public void move()
    {
        float xDisp = Input.GetAxis("LeftJoystickHorizontal");
        float zDisp = -1 * Input.GetAxis("LeftJoystickVertical");
        
        if (cc.isGrounded)
        {
            previous_y = 0f;
        }
        this.moveDirection = Vector3.zero;

        if (Mathf.Abs(xDisp) >= joystickThreshold || Mathf.Abs(zDisp) >= joystickThreshold)
        {
            anim.SetBool("isMoving", true);

            Vector3 forward = pivotPoint.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            forward = forward.normalized;

            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            this.moveDirection = (xDisp * right + zDisp * forward);
            this.moveDirection *= speed;
            orient(Mathf.Atan2(-1 * this.moveDirection.z, this.moveDirection.x) * Mathf.Rad2Deg);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        //dispDir += new Vector3(0f, this.verticalVelocity, 0f);
        //Debug.Log("vert: " + this.verticalVelocity);
        //Debug.Log(dispDir);
        //cc.Move(dispDir * Time.deltaTime);
    }

    public void orient()
    {
        float camOrientation = pivotPoint.transform.rotation.eulerAngles.y;


        Vector3 orientation = Vector3.zero;
        orientation.x = Input.GetAxis("RightJoystickVertical");
        orientation.z = Input.GetAxis("RightJoystickHorizontal");

        switch (CameraControls.type)
        {
            case 0:
                if (Input.GetAxis("LeftTrigger") >= 0.9)
                {
                    orientation.x = Input.GetAxis("RightJoystickVertical");
                    orientation.z = Input.GetAxis("RightJoystickHorizontal");

                    if (orientation.sqrMagnitude >= 0.01)
                    {
                        orient(Mathf.Atan2(orientation.x, orientation.z) * Mathf.Rad2Deg + camOrientation);
                        cursor.transform.localPosition = new Vector3(0, 0, Mathf.Clamp(actualCursorRange * orientation.magnitude, 2f, actualCursorRange));
                    }
                    else
                    {
                        cursor.transform.localPosition = new Vector3(0, 0, 1.8f);
                    }
                }
                else
                {
                    cursor.transform.localPosition = new Vector3(0, 0, 1.8f);
                }
                break;
            case 1:
                orientation.x = Input.GetAxis("RightJoystickVertical");
                orientation.z = Input.GetAxis("RightJoystickHorizontal");

                if (orientation.sqrMagnitude >= 0.01)
                {
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(orientation.x, orientation.z) * Mathf.Rad2Deg + camOrientation + 90, transform.eulerAngles.z);
                    cursor.transform.localPosition = new Vector3(0, 0, actualCursorRange * orientation.magnitude);
                    //Handles.DrawBezier
                }
                else
                {
                    cursor.transform.localPosition = new Vector3(0, 0, 0);
                }
                break;
        }
    }

    public void orient(float angle)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle + 90, transform.eulerAngles.z);
    }

    public void jump()
    {

    }

    public void setPivotPoint(GameObject pivotPoint)
    {
        this.pivotPoint = pivotPoint;
    }

    public Tile getTileUnderCursor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2.0f);
        Tile best = null;
        float bestVal = 0.5f;
        foreach (Collider hit in hits)
        {
            Tile tile = hit.GetComponent<Tile>();
            if (tile != null && tile.GetTopTile() == tile)
            {
                float heightDiff = tile.transform.position.y - transform.position.y;
                Vector3 planarDiff = Vector3.ProjectOnPlane(tile.transform.position - transform.position, Vector3.up);
                if (-1.65f < heightDiff && heightDiff < 1.65f && planarDiff.magnitude < 1.45f)
                {
                    float val = Vector3.Dot(transform.forward, planarDiff.normalized) + (0.001f * tile.transform.position.y);
                    if (val > bestVal)
                    {
                        best = tile;
                        bestVal = val;
                    }
                }
            }
        }
        return best;
        /*
        Tile playerTile = playerScript.getCurTile();
        foreach (Tile tile in playerTile.neighbors)
        {
            Tile topTile = tile.GetTopTile();
            float heightDiff = topTile.transform.position.y - playerTile.transform.position.y;
            if (-1.1f < heightDiff && heightDiff < 2.1f)
            {
                float val = Vector3.Dot(transform.forward, Vector3.ProjectOnPlane(topTile.transform.position - transform.position, Vector3.up).normalized);
                if (val > bestVal)
                {
                    best = topTile;
                    bestVal = val;
                }
            }
        }
        return best;
        */

        /*
        Tile playerTile = playerScript.getCurTile();
        Tile best = null;
        float bestVal = 0.5f;
        foreach (Tile tile in playerTile.neighbors)
        {
            Tile topTile = tile.GetTopTile();
            float heightDiff = topTile.transform.position.y - playerTile.transform.position.y;
            if (-1.1f < heightDiff && heightDiff < 2.1f)
            {
                float val = Vector3.Dot(transform.forward, Vector3.ProjectOnPlane(topTile.transform.position - transform.position, Vector3.up).normalized);
                if (val > bestVal)
                {
                    best = topTile;
                    bestVal = val;
                }
            }
        }
        return best;
        */
        /*
        RaycastHit hitAbove = new RaycastHit();
        Ray rayAbove = new Ray(gameObject.transform.position, Vector3.up);
        float y = 0;
        if (Physics.Raycast(rayAbove, out hitAbove)) {
            y = 0.1f;
        } else {
            y = 50f;
        }

        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(cursor.transform.position + new Vector3(0, y, 0), Vector3.down);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject tileGO = hit.collider.gameObject;
            if (tileGO != null)
            {
                Tile tile = tileGO.GetComponent<Tile>();
                if (tile != null)
                {
                    return tile;
                }
            }
        }
        //Debug.Log("ERROR GETTING TILE UNDER CURSOR");
        return null;
        */
    }



    public void climb()
    {
        Tile frontTile = null;
        Tile tile = getTileUnderCursor();

        if (tile != null)
        {
            frontTile = tile;
        }

        /*
        RaycastHit hit = new RaycastHit();
        Debug.DrawRay(this.gameObject.transform.position, this.transform.forward, Color.red, 5);
        Ray ray = new Ray(this.gameObject.transform.position + new Vector3(0, 0.2f, 0), this.transform.forward);


        if (Physics.Raycast(ray, out hit)) {
            GameObject tileGO = hit.collider.gameObject;

            if (tileGO != null && hit.distance < 1) {
                    Debug.Log("GLOBAL: " + tileGO.transform.position + " LOCAL: " + tileGO.transform.localPosition);
                    frontTile = tileGO;
            }
        }
        */
        
        if (frontTile != null)
        {

            float heightDiff = frontTile.transform.position.y - transform.position.y;
            /*
            Debug.Log("Front tile y: " + frontTile.transform.position.y);
            Debug.Log("Cur tile y :" + playerScript.getCurTile().gameObject.transform.position.y);
            Debug.Log(heightDiff);
            Debug.Log("FrontTile element: " + frontTile.element);
            if(frontTile.element != null){
                Debug.Log("FrontTile element type: " + frontTile.element.elementType);
            }
            */
            if (frontTile.element != null && (frontTile.element.elementType == ElementType.MetalCube || frontTile.element.elementType == ElementType.MetalCubeRusted) && heightDiff < 0.75f)
            {
                StartCoroutine(climbWithStall(frontTile));

                //Debug.Log("AUDIO");
                playerScript.audio.clip = playerScript.climbTrack;
                playerScript.audio.volume = 0.1f;
                playerScript.audio.Play();
            }
            else if (0.25f < heightDiff && heightDiff < 0.75f)
            {
                StartCoroutine(climbWithStall(frontTile));

                //Debug.Log("AUDIO");
                playerScript.audio.clip = playerScript.climbTrack;
                playerScript.audio.volume = 0.1f;
                playerScript.audio.Play();
            }
            else
            {
                Debug.Log("THE TILE THE PLAYER IS TRYING TO CLIMB IS TOO HIGH");
            }

        }
        else
        {
            Debug.Log("Climbing failed, either not close enough to front tile or no such tile exists");
        }

    }

    IEnumerator climbWithStall(Tile tile)
    {
        climbing = true;
        //PlayerMesh mesh = this.GetComponentInChildren<PlayerMesh>();
        /*
        Debug.Log("AUDIO");

        playerScript.audio.clip = playerScript.climbTrack;
        playerScript.audio.volume = 0.1f;
        playerScript.audio.Play();
        */

        //mesh.enableMesh(false);
        yield return new WaitForSeconds(0.15f);

        Vector3 newPosition;
        if (tile.element != null && tile.element.climable)
        {
            newPosition = tile.element.gameObject.transform.position;
        }
        else
        {
            newPosition = tile.transform.position;
        }
        
        this.transform.position = new Vector3(newPosition.x, newPosition.y + 1f, newPosition.z); // height is hard coded for now
        climbing = false;
        //mesh.enableMesh(true);
    }

    public float getCurrentAmmo() {
        return currentAmmo;
    }
}