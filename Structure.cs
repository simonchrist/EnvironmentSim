using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    Environment env;
    Region region;

    public enum structureType { Home, Store, Factory, Chemical, PowerPlant, Distiller, Purifier };
    public structureType type;

    public float AirPollutionRate;
    public float AirPollutionAmount;
    public float WaterPollutionRate;
    public float WaterPollutionAmount;
    public float EnergyConsumptionRate;
    public float EnergyConsumptionAmount;
    public float EnergyProductionRate;
    public float EnergyProductionAmount;
    public int spaceRequirement;
    public int spawnChance;
    public GameObject myRegion;

    private void Start()
    {
        env = GameObject.Find("Manager").GetComponent<Environment>();
        region = GetComponentInParent<Region>();
        myRegion = region.gameObject;

        EnergyConsumptionInitilize();

        InvokeRepeating("AirPollution", 1, AirPollutionRate);
        InvokeRepeating("WaterPollution", 1, WaterPollutionRate);
        InvokeRepeating("EnergyConsumption", 1, EnergyConsumptionRate);
        InvokeRepeating("EnergyRefill", 1, EnergyProductionRate);
    }

    public void AirPollution()
    {
        if (region.regionAQI <= region.max_value)
            region.regionAQI += AirPollutionAmount;
    }

    // ADD WATER POLLUTION
    public void WaterPollution()
    {
        if (region.regionWQI <= region.max_value)
            region.regionWQI += WaterPollutionAmount;
    }

    // ADD ENERGY CONSUMPTION -> CONNECT TO REGION ENERGY RESERVE VALUE
    public void EnergyConsumption()
    {
        region.regionEnergy -= EnergyConsumptionAmount;
    }

    // ADD ENERGY REFILL -> CONNECT TO REGION ENERGY RESERVE VALUE
    public void EnergyRefill()
    {
        region.regionEnergy += EnergyProductionAmount;
    }

    public void EnergyConsumptionInitilize()
    {
        switch (type)
        {
            case structureType.Home:
                EnergyConsumptionRate = 10;
                EnergyConsumptionAmount = 5;
                break;
            case structureType.Factory:
                EnergyConsumptionRate = 50;
                EnergyConsumptionAmount = 200;
                EnergyProductionAmount = 65;
                EnergyProductionRate = 25;
                break;
            case structureType.Chemical:
                EnergyConsumptionAmount = 110;
                EnergyConsumptionRate = 55;
                break;
            case structureType.PowerPlant:
                //EnergyConsumptionAmount = 60;
                //EnergyConsumptionRate = 35;
                EnergyProductionAmount = 425;
                EnergyProductionRate = 20;
                break;
            case structureType.Store:
                EnergyConsumptionAmount = 30;
                EnergyConsumptionRate = 15;
                break;
            case structureType.Distiller:
                EnergyConsumptionAmount = 50;
                EnergyConsumptionRate = 15;
                break;
            case structureType.Purifier:
                EnergyConsumptionAmount = 30;
                EnergyConsumptionRate = 20;
                break;
        }
    }

}