using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crest;
using UnityEngine.UI;
using DG.Tweening;

public class Minimap : MonoBehaviour
{
    public static Minimap Instance;

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

    public Transform Trail_Back, Trail_Front;

    public Transform[] LangHuaGroup;

    public Vector3 Trail_Scale = new Vector3(0.3f, 0.3f, 0.3f);

    private float Transition_Time = 4.0f;

    public Image RouteImage;
    public Transform RouteImageGroup;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Current_Destination = Vector3.zero;
        MapButton = map.gameObject.GetComponent<Button>();
        MapButton.onClick.AddListener(() => {
            
        });
        Set_langhua_start();
        rb = Boat.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Set_MapBoat_Position();
        Set_MapTarget_Position();
        
        Debug.DrawLine(Boat.position, Target.transform.position, Color.red);
        Debug.DrawRay(Boat.position, Boat.forward*100.0f, Color.blue);

    }

    private void FixedUpdate()
    {
        Boat_Turnto(Target.gameObject.transform.position);

       // LookAtTarget(Target.gameObject.transform.position);
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
    float Distance = 10.0f;
    Vector2 Current_Map_Boat;
    /// <summary>
    /// Turnto == 转向
    /// </summary>
    /// <param name="vector"></param>
    private void Boat_Turnto(Vector3 vector)
    {
        //float rotate = Vector3.Angle(Boat.forward, (vector - Boat.position).normalized);
        //Debug.Log("旋转角度==" + rotate);
        Vector3 dir;
        dir = vector - Boat.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        //Debug.Log("rot=== " + rot.eulerAngles.y+" boat eulerAngle.y "+Boat.eulerAngles.y);
        if (Mathf.Abs( rot.eulerAngles.y - Boat.eulerAngles.y) > 1.0f)
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
                originSpeed = boatProbes._enginePower;

                time = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y + rb.velocity.z * rb.velocity.z) * 0.2f;
                boatProbes._turnPower = 0;
                boatProbes._enginePower = 0;
                TurnPower = false;
                lerp = true;     
            }
        }

        if (lerp && time > 0)
        {
            Vector3 vector3 = new Vector3(vector.x, Boat.position.y, vector.z);
            time = Mathf.Lerp(time, originSpeed, 0.01f);
            Set_langhua_SpeedUp();

            if (Vector2.Distance(Current_Map_Boat, map_Boat.anchoredPosition) > Distance)
            {
                Current_Map_Boat = map_Boat.anchoredPosition;
                Creat_RouteImage(map_Boat.anchoredPosition);
            }

            Boat.DOMove(Target.transform.position, time).SetSpeedBased();

            float dis = Vector3.Distance(Target.transform.position, Boat.transform.position);

            if (dis < 1000)
            {
                Boat.DOKill();
                Boat.DOMove(Target.transform.position, 1000 / (2.0f * time)).SetAutoKill(true);
                Set_langhua_SpeedDown();
            }
        }
    }

    private bool Destination_Change(Vector3 vector)
    {
        if (Vector3.Angle(Current_Destination, (vector - Current_Destination).normalized) < 1.5f && Current_Destination != Vector3.zero)
        {
            return false;
        }

        if (vector!= Current_Destination)
        {
            Current_Destination = vector;
            boatProbes._enginePower = speed;
            lerp = false;
            Boat.DOKill();
            Set_langhua_SpeedDown();
            Current_Map_Boat = map_Boat.anchoredPosition;

            Creat_RouteImage(map_Boat.anchoredPosition);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Set_langhua_start()
    {

        Trail_Back.localScale = Trail_Scale;
        Trail_Front.localScale = Trail_Scale;

        foreach (Transform item in LangHuaGroup)
        {
            item.localScale = Trail_Scale;
        }
    }

    private void Set_langhua_SpeedUp()
    {
        Trail_Back.DOKill();
        Trail_Front.DOKill();
        foreach (Transform item in LangHuaGroup)
        {
            item.DOKill();
        }

        Trail_Back.DOScale(new Vector3(1, 1, 1), Transition_Time);
        Trail_Front.DOScale(new Vector3(1, 1, 1), Transition_Time);

        foreach (Transform item in LangHuaGroup)
        {
            item.DOScale(new Vector3(1, 1, 1), Transition_Time);
        }
    }

    private void Set_langhua_SpeedDown()
    {
        Trail_Back.DOKill();
        Trail_Front.DOKill();
        foreach (Transform item in LangHuaGroup)
        {
            item.DOKill();
        }

        Trail_Back.DOScale(Trail_Scale, Transition_Time);
        Trail_Front.DOScale(Trail_Scale, Transition_Time);

        foreach (Transform item in LangHuaGroup)
        {
            item.DOScale(Trail_Scale, Transition_Time); 
        }
    }

    public void Set_langhua_Color(Color color)
    {
        foreach (Transform item in LangHuaGroup)
        {
            item.gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
        }
    }

    private void Creat_RouteImage(Vector2 vector2)
    {
        Image img = Instantiate(RouteImage);
       
        img.transform.SetParent(RouteImageGroup);
        img.GetComponent<RectTransform>().anchoredPosition = vector2;
    }
}
