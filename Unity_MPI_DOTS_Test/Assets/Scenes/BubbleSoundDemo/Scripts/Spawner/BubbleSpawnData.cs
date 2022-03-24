using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BubbleSpawnData : MonoBehaviour
{
	public Float_BubbleSoundDataCondensedDynamic bubble;
	public int sampleRate = 12000;//41000
	public bool PlayOnStart = true;
	public AudioSource audioSource;
	private AudioClip clip;
	private int currentStep = 0;
	System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


	// Start is called before the first frame update
	void Start()
	{
		bubble = new Float_BubbleSoundDataCondensedDynamic(bubble.interfacetype, bubble.movingtype, bubble.radius, bubble.depth, bubble.from, bubble.to, bubble.start, bubble.end, bubble.steps);
		bubble.Init(Float_BubbleSoundDataCondensedDynamic.Init_Mode.Fast_Formatted, true, true);

		PlayBubbleSound(name, bubble.formatted_data, 1, sampleRate, true);
	}

	// Update is called once per frame
	void Update()
	{
		
	}
	private string path;
	
	public void PlayBubbleSound(string name, float[] wave_data, int channels = 1, int sampleRate = 41000, bool stream = false)
	{
		clip = AudioClip.Create(name, wave_data.Length, channels, sampleRate, stream, OnAudioRead/*,OnAudioSetPosition*/);
		if (audioSource.isPlaying)
			audioSource.Stop();
		audioSource.clip = clip;
		audioSource.Play();
	}

	void OnAudioRead(float[] data)
	{
		int total = bubble.formatted_data.Length - currentStep - data.Length;
		if (total > data.Length) total = data.Length;
		else if (total <= 0 && currentStep < bubble.formatted_data.Length)
			total = data.Length + total;
		if (total <= 0)
		{
			for (int i = 0; i < data.Length; i++)
				data[i] = 0;
			return;
		}
		else
		{
			for (int i = 0; i < total; i++)
			{
				float value = 0;
				value += bubble.formatted_data[currentStep + i];
				data[i] = ClampToValidRange(value);

			}
			currentStep += data.Length;
		}
	}

	void OnAudioSetPosition(int newPosition)
	{

	}
	private float ClampToValidRange(float value)
	{
		float min = -1.0f;
		float max = 1.0f;
		return (value < min) ? min : (value > max) ? max : value;
	}

	private float[] MixAndClampFloatBuffers(float[] bufferA, float[] bufferB)
	{
		int maxLength = math.min(bufferA.Length, bufferB.Length);
		float[] mixedFloatArray = new float[maxLength];

		for (int i = 0; i < maxLength; i++)
		{
			mixedFloatArray[i] = ClampToValidRange((bufferA[i] + bufferB[i]) / 2);
		}
		return mixedFloatArray;
	}
}
