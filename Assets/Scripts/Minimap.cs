using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using UnityEngine.UI;
using DG.Tweening;

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

    public float speed = 50.0f;

    private Vector3 Current_Destination;

    private Button MapButton;
    // Start is called before the first frame update
    private Rigidbody rb;

    void Start()
    {
        Current_Destination = Vector3.zero;
        MapButton = map.gameObject.GetComponent<Button>();
        MapButton.onClick.AddListener(() => {
            
        });

        rb = Boat.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Set_MapBoat_Position();
        Set_MapTarget_Position();
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

    private void Set_Target_Position()
    {
        Target.transform.position = new Vector3(map_Target.anchoredPosition.x/map.rect.width*Weight,Target.transform.position.y, map_Target.anchoredPosition.y/map.rect.height*Height);
    }

    bool lerp;
    float time = 0;
    float originSpeed;
    /// <summary>
    /// Turnto == 转向
    /// </summary>
    /// <param name="vector"></param>
    private void Boat_Turnto(Vector3 vector)
    {
        float rotate = Vector3.Angle(Boat.forward, (vector-Boat.position).normalized);
        //Debug.Log("旋转角度==" + rotate);

        if (rotate > 3f)
        {
            if (Destination_Change(vector))
            {
                TurnPower = true;

                //向右转
                if (Vector3.Cross(Boat.forward, (vector - Boat.position).normalized).y > 0)
                {
                    boatProbes._turnPower = Turnto_speed;
                }
                else if (Vector3.Cross(Boat.forward, (vector - Boat.position).normalized).y < 0)//向左转
                {
                    boatProbes._turnPower = -Turnto_speed;
                }
                else
                {
                    Debug.Log("Boat.forward, vector.normalized).y == 0");
                }
                //Debug.Log(Vector3.Cross(Boat.forward, vector.normalized).y);
                //Debug.Log("单位向量==" + Vector3.Cross(Boat.forward, vector.normalized));
                //Debug.Log("rotate==" + rotate);
            }
        }
        else
        {
            if (TurnPower)
            {
                //time = boatProbes._enginePower;
                originSpeed = boatProbes._enginePower;
                Debug.Log(rb.velocity);
                time = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y + rb.velocity.z * rb.velocity.z)*0.2f;

                //time = Vector2.Distance(new Vector2(Boat.position.x, Boat.position.z), new Vector2(Target.transform.position.x, Target.transform.position.z)) /Mathf.Abs(Boat.GetComponent<Rigidbody>().velocity.z);
                Debug.Log(time);
                boatProbes._turnPower = 0;
                boatProbes._enginePower = 0;
                TurnPower = false;
                lerp = true;
            }

        }

        if (lerp && time> 0)
        {
            Vector3 vector3 = new Vector3(vector.x, Boat.position.y, vector.z);
            time = Mathf.Lerp(time, originSpeed, 0.01f);
            Debug.Log("time==" + time);
            Boat.DOMove(Target.transform.position, time).SetSpeedBased().OnComplete(delegate() {
                lerp = false;
                Boat.DOKill(); 
                
            });

            float dis = Vector3.Distance(Target.transform.position, Boat.transform.position);
            
            if (dis<1000)
            {
                Boat.DOKill();
                Boat.DOMove(Target.transform.position, 1000/(2.0f*time)).SetAutoKill(true) ;
            }
        }


        //Debug.Log(Vector3.Cross(Boat.forward, vector.normalized).y);

    }


    private bool Destination_Change(Vector3 vector)
    {
        if(vector!= Current_Destination)
        {
            Current_Destination = vector;
            boatProbes._enginePower = speed;
            lerp = false;
            Boat.DOKill();
            return true;
        }
        else
        {
            return false;
        }
    }
}
