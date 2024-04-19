using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class AudioRoutine : MonoBehaviour
{
    public AudioSource AudioSource;
    public GameObjectDespawnHandle AutoDespawnHandle;
    public AssetReferenceEntity ReferenceEntity;

    public void Init()
    {
        AudioSource = GetComponent<AudioSource>();
        AutoDespawnHandle = GetComponent<GameObjectDespawnHandle>();
    }
}
