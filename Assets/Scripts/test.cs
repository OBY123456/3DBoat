using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class test : MonoBehaviour
{
    public Transform Boat, Target;
    private Rigidbody _rb;
    public float _enginePower, _turnPower;

    public Vector3 Current_Destination { get; private set; }
    public float Turnto_speed;
    public bool TurnPower;

    public Transform T1, T2;
    public float time = 4.135f;

    // Start is called before the first frame update
    void Start()
    {
        _rb = T2.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //FixedUpdateEngine();
        //Boat_Turnto(Target.position);
        //Debug.DrawLine(Boat.transform.position, Target.transform.position, Color.red);
        T1.DOMove(Target.position, time).SetSpeedBased();
        var forcePosition = _rb.position;
        _rb.AddForceAtPosition(Boat.forward * _enginePower, forcePosition, ForceMode.Acceleration);
    }

    private void Boat_Turnto(Vector3 vector)
    {
        float rotate = Vector3.Angle(Boat.forward, (vector - Boat.position).normalized);
        //Debug.Log("旋转角度==" + rotate);

        if (rotate > 3f)
        {
            if(Destination_Change(vector))
            {
                TurnPower = true;

                //向右转
                if (Vector3.Cross(Boat.forward, vector.normalized).y > 0)
                {
                    _turnPower = Turnto_speed;
                }
                else if (Vector3.Cross(Boat.forward, vector.normalized).y < 0)//向左转
                {
                    _turnPower = -Turnto_speed;
                }
                else
                {
                    Debug.Log("Boat.forward, vector.normalized).y == 0");
                }
            }

            Debug.Log(Vector3.Cross(Boat.forward, vector.normalized).y);
            Debug.Log("rotate==" + rotate);
        }
        else
        {
            if (TurnPower)
            {
                _turnPower = 0;
                TurnPower = false;
            }

        }

    }

    private bool Destination_Change(Vector3 vector)
    {
        if (vector != Current_Destination)
        {
            Current_Destination = vector;
            _enginePower = 50.0f;
            return true;
        }
        else
        {
            return false;
        }
    }

    void FixedUpdateEngine()
    {
        var forcePosition = _rb.position;
        _rb.AddForceAtPosition(Boat.forward * _enginePower , forcePosition, ForceMode.Acceleration);
        Vector3 rotVec = transform.up + 0.35f * transform.forward;
        _rb.AddTorque(rotVec * _turnPower , ForceMode.Acceleration);
    }
}
