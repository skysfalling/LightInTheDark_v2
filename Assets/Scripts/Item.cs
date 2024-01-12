using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum ItemType { LIGHT, DARKLIGHT, GOLDEN, ETHEREAL}
public enum ItemState { FREE, PLAYER_INVENTORY, SUBMITTED, STOLEN, THROWN }


public class Item : MonoBehaviour
{
    PlayerMovement playerMovement;
    PlayerInventory playerInventory;
    FMODUnity.StudioEventEmitter studioEmitter;
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public Light2D light;

    SpriteRenderer sr;
    string defaultSortingLayer;
    int defaultSortingOrder;

    public ItemType type;
    public ItemState state = ItemState.FREE;
    public float triggerSize = 0.75f;
    public int lifeForce = 10;

    [Header("Light")]
    public float default_lightRange = 5;
    public float default_lightIntensity = 5;
    [Space(10)]
    public float inventory_lightRange = 5;
    public float inventory_lightIntensity = 5;
    [Space(10)]
    public float thrown_lightRange = 5;
    public float thrown_lightIntensity = 5;

    [Header("Effects")]
    public GameObject destroyEffect;

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
        playerMovement = playerInventory.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        light = GetComponent<Light2D>();
        studioEmitter = GetComponent<FMODUnity.StudioEventEmitter>();

        sr = GetComponent<SpriteRenderer>();
        defaultSortingLayer = sr.sortingLayerName;
        defaultSortingOrder = sr.sortingOrder;
    }

    // Update is called once per frame
    void Update()
    {
        if ( (state == ItemState.FREE || state == ItemState.THROWN) && this.gameObject != playerMovement.throwObject)
        {

            Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, triggerSize);
            List<Collider2D> collidersInTrigger = new List<Collider2D>(overlapColliders);

            foreach (Collider2D col in collidersInTrigger)
            {
                if (col.tag == "Player")
                {
                    playerInventory.AddItemToInventory(this.gameObject);
                }
            }
        }

        if (state == ItemState.STOLEN) { EnableCollider(false); }
        else { EnableCollider(true); }

        if (state == ItemState.PLAYER_INVENTORY) { UpdateItemLight(inventory_lightRange, inventory_lightIntensity); }
        else if (state == ItemState.THROWN) { UpdateItemLight(thrown_lightRange, thrown_lightIntensity, 2); }
        else { UpdateItemLight(default_lightRange, default_lightIntensity); }

        // if (studioEmitter && state == ItemState.PLAYER_INVENTORY) { studioEmitter.Play(); }

    }

    // immediate change in light
    public void SetItemLight(float outerRange, float intensity)
    {
        light.pointLightOuterRadius = outerRange;
        light.intensity = intensity;
    }

    // for use in update functions
    public void UpdateItemLight(float outerRange, float intensity, float speed = 1)
    {
        light.pointLightOuterRadius = Mathf.Lerp(light.pointLightOuterRadius, outerRange, speed * Time.deltaTime);
        light.intensity = Mathf.Lerp(light.intensity, intensity, speed * Time.deltaTime); ;
    }

    public void SetSortingOrder(int order, string layerName)
    {
        sr.sortingLayerName = layerName;
        sr.sortingOrder = order;
    }

    public void ResetSortingOrder()
    {
        sr.sortingLayerName = defaultSortingLayer;
        sr.sortingOrder = defaultSortingOrder;
    }

    public void Destroy()
    {
        if (state == ItemState.PLAYER_INVENTORY || state == ItemState.SUBMITTED) { return; }

        GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        Destroy(effect, 5);

        Destroy(this.gameObject);
    }

    public void Destroy(float delay)
    {
        StartCoroutine(DestroyDelay(delay));
    }

    IEnumerator DestroyDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy();
    }

    public void EnableCollider(bool enabled)
    {
        GetComponent<CapsuleCollider2D>().enabled = enabled;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerSize);
    }
}
