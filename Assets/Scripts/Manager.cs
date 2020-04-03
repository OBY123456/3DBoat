using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
public class Manager : MonoBehaviour
{
    // Start is called before the first frame update
    public  BarChart barChart;
    SerieData serieData;

    public GaugeChart gaugeChart;
    public float speed = 60.0f;
    void Start()
    {
        barChart.UpdateData(0,2,56);
        barChart.AddData(0,14);
        Debug.Log(barChart.series.Count);

        for(int i=0;i<=barChart.series.list[0].data.Count-1;i++)
        {
            Debug.Log(barChart.series.GetData(0,i));
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        gaugeChart.UpdateData(0, 0, speed);
    }
}
