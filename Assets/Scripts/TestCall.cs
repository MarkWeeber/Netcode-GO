using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class TestCall : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private MyClass myClass;
    private GameObject go;
    private void Awake()
    {
        Debug.Log("Awake");
        myClass = new MyClass();
        go = Instantiate(prefab);
        if(go.TryGetComponent<NetworkObject>(out NetworkObject no))
        {
            no.Spawn();
        }
    }

    private void OnEnable()
    {
        Debug.Log("On Enable");
    }

    private void Start()
    {
        Debug.Log("Start");
        MemberInfo[] x = typeof(IDisposable).GetMembers();
        foreach (MemberInfo item in x)
        {
            Debug.Log("IDisposable");
        }
    }

    private void OnDisable()
    {
        Debug.Log("On Disable");
    }

    private void OnDestroy()
    {
        Debug.Log("On Destroy");
        Destroy(go);
        
    }
}

public class MyClass : IDisposable
{
    public MyClass()
    {
        Debug.Log("Constructor");
    }
    ~MyClass()
    {
        Debug.Log("Desctructor");
    }
    public void Dispose()
    {
        Debug.Log("Dispose");
    }
}
