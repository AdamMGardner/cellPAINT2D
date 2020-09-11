using System;
using System.Collections.Generic;
//using Unity.Physics;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Collections;
using UnityEngine;
//using Unity.Transforms;
//using Collider = Unity.Physics.Collider;

[Serializable]
public class ManagerECS : BasePhysicsDemo
{
    /*
    public Dictionary<string,Entity> sourceEntitys;
    public Dictionary<string,BlobAssetReference<Collider>> sourceColliders;
    private static ManagerECS _instance=null;
  
    public static ManagerECS Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<ManagerECS>();
            if (_instance == null)
            {
                var go = GameObject.Find("ManagerECS");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("ManagerECS"); //{ hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<ManagerECS>();
            }
            return _instance;
        }
    }
    
    void SetupFromManager(){
        //after everything is loaded and all prefab are available
        //convert to ECS
        sourceEntitys.Clear();
        sourceColliders.Clear();
        foreach(var KeyVal in Manager.Instance.all_prefab)
        {
            var name = KeyVal.Key;
            var prefab = KeyVal.Value;
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, new BlobAssetStore());
            // Create entity prefab from the game object hierarchy once
            var sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);
            var entityManager = BasePhysicsDemo.DefaultWorld.EntityManager;
            sourceEntitys.Add(name,sourceEntity);

            var sourceCollider = entityManager.GetComponentData<PhysicsCollider>(sourceEntity).Value;
            sourceColliders.Add(name,sourceCollider);
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        sourceEntitys = new Dictionary<string,Entity>();
        sourceColliders = new Dictionary<string,BlobAssetReference<Collider>>();
        float3 gravity = new float3(0, 0.0f, 0);
        base.init(gravity, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InstanceOnePrefab(Vector3 position, Quaternion quat, String entityName)
    {
        var instance = BasePhysicsDemo.DefaultWorld.EntityManager.Instantiate(sourceEntitys[entityName]);
        BasePhysicsDemo.DefaultWorld.EntityManager.SetComponentData(instance, new Translation { Value = position });
        BasePhysicsDemo.DefaultWorld.EntityManager.SetComponentData(instance, new Rotation { Value = quat });
        BasePhysicsDemo.DefaultWorld.EntityManager.SetComponentData(instance, new PhysicsCollider { Value = sourceColliders[entityName] });

    }
    */
}
