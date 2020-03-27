using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;

public class CameraFllow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public BoatProbes boatProbes;
    private Vector3 offset2;

    public float Sensity = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        offset2 = player.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, player.position - offset2, Time.deltaTime * 5);
        //Quaternion rotation = Quaternion.LookRotation(player.position);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 3f);
        //transform.position = Vector3.Lerp(transform.position, player.position + player.TransformDirection(offset), Time.deltaTime * 5);
        //transform.LookAt(player.position);
        //Vector3 vector3 = new Vector3(19.34f, transform.localEulerAngles.y, 0);
        //transform.localEulerAngles = vector3;

    }

    private void LateUpdate()
    {
        Vector3 vector3 = player.position + player.TransformDirection(offset);
        //Debug.Log(Mathf.Abs(vector3.y - transform.position.y));
        transform.position = Vector3.Lerp(transform.position, new Vector3(vector3.x, transform.position.y, vector3.z), Time.deltaTime * Sensity);
       // transform.position = vector3;
        transform.LookAt(player.position);
        //if (boatProbes._turnPower != 0)
        //{
        //    //transform.position = Vector3.Lerp(transform.position, new Vector3(vector3.x, transform.position.y, vector3.z), Time.deltaTime * 5);
        //    transform.LookAt(player.position);
        //}
        //else
        //{
        //    transform.position = Vector3.Lerp(transform.position, new Vector3(vector3.x, transform.position.y, transform.position.z), Time.deltaTime * 5);
        //}
    }
}

