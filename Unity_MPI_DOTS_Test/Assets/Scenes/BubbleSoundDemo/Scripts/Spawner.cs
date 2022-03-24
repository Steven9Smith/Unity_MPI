using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public Float_BubbleSoundDataCondensedDynamic _default;
    public int amountSpawnedWithinASecond = 1;
    public float3 spawnBounds;
    private AudioSource audioSource;
    public float2 radiusRange,depthRange;
    public GameObject prefab;

    private float timeLeft = 1f;
    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = this.gameObject.AddComponent<AudioSource>();


        //	var a = AudioClip.Create("Test", wav_data.Length, 1, 41000, false);
        //	a.SetData(wav_data, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timeLeft >= 0)
            timeLeft -= Time.deltaTime;
        else
        {
            var go = GameObject.Instantiate(prefab);
            Unity.Mathematics.Random r = new Unity.Mathematics.Random();
            go.transform.position = new Vector3(
                   r.NextFloat(0,spawnBounds.x),
                   r.NextFloat(0,spawnBounds.y),
                   r.NextFloat(0,spawnBounds.x)
                );
            timeLeft = 1f;
        }
    }

}
