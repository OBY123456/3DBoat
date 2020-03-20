using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;

public class Minimap : MonoBehaviour
{
    public RectTransform map;
    public RectTransform map_Boat,map_Target;

    public Transform Boat;

    public float Height;
    public float Weight;

    public GameObject Target;

    public BoatProbes boatProbes;

    private bool TurnPower;

    public float Turnto_speed = 1.0f;

    private Vector3 Current_Destination;

    // Start is called before the first frame update
    void Start()
    {
        Current_Destination = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Set_MapBoat_Position();
        Boat_Turnto(Target.gameObject.transform.position);
        Debug.DrawLine(Boat.position, Target.transform.position, Color.red);
    }

    private void Set_MapBoat_Position()
    {
        Vector2 temp = new Vector2(Boat.position.x / Weight * map.rect.width, Boat.position.z / Height * map.rect.height);
        map_Boat.anchoredPosition = temp;    
    }

    private void Set_MapTarget_Position()
    {
        Vector2 temp = new Vector2(Target.transform.position.x / Weight * map.rect.width, Target.transform.position.z / Height * map.rect.height);
        map_Target.anchoredPosition = temp;
    }

    /// <summary>
    /// Turnto == 转向
    /// </summary>
    /// <param name="vector"></param>
    private void Boat_Turnto(Vector3 vector)
    {
        float rotate = Vector3.Angle(Boat.forward, (vector-Boat.position).normalized);
        //Debug.Log("旋转角度==" + rotate);
        if (rotate > 3.0f)
        {
            TurnPower = true;
            if (Destination_Change(vector))
            {
                //向右转
                if (Vector3.Cross(Boat.forward, vector.normalized).y >= 0)
                {
                    boatProbes._turnPower = Turnto_speed;
                }
                else //向左转
                {
                    boatProbes._turnPower = -Turnto_speed;
                }
                Debug.Log(Vector3.Cross(Boat.forward, vector.normalized).y);
                Debug.Log("rotate==" + rotate);
            }
        }
        else
        {
            if (TurnPower)
            {
                boatProbes._turnPower = 0;
                TurnPower = false;
            }
        }

        Debug.Log(Vector3.Cross(Boat.forward, vector.normalized).y);

    }


    private bool Destination_Change(Vector3 vector)
    {
        if(vector!= Current_Destination)
        {
            Current_Destination = vector;
            boatProbes._enginePower = 50.0f;
            Set_MapTarget_Position();
            return true;
        }
        else
        {
            return false;
        }
    }
}
