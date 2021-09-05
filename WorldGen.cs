using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGen : MonoBehaviour
{
    public int width, height, offset;
    public GameObject region;

    // Start is called before the first frame update
    void Start()
    {
        for(int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tile = Instantiate(region, new Vector3(x * offset, 0, z * offset), Quaternion.identity);
                tile.name = "Region " + x + "," + z;
            }
        }
    }
}
