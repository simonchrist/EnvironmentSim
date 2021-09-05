using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Region : MonoBehaviour
{
    public int population;
    public int maxCapacity;
    public int remainingCapacity;

    public float regionTemperature;

    [Header("Air Quality Variables")]
    public float regionAQI;
    [HideInInspector]
    public float max_value = 500;

    [Header("Water Quality Variables")]
    public float regionWQI;

    [Header("Energy Variables")]
    public float regionEnergy;
    public float TotalEnergyAmount;
    public float TotalEnergyRate;

    // Script references
    WorldGen worldGen;
    Environment env;
    UIManager uiManager;

    public GameObject[] Structures;
    [HideInInspector]
    public GameObject[] Blobs;

    public GameObject blobPrefab;

    private int counter = 0;
    // Start is called before the first frame update
    private void Start()
    {
        TotalEnergyRate = 0;

        worldGen = GameObject.Find("Manager").GetComponent<WorldGen>();
        env = GameObject.Find("Manager").GetComponent<Environment>();
        uiManager = GameObject.Find("Manager").GetComponent<UIManager>();

        regionAQI = Random.Range(0, 50);
        regionWQI = Random.Range(0, 50);
        regionTemperature = Random.Range(61, 70);

        SpawnStructures();
        SpawnBlobs();
        population = Blobs.Length;
    }

    private void Update()
    {
        if (regionAQI < 0)
        {
            regionAQI = 0;
        }
        if (regionWQI < 0)
        {
            regionWQI = 0;
        }
    }

    private void OnMouseDown()
    {
        TotalEnergy();
        Color regionColor = GetComponent<Renderer>().material.color;
        if (env.selectedRegionOBJ == gameObject || env.selectedRegionOBJ == null)
        {
            if (regionColor == Color.yellow)
            {
                regionColor = Color.white;
                env.selectedRegionOBJ = null;
            }
            else if (regionColor == Color.white)
            {
                regionColor = Color.yellow;
                env.selectedRegionOBJ = this.gameObject;
            }
        }
        else
        {
            env.selectedRegionOBJ.GetComponent<Renderer>().material.color = Color.white;
            regionColor = Color.yellow;
            env.selectedRegionOBJ = this.gameObject;
        }
        GetComponent<Renderer>().material.color = regionColor;

        uiManager.AQIText.text = "Air Quality: " + Mathf.Round(regionAQI);
        uiManager.WQIText.text = "Water Quality: " + Mathf.Round(regionWQI);
        uiManager.temperatureText.text = Mathf.Round(regionTemperature) + "°" + env.symbol;

    }

    public void SpawnStructures()
    {
        foreach (GameObject obj in Structures)
        {
            Vector3 spawnPos = new Vector3(Random.Range(transform.localPosition.x - transform.localScale.x / 2, transform.localPosition.x + transform.localScale.x / 2),
                obj.transform.localPosition.y,
                Random.Range(transform.localPosition.z - transform.localScale.z / 2, transform.localPosition.z + transform.localScale.z / 2));
            Structure objScript = obj.GetComponent<Structure>();
            if (remainingCapacity > objScript.spaceRequirement)
            {
                int chance = Random.Range(0, 100);
                if (chance <= objScript.spawnChance)
                {
                    GameObject structure = Instantiate(obj);
                    structure.name = obj.name + counter;
                    structure.transform.localPosition = spawnPos;
                    structure.transform.SetParent(this.transform);
                    remainingCapacity -= objScript.spaceRequirement;
                }
            }
        }

        if (remainingCapacity > 10)
        {
            SpawnStructures();
        }

    }

    public void SpawnBlobs()
    {
        Vector3 spawnPos = new Vector3(transform.position.x, 5, transform.position.z);
        int pop = Random.Range(1, 10);
        Blobs = new GameObject[pop];
        for (int i = 0; i < pop; i++)
        {
            GameObject blob = Instantiate(blobPrefab, spawnPos, Quaternion.identity);
            Blobs[i] = blob;
            blob.GetComponent<Blob>().myRegionOBJ = gameObject;
        }
    }

    public void TotalEnergy()
    {
        TotalEnergyRate = 0;
        TotalEnergyAmount = 0;
        int counter = 0;
        foreach (Transform child in transform)
        {
            TotalEnergyAmount += child.GetComponent<Structure>().EnergyConsumptionAmount;
            TotalEnergyRate += child.GetComponent<Structure>().EnergyConsumptionRate;
            counter++;
        }
        //TotalEnergyRate /= counter;
    }

}