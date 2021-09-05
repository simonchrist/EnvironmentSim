using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Environment : MonoBehaviour
{
    UIManager uiManager;
    Region selectedRegion;

    public GameObject selectedRegionOBJ;

    [HideInInspector]
    public char symbol = 'F';
    public enum metricSys { Imperial, Metric }

    public event OnMetricChangeDelegate OnMetricChange;
    public delegate void OnMetricChangeDelegate(metricSys newVal);

    public metricSys _metric;
    private metricSys metric
    {
        get
        {
            return _metric;
        }
        set
        {
            if (_metric == value) return;
            _metric = value;
            if (OnMetricChange != null)
            {
                OnMetricChange(_metric);
            }
        }
    }

    [Header("Environment Variables")]
    public TempClass[] TemperatureRanges;

    [System.Serializable]
    public class TempClass
    {
        [SerializeField]
        public float minTemp, maxTemp;
        public enum tempType { DeathCold, Danger, Freezing, Cold, Normal, Warm, Hot, DeathHot };
        public tempType climate;
    }
    [Space(3)]

    public int maxPopulation = 100;

    private void Start()
    {
        uiManager = GameObject.Find("Manager").GetComponent<UIManager>();
        OnMetricChange += MetricChangeHandler;
    }

    private void Update()
    {
        selectedRegion = selectedRegionOBJ.GetComponent<Region>();

        uiManager.temperatureText.text = Mathf.Round(selectedRegion.regionTemperature) + "°" + symbol;
        uiManager.AQIText.text = "Air Quality: " + Mathf.Round(selectedRegion.regionAQI);
        uiManager.WQIText.text = "Water Quality: " + Mathf.Round(selectedRegion.regionWQI);
        uiManager.EnergyConsumptionRate.text = "Energy Consumption Rate: " + Mathf.Round(selectedRegion.TotalEnergyRate) + "/tick";
        uiManager.EnergyReserve.text = "Energy Reserve: " + Mathf.Round(selectedRegion.regionEnergy);
        uiManager.Population.text = "Population: " + selectedRegion.population;
    }

    private void MetricChangeHandler(metricSys newVal)
    {
        if (newVal == metricSys.Imperial)
        {
            symbol = 'F';

            foreach (TempClass tempClass in TemperatureRanges)
            {
                tempClass.maxTemp = tempClass.maxTemp * 9 / 5 + 32;
                tempClass.minTemp = tempClass.minTemp * 9 / 5 + 32;
            }

            selectedRegion.regionTemperature = selectedRegion.regionTemperature * 9 / 5 + 32;
        }
        else if (newVal == metricSys.Metric)
        {
            symbol = 'C';

            foreach (TempClass tempClass in TemperatureRanges)
            {
                tempClass.maxTemp = (tempClass.maxTemp - 32) * 5 / 9;
                tempClass.minTemp = (tempClass.minTemp - 32) * 5 / 9;
            }

            selectedRegion.regionTemperature = (selectedRegion.regionTemperature - 32) * 5 / 9;
        }
    }

    public void ChangeMeasurement(Dropdown change)
    {
        if (change.value == 0)
            metric = metricSys.Imperial;
        else
            metric = metricSys.Metric;
    }

}