using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDA.BlastTest
{
    public class ObjectPooler : MonoBehaviour
    {
        #region Instance
        public static ObjectPooler SharedInstance
        {
            get
            {
                if (sharedInstance == null)
                    sharedInstance = FindObjectOfType(typeof(ObjectPooler)) as ObjectPooler;

                return sharedInstance;
            }
            set
            {
                sharedInstance = value;
            }
        }
        private static ObjectPooler sharedInstance;
        #endregion

        public List<GameObject> pooledObjects;

        public List<GameObject> pooledObjectsParticles;

        public List<GameObject> itemsToPool;

        // Start is called before the first frame update
        void Start()
        {
            //CreateObject(itemsToPool[0]);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void CreateObject(GameObject item)
        {
            if (item == null) return;
            for(int i = 0; i < 10; i++)
            {
                CreatePooledObject(item);
            }            
        }
        public GameObject CreatePooledObject(GameObject item)
        {
            GameObject obj = Instantiate(item);
            obj.transform.SetParent(transform);
            obj.name = item.name;
            obj.transform.localScale = item.transform.localScale;
            obj.SetActive(false);
            pooledObjects.Add(obj);
            return obj;
        }
        public GameObject GetPooledObject(string name, bool active = true, bool canBeActive = false)
        {
            pooledObjects.RemoveAll(i => i == null);
            GameObject obj = null;
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (pooledObjects[i] == null) continue;
                if ((!pooledObjects[i].gameObject.activeSelf || canBeActive) && pooledObjects[i].name == name)
                {
                    Item item = pooledObjects[i].GetComponent<Item>();
                    if (item)
                        obj = pooledObjects[i];
                    else if (!item)
                        obj = pooledObjects[i];
                    if (obj) break;
                }
            }
            if (!obj)
            {
                foreach (var item in itemsToPool)
                {
                    obj = CreatePooledObject(item);
                    break;
                }
            }
            if (obj != null)
            {
                obj.gameObject.SetActive(active);
                return obj.gameObject;
            }
            return null;
        }
        public GameObject GetPooledParticle(string name)
        {
            pooledObjectsParticles.RemoveAll(i => i == null);
            GameObject obj = null;
            for (int i = 0; i < pooledObjectsParticles.Count; i++)
            {
                if (pooledObjectsParticles[i] == null) continue;
                if (!pooledObjectsParticles[i].gameObject.activeSelf && pooledObjectsParticles[i].name == name)
                {
                    ParticleSystem particle = pooledObjectsParticles[i].GetComponent<ParticleSystem>();
                    if (particle)
                        obj = pooledObjectsParticles[i];
                    else if (!particle)
                        obj = pooledObjectsParticles[i];
                    if (obj) break;
                }
            }
            if (obj != null)
            {
                obj.gameObject.SetActive(true);
                return obj.gameObject;
            }
            return null;
        }
        public void PutBack(GameObject obj)
        {
            obj.SetActive(false);
        }
    }
}