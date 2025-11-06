using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    // A prefab to instantiate
    [SerializeField] protected T prefab;

    public List<T> pooledObjects;
    private int amount;
    private bool isReady;

    private GameObject objectToInstantiate;

    // Create the pool with a specified amount of objects
    public void CreatePool(int amountToPool = 0)
    {
        if (amountToPool <= 0)
        {
            Debug.LogError("Amount to pool must be greater than zero.");
            return;
        }

        amount = amountToPool;
        pooledObjects = new List<T>(amount);

        objectToInstantiate = new GameObject(typeof(T).Name + " Pool");

        for (int i = 0; i < amount; i++)
        {
            objectToInstantiate = Instantiate(prefab.gameObject, transform);
            objectToInstantiate.gameObject.SetActive(false);
            pooledObjects.Add(objectToInstantiate.GetComponent<T>());
        }

        isReady = true;
    }

    // Get an object from the pool
    public virtual T GetPooledObject()
    {
        if (!isReady)
        {
            Debug.LogWarning("Object pool is not initialized. Call CreatePool first.");
            return null;
        }

        // If there are any inactive objects, return the first one found
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].isActiveAndEnabled)
            {
                return pooledObjects[i];
            }
        }

        // If no inactive objects are available, create a new one
        objectToInstantiate = Instantiate(prefab.gameObject, transform);
        objectToInstantiate.SetActive(false);
        pooledObjects.Add(objectToInstantiate.GetComponent<T>());
        amount++;

        return objectToInstantiate.GetComponent<T>();
    }

    // Return object to the pool
    public void ReturnToPool(T obj)
    {
        if (obj == null)
        {
            return;
        }
        if (!isReady)
        {
            CreatePool();
            pooledObjects.Add(obj);
        }

        obj.gameObject.SetActive(false);
    }

}
