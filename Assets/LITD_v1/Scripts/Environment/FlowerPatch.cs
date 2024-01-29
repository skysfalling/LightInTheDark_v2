using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPatch : MonoBehaviour
{
    public Totem connectedTotem; // totem to compare life force
    public List<Transform> flowerGrowthPositions; // positions of each flower
    public List<GameObject> flowerMidGrowthPrefabs; // prefabs to use when in mid growth
    public List<GameObject> flowerFullGrowthPrefabs; // prefabs to use when in full growth

    float midRangeStartPercentage = 0.2f; // starting percentage for mid-range growth
    float fullGrowthStartPercentage = 0.4f; // starting percentage for full growth

    // Keep track of the currently growing flowers
    private List<GameObject> growingFlowers = new List<GameObject>();
    private List<int> growthStages = new List<int>();


    // Start is called before the first frame update
    void Start()
    {
        // Spawn initial mid-growth flowers
        foreach (Transform position in flowerGrowthPositions)
        {
            growingFlowers.Add(null);
            growthStages.Add(0);
        }

        fullGrowthStartPercentage = (float)1 / (float)flowerGrowthPositions.Count;
        midRangeStartPercentage = (float)(1 - fullGrowthStartPercentage) / (float)flowerGrowthPositions.Count;

    }

    void Update()
    {
        float totemLifePercentage = connectedTotem.GetLifeForcePercentage();

        for (int i = 0; i < growingFlowers.Count; i++)
        {
            if (growingFlowers[i] == null && growthStages[i] == 0) // no flower at this position yet
            {
                if (totemLifePercentage >= (i + 1) * midRangeStartPercentage)
                {
                    // Spawn a new mid-growth flower at the empty spot
                    GameObject newFlower = Instantiate(flowerMidGrowthPrefabs[Random.Range(0, flowerMidGrowthPrefabs.Count)], flowerGrowthPositions[i].position, Quaternion.identity);
                    growingFlowers[i] = newFlower;
                    growthStages[i] = 1;
                }
                else
                {
                    growthStages[i] = 0;
                }
            }
            else // there is a flower at this position already
            {
                int newGrowthStage = growthStages[i];

                if (totemLifePercentage >= (i + 1) * fullGrowthStartPercentage)
                {
                    // Upgrade to full-growth flower
                    newGrowthStage = 2;
                }
                else if (totemLifePercentage >= (i + 1) * midRangeStartPercentage)
                {
                    // Mid-range growth
                    newGrowthStage = 1;
                }
                else
                {
                    // Destroy the flower and leave the spot empty
                    Destroy(growingFlowers[i]);
                    growingFlowers[i] = null;
                    growthStages[i] = 0;
                    newGrowthStage = 0;
                }

                if (newGrowthStage != growthStages[i] && newGrowthStage > 0)
                {
                    if (newGrowthStage == 2)
                    {
                        // Upgrade mid-growth flower to full-growth flower
                        Destroy(growingFlowers[i]);
                        GameObject newFlower = Instantiate(flowerFullGrowthPrefabs[Random.Range(0, flowerFullGrowthPrefabs.Count)], flowerGrowthPositions[i].position, Quaternion.identity);
                        growingFlowers[i] = newFlower;
                    }
                    else if (newGrowthStage == 1)
                    {
                        // Downgrade full-growth flower to mid-growth flower
                        Destroy(growingFlowers[i]);
                        GameObject newFlower = Instantiate(flowerMidGrowthPrefabs[Random.Range(0, flowerMidGrowthPrefabs.Count)], flowerGrowthPositions[i].position, Quaternion.identity);
                        growingFlowers[i] = newFlower;
                    }

                    growthStages[i] = newGrowthStage;
                }
            }
        }
    }

}
