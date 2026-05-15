using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public static MainCameraController Instance;

    // private bool inForce = false;

    private float maxX = 100f;
    private float minX = -100f;

    private float maxZ = 100f;
    private float minZ = -100f;

    private float maxY = -30f;
    private float minY = -100f;
    private float defaultY = -50f;

    private float maxDistance = 200f; // 最大直线距离
    private float minDistance = 15f; // 最小直线距离
    private float defaultDistance = 45f;

    private float defaultRotateX = 55f;

    private float posY;
    private float distance;
    private float rotateX;

    private bool isFollow = true;
    public Transform followPlayer;

    private void Awake()
    {
        Instance = this;
    }


    private void Start() {
        posY = defaultY;
        rotateX = defaultRotateX;
        distance = defaultDistance;
    }

    private void Update()
    {
        if(isFollow )
        {
            float posX = followPlayer.position.x;
            float posZ = followPlayer.position.z;
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                // this.posY +=0.5F;
                distance -=10F;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
                
            } else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                // this.posY -=0.5F;
                distance +=10F;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
            
            Vector3 vt =  new Vector3(posX, posY, posZ);
            if (vt.x>=maxX)
            {
                vt.x = maxX;
            }else if (vt.x<minX)
            {
                vt.x = minX;
            }else if(vt.y>=maxY)
            {
                vt.y = maxY;
            }else if(vt.y<minY)
            {
                vt.y = minY;    
            }else if(vt.z<=maxZ)
            {
                vt.z = maxZ;
            }else if(vt.z>minZ)
            {
                vt.z = minZ;
            }
           

            transform.position = vt;
            Vector3 offset = Quaternion.Euler(rotateX, 0f, 0f) * new Vector3(0f, 0f, -distance);
            transform.position = followPlayer.position + offset;
            LookAtTarget();

        }
    }


        private void LookAtTarget()
    {
        // 让相机始终朝向物体
        transform.LookAt(followPlayer);
    }

}
