using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveModel
{
    Free,
    Limit,
    Auto
} 


public class MoveController : MonoBehaviour
{
    public static MoveController Instance;
    private float maxMoveSpeed;
    private float currMoveSpeed;
    private Vector3 movement;
    private Rigidbody rb;
    public MoveModel moveModel = MoveModel.Free;
    public bool isMove;


    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        maxMoveSpeed = 30;
    }

    private void Update()
    {
        movement.x = 0;
        movement.z = 0;
        
        
        if (moveModel == MoveModel.Free)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.z = Input.GetAxisRaw("Vertical");
        } else if (moveModel == MoveModel.Limit)
        {
            movement.z = Input.GetAxisRaw("Vertical");
        }


        movement.Normalize();
        if (movement.x!=0||movement.z!=0)
        {
            StartCoroutine(doMove());
            isMove = true;
        }else
        {
            StartCoroutine(stopMove());
            isMove = false;
        }


        if (Input.GetKeyDown(KeyCode.X)) 
        {
            switchMoveModel();
        }

        
        
    }

    IEnumerator doMove()
    {
        currMoveSpeed = 0;
        while (currMoveSpeed<=maxMoveSpeed){
            yield return new WaitForSeconds(0.001F);
            currMoveSpeed+=(maxMoveSpeed/40);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                rb.velocity =  movement * currMoveSpeed;
            }else
            {
                rb.velocity = movement *currMoveSpeed/2;
            
            }
        } 
        
    }

    IEnumerator stopMove()
    {
        while (currMoveSpeed>0){
            yield return new WaitForSeconds(0.001F);
            currMoveSpeed-=(maxMoveSpeed/80);
        }
    }

    private void switchMoveModel()
    {
        switch (moveModel)
        {
            case MoveModel.Free:
                moveModel = MoveModel.Limit;
                break;
            case MoveModel.Limit:
                moveModel = MoveModel.Auto;
                break;
            case MoveModel.Auto:
                moveModel = MoveModel.Free;
                break;
        }
    }
}

