using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleSettings", menuName = "GameSettings/CollectibleSettings")]
public class CollectibleSettings : ScriptableObject
{
    public GameObject collectiblePrefab;
    public int totalCollectibles = 50;
    public float minDistanceBetweenCollectibles = 10f;
    public float collectibleRadius = 10f;
    public string excludedScriptName = "BuildingMarker";
    public AudioSource winAudioSource; // Optional
}
