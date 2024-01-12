using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerInventory : MonoBehaviour
{
    GameManager gameManager;
    PlayerMovement movement;
    SoundManager soundManager;
    LevelManager levelManager;

    public int maxInventorySize = 5;
    public List<GameObject> inventory;
    public float inventoryRange = 30; // how far away the inventory items can go before being removed

    [Header("Follow Player")]
    public float speed = 30;
    public float spacing = 1;

    [Header("Circle Player")]
    private float currCircleAngle = 0f; // Current angle of rotation
    public float circleSpeed = 1f; // Speed of rotation
    public float circleSpacing = 1f; // Spacing between objects
    public float circleRadius = 1f; // Radius of circle

    [Header("Drop Items")]
    public float itemDropForce = 20;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        soundManager = gameManager.soundManager;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // << STATE MACHINE >>
        /*
        if (movement.state == PlayerState.MOVING) { InventoryFollowPlayer(); }
        else if (movement.state == PlayerState.CHARGING) { InventoryChargeRadius(); }
        else { InventoryCirclePlayer(); }
        */

        // remove null values in the inventory
        RemoveNullValues(inventory);

        InventoryCirclePlayer();

        UpdateRemoveByDistance(inventoryRange);
    }

    public void AddItemToInventory(GameObject itemObject)
    {
        if (inventory.Count >= maxInventorySize)
        {
            Debug.Log("Inventory is full, can't add any more items");
            itemObject.GetComponent<Item>().state = ItemState.FREE;
            return;
        }


        itemObject.transform.parent = transform;
        Item item = itemObject.GetComponent<Item>();
        item.rb.velocity = Vector2.zero; // stop velocity

        inventory.Add(itemObject);
        item.state = ItemState.PLAYER_INVENTORY;

        // Debug.Log("Player picked up " + itemObject.name, itemObject);


        // Play Sound
        if (item.type == ItemType.LIGHT)
        {
            soundManager.Play(soundManager.lightPickupSound);
        }
    }

    public GameObject StealItem(GameObject item)
    {
        if (inventory.Contains(item))
        {
            item.transform.parent = null;
            inventory.Remove(item);
            item.GetComponent<Item>().state = ItemState.STOLEN;

            return item;
        }

        return null;
    }

    public GameObject RemoveItem(GameObject item)
    {
        if (inventory.Contains(item))
        {
            item.transform.parent = null;
            inventory.Remove(item);
            item.GetComponent<Item>().state = ItemState.FREE;

            return item;
        }

        return null;
    }

    public GameObject RemoveItemToThrow(GameObject item)
    {
        if (inventory.Contains(item))
        {
            item.transform.parent = null;
            inventory.Remove(item);

            return item;
        }

        return null;
    }

    public void Destroy()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            inventory[i].GetComponent<Item>().Destroy();
        }

        inventory.Clear();
    }

    public GameObject GetMostExpensiveItem()
    {
        if (inventory.Count == 0) { return null; }

        int largestLifeForce = -1000;
        GameObject expensiveItem = inventory[0];

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null) { continue; }
            Item item = inventory[i].GetComponent<Item>();
            if (item.lifeForce > largestLifeForce) { expensiveItem = item.gameObject; }
        }

        return expensiveItem;
    }

    public int GetTypeCount(ItemType type)
    {
        if (inventory.Count == 0) { return 0; }

        int count = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null) { continue; }

            // add to count if matches type
            Item item = inventory[i].GetComponent<Item>();
            if (item.type == type) { count++; }
        }

        return count;
    }

    public GameObject GetFirstItemOfType(ItemType type)
    {
        if (inventory.Count == 0) { return null; }

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null) { continue; }

            // return if matches type
            Item item = inventory[i].GetComponent<Item>();
            if (item.type == type) { return item.gameObject; }
        }

        return null;
    }

    public void UpdateRemoveByDistance(float distance = 10)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (Vector2.Distance(transform.position, inventory[i].transform.position) > distance)
            {
                RemoveItem(inventory[i]);
            }
        }
    }

    public void DropAllItems()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            GameObject obj = inventory[i];

            obj.GetComponent<Item>().state = ItemState.FREE;
            obj.GetComponent<Item>().ResetSortingOrder();

            // remove from inventory and set state
            RemoveItem(obj);
            obj.transform.parent = null;

            // add force pushing item to opposite direction
            obj.GetComponent<Rigidbody2D>().AddForce( (transform.position - obj.transform.position) * itemDropForce, ForceMode2D.Impulse);
        }
    }

    private void RemoveNullValues(List<GameObject> list)
    {
        list.RemoveAll(item => item == null);
    }

    #region Inventory Movement ====================
    public void ItemFollowTarget(GameObject itemObj, Transform target, float speed = 1, float spacing = 2)
    {
        Vector3 direction = movement.moveDirection; // get direction of target movement

        // << move obj to new position >> 
        Vector3 newPos = target.position - (direction * spacing); // Calculate new follower position in opposite direction of target movement
        itemObj.GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(itemObj.transform.position, newPos, speed * Time.deltaTime)); // Move follower towards new position using Lerp

    }

    public void InventoryFollowPlayer()
    {
        Vector3 direction = movement.moveDirection; // get direction of target movement

        Vector3 prevFollowerPos = transform.position; // Set initial previous follower position to current position

        foreach (GameObject obj in inventory)
        {
            Vector3 newPos = prevFollowerPos - (direction * spacing); // Calculate new follower position in opposite direction of target movement
            obj.GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(obj.transform.position, newPos, speed * Time.deltaTime)); // Move follower towards new position using Lerp

            prevFollowerPos = obj.transform.position; // Update previous follower position to current follower position
        }
    }

    public void InventoryCirclePlayer()
    {
        currCircleAngle += circleSpeed * Time.deltaTime; // Update angle of rotation

        Vector3 targetPos = transform.position;
        targetPos.z = 0f; // Ensure target position is on the same plane as objects to circle

        for (int i = 0; i < inventory.Count; i++)
        {
            float angleRadians = (currCircleAngle + (360f / inventory.Count) * i) * Mathf.Deg2Rad; // Calculate angle in radians for each object
            Vector3 newPos = targetPos + new Vector3(Mathf.Cos(angleRadians) * circleRadius, Mathf.Sin(angleRadians) * circleRadius, 0f); // Calculate new position for object
            inventory[i].GetComponent<Rigidbody2D>().MovePosition(Vector3.Lerp(inventory[i].transform.position, newPos, speed * Time.deltaTime)); // Move object towards new position using Lerp
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, inventoryRange);
    }
}
