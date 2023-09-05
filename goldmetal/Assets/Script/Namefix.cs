using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Namefix : MonoBehaviour
{
    public Transform nickname;



    void Update()
    {
        
        nickname.position = transform.position + new Vector3(0, -1.6f, 0);
    }
}
