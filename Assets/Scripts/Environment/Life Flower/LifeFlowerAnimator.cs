using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LifeFlowerAnimator : MonoBehaviour
{
    LifeFlower flower;

    [Header("Animation")]
    public Light2D flowerLight;
    public SpriteRenderer crystalOutline;

    [Space(5)]
    public SpriteRenderer innerHex;
    public SpriteRenderer pentagram;

    [Space(5)]
    public Transform petals;

    [Header("Colors")]
    public Color currColor;

    [Space(10)]
    public Color healthyColor = Color.magenta;
    public float healthyLightIntensity = 3;
    public float healthyLightRadius = 75;

    [Space(10)]
    public Color deathColor = Color.black;
    public float deathLightIntensity = 0;
    public float deathLightRadius = 5;

    [Space(10)]
    public Color healedColor = Color.white;
    public float healedLightIntensity = 5;
    public float healedLightRadius = 500;

    [Space(10)]
    public float effectRotationSpeed = 10;
    public GameObject healedEffect;
    public GameObject generalEffect;
    public GameObject deathEffect;
    public GameObject aggressiveBurst;

    // Start is called before the first frame update
    void Start()
    {
        flower = GetComponent<LifeFlower>();
    }

    // Update is called once per frame
    void Update()
    {

        // << FLOWER LIGHT >>
        if (flower.state != FlowerState.HEALED && flower.state != FlowerState.DEAD)
        {
            float lifeForceRatio = (float)flower.lifeForce / (float)flower.maxLifeForce;

            currColor = Color.Lerp(deathColor, healthyColor, lifeForceRatio);

            // scale intensity to current flower health
            flowerLight.pointLightOuterRadius = Mathf.Lerp(deathLightRadius, healthyLightRadius, lifeForceRatio);
            flowerLight.intensity = Mathf.Lerp(deathLightIntensity, deathLightRadius, lifeForceRatio);

            if (flower.state == FlowerState.SICK || flower.state == FlowerState.NEAR_DEATH) 
            { 
                deathEffect.SetActive(true);
                generalEffect.SetActive(false);

                Rotate(deathEffect.transform, effectRotationSpeed);
            }
            else
            {
                deathEffect.SetActive(false);
                generalEffect.SetActive(true);

                Rotate(generalEffect.transform, effectRotationSpeed);

            }
        }
        else
        {
            // << WIN STATE >>
            if (flower.state == FlowerState.HEALED)
            {
                currColor = Color.Lerp(currColor, healedColor, Time.deltaTime);

                flowerLight.pointLightOuterRadius = Mathf.Lerp(flowerLight.pointLightOuterRadius, healedLightRadius, Time.deltaTime);
                flowerLight.intensity = Mathf.Lerp(flowerLight.intensity, healedLightIntensity, Time.deltaTime);

                healedEffect.SetActive(true);
                generalEffect.SetActive(false);
                deathEffect.SetActive(false);

                Rotate(healedEffect.transform, effectRotationSpeed);

            }

            // << FAIL STATE >>
            else if (flower.state == FlowerState.DEAD)
            {
                currColor = Color.Lerp(currColor, deathColor, Time.deltaTime);

                flowerLight.pointLightOuterRadius = Mathf.Lerp(flowerLight.pointLightOuterRadius, deathLightRadius, Time.deltaTime);
                flowerLight.intensity = Mathf.Lerp(flowerLight.intensity, deathLightIntensity, Time.deltaTime);

                deathEffect.SetActive(true);
                generalEffect.SetActive(false);
                healedEffect.SetActive(false);

                Rotate(deathEffect.transform, effectRotationSpeed);

            }
        }

    }

    public void Rotate(Transform transform, float speed)
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }

    public void SpawnAggressiveBurstEffect()
    {
        GameObject effect = Instantiate(aggressiveBurst, transform.position, Quaternion.identity);

        Destroy(effect, 5);
    }
}
