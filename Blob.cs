using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Blob : MonoBehaviour
{
    Environment env;

    public GameObject myRegionOBJ;
    public enum PrimaryState { Idle, SearchFood, SearchHome, SearchTemperature };
    public PrimaryState blobState;

    NavMeshAgent agent;
    public float walkRadius;
    public float timer;
    public float decisionTimer;
    float reproduceTimer;
    float eatTimer;

    [Range(0, 100)]
    public float health; // 0 Death, 100 Perfect Condition, under 50 Danger
    [Range(0, 100)]
    public float energy; // 100 Motivated/Eager, 50 indifferent, 0 Must sleep or begin losing health
    [Range(0, 100)]
    public float hunger;  // 100 Full, 50 kinda hungry, under 30 begin losing energy, 0 begin losing health
    [Range(0, 100)]
    public float comfort; // How bothered by the environment the blob is, the lower the more bothered.
    [Range(0, 100)]
    public float urgency; // The higher the more often blob decides its task 

    public GameObject Car;
    public GameObject blobPrefab;

    float distanceToStore;

    private void Start()
    {
        env = GameObject.Find("Manager").GetComponent<Environment>();
        blobState = PrimaryState.Idle;
        timer = 5f;
        decisionTimer = Mathf.Abs(urgency - 100) * 0.1f + 2.5f;

        Car.SetActive(false);

        health = Random.Range(70, 100);
        energy = Random.Range(70, 100);
        hunger = Random.Range(70, 100);
        comfort = Random.Range(70, 100);

        agent = GetComponent<NavMeshAgent>();

        agent.SetDestination(RandomNavSphere(transform.position, walkRadius, -1));

        InvokeRepeating("Emissions", 1, 5);
    }

    private void Update()
    {
        urgency = 100 - health;

        if (blobState == PrimaryState.Idle)
        {
            timer -= Time.deltaTime;
            comfort += Time.deltaTime * 0.075f;
        }
        else
        {
            timer = 5f;
            comfort -= Time.deltaTime * 0.1f;
        }

        if (blobState == PrimaryState.SearchFood && agent.remainingDistance < 0.1f)
        {
            eatTimer -= Time.deltaTime;
            if (eatTimer <= 0.0f)
            {
                eatTimer = 8f;
                hunger += 10;
                blobState = PrimaryState.Idle;
                Car.SetActive(false);
                agent.speed = 5f;
            }
        }

        if (blobState == PrimaryState.SearchHome && agent.remainingDistance < 0.1f)
        {
            reproduceTimer -= Time.deltaTime;
            if (reproduceTimer <= 0.0f)
            {
                reproduceTimer = 15f;
                GameObject blob = Instantiate(blobPrefab, myRegionOBJ.transform.position, Quaternion.identity);
                blob.GetComponent<Blob>().myRegionOBJ = myRegionOBJ;
                blobState = PrimaryState.Idle;
                agent.destination = new Vector3(Random.Range(myRegionOBJ.transform.localPosition.x - myRegionOBJ.transform.localScale.x / 2, myRegionOBJ.transform.localPosition.x + myRegionOBJ.transform.localScale.x / 2),
                0,
                Random.Range(myRegionOBJ.transform.localPosition.z - myRegionOBJ.transform.localScale.z / 2, myRegionOBJ.transform.localPosition.z + myRegionOBJ.transform.localScale.z / 2));
                agent.speed = 5f;
                Car.SetActive(false);
                myRegionOBJ.GetComponent<Region>().population += 1;
            }
        }

        if (agent.remainingDistance < 0.3f && timer <= 0.0f && blobState == PrimaryState.Idle)
        {
            agent.SetDestination(RandomNavSphere(transform.position, walkRadius, -1));
            timer = 5f;
        }

        if (agent.velocity.magnitude > 0)
        {
            energy -= agent.velocity.magnitude * Time.deltaTime * 0.05f;
            hunger -= agent.velocity.magnitude * Time.deltaTime * 0.05f;
        }

        decisionTimer -= Time.deltaTime;
        if (decisionTimer < 0.0f)
        {
            Decision();
            decisionTimer = Mathf.Abs(urgency - 100) * 0.1f + 5;
        }

        if (myRegionOBJ.GetComponent<Region>().regionTemperature > 90 || energy < 0 || hunger < 20)
        {
            health -= Time.deltaTime * 0.5f;
        }

        if (health <= 20)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (health <= 0.0f)
        {
            myRegionOBJ.GetComponent<Region>().regionTemperature -= 2;
            myRegionOBJ.GetComponent<Region>().population -= 1;
            Destroy(this.gameObject);
        }
    }

    public void Decision()
    {
        if (hunger < 60)
        {
            blobState = PrimaryState.SearchFood;
            Vector3 store = new Vector3(Random.Range(myRegionOBJ.transform.localPosition.x - myRegionOBJ.transform.localScale.x / 2, myRegionOBJ.transform.localPosition.x + myRegionOBJ.transform.localScale.x / 2),
                0,
                Random.Range(myRegionOBJ.transform.localPosition.z - myRegionOBJ.transform.localScale.z / 2, myRegionOBJ.transform.localPosition.z + myRegionOBJ.transform.localScale.z / 2));
            foreach (Transform child in myRegionOBJ.transform)
            {
                if (child.GetComponent<Structure>().type == Structure.structureType.Store)
                {
                    store = child.position;
                    int decider = Random.Range(0, 100);
                    if (decider < 50)
                    {
                        store = child.position;
                    }
                    distanceToStore = Vector3.Distance(transform.position, store);
                }
            }
            if (distanceToStore > walkRadius)
            {
                Car.SetActive(true);
                agent.speed = 3 * agent.speed;
                agent.destination = store;
            }
            else
            {
                Car.SetActive(false);
                agent.speed = 5f;
                agent.destination = store;
            }
        }

        if (myRegionOBJ.GetComponent<Region>().regionAQI < 100 &&
            myRegionOBJ.GetComponent<Region>().regionWQI < 40 &&
            myRegionOBJ.GetComponent<Region>().regionEnergy > 0 &&
            myRegionOBJ.GetComponent<Region>().population < env.maxPopulation &&
            health > 70 && comfort > 70)
        {
            blobState = PrimaryState.SearchHome;
            Vector3 home = new Vector3(Random.Range(myRegionOBJ.transform.localPosition.x - myRegionOBJ.transform.localScale.x / 2, myRegionOBJ.transform.localPosition.x + myRegionOBJ.transform.localScale.x / 2),
                0,
                Random.Range(myRegionOBJ.transform.localPosition.z - myRegionOBJ.transform.localScale.z / 2, myRegionOBJ.transform.localPosition.z + myRegionOBJ.transform.localScale.z / 2));
            int loop = 0;
            foreach (Transform child in myRegionOBJ.transform)
            {
                if (child.GetComponent<Structure>().type == Structure.structureType.Home)
                {
                    if (loop == 0)
                    {
                        home = child.position;
                    }
                    int decider = Random.Range(0, 100);
                    if (decider < 50)
                    {
                        home = child.position;
                    }
                    distanceToStore = Vector3.Distance(transform.position, home);
                }
                loop++;
            }
            if (distanceToStore > walkRadius)
            {
                Car.SetActive(true);
                agent.speed = 3 * agent.speed;
                agent.destination = home;
            }
            else
            {
                Car.SetActive(false);
                agent.speed = 5f;
                agent.destination = home;
            }
        }
    }

    public void Emissions()
    {
        myRegionOBJ.GetComponent<Region>().regionAQI += 1;
        myRegionOBJ.GetComponent<Region>().regionWQI += 1;
        myRegionOBJ.GetComponent<Region>().regionTemperature += 0.1f;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;

        randomDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

        return navHit.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, walkRadius);
    }
}