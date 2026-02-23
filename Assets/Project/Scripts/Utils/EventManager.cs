using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private Dictionary<string, Delegate> eventDictionary = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ========================
    // ADD LISTENER
    // ========================
    public void AddListener<T>(string eventName, Action<T> listener)
    {
        //Debug.Log($"Listener adicionado para o evento '{eventName}' do tipo {typeof(T).Name}");

        if (!eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            if (existingDelegate is Action<T> typedDelegate)
                eventDictionary[eventName] = typedDelegate + listener;
            else
                Debug.LogError($"Tentando adicionar listener de tipo diferente para o evento '{eventName}'");
        }
    }

    public void AddListener(string eventName, Action listener)
    {
        //Debug.Log($"Listener adicionado para o evento '{eventName}' sem parâmetro");

        if (!eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            eventDictionary[eventName] = listener;
        }
        else
        {
            if (existingDelegate is Action typedDelegate)
                eventDictionary[eventName] = typedDelegate + listener;
            else
                Debug.LogError($"Tentando adicionar listener de tipo diferente para o evento '{eventName}'");
        }
    }

    // ========================
    // REMOVE LISTENER
    // ========================
    public void RemoveListener<T>(string eventName, Action<T> listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T> typedDelegate)
            {
                typedDelegate -= listener;
                if (typedDelegate == null)
                    eventDictionary.Remove(eventName);
                else
                    eventDictionary[eventName] = typedDelegate;
            }
        }
    }

    public void RemoveListener(string eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action typedDelegate)
            {
                typedDelegate -= listener;
                if (typedDelegate == null)
                    eventDictionary.Remove(eventName);
                else
                    eventDictionary[eventName] = typedDelegate;
            }
        }
    }

    // ========================
    // INVOKE
    // ========================
    public void Invoke<T>(string eventName, T param)
    {

        //Debug.Log($"Invocando evento '{eventName}' com parâmetro do tipo {typeof(T).Name}");
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action<T> typedDelegate)
                typedDelegate.Invoke(param);
            else
                Debug.LogError($"Tipo incorreto ao tentar invocar '{eventName}'");
        }
    }

    public void Invoke(string eventName)
    {
        //Debug.Log($"Invocando evento '{eventName}' sem parâmetro");
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is Action typedDelegate)
                typedDelegate.Invoke();
            else
                Debug.LogError($"Tipo incorreto ao tentar invocar '{eventName}'");
        }
    }
}
