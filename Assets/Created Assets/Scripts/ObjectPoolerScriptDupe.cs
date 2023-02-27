using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolerScriptDupe : MonoBehaviour
{
    #region Fields

    public static ObjectPoolerScriptDupe current;
    public GameObject pooledObject;
    public int pooledAmount = 8;
    public bool willGrow = false;

    private List<GameObject> pooledObjects;

    #endregion Fields

    #region Methods

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                
                return pooledObjects[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            pooledObjects.Add(obj);
            return obj;
        }

        return null;
    }

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    #endregion Methods
}