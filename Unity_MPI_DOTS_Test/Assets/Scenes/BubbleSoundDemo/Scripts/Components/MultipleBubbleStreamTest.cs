using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.Profiling;
using System;
using System.Collections.Generic;

public class MultipleBubbleStreamTest : MonoBehaviour
{
	public Float_BubbleSoundDataCondensed[] bubbles;
	public bool PlayOnStart = true;
	[Tooltip("Set to true to save to a file")]
	public bool SaveToFile = false;
	public string SoundFileName = "pop";
	private AudioSource audioSource;
	private AudioClip clip;
	int currentStep = 0;
	System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

	private void OnEnable()
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
		if (PlayOnStart && !audioSource.isPlaying)
		{
			PlayBubbleSound();
			PlayOnStart = false;
		}
	}
	public void PlayBubbleSound()
	{
		for (int i = 0; i < bubbles.Length; i++)
		{
			bubbles[i] = new Float_BubbleSoundDataCondensed(bubbles[i].interfacetype, bubbles[i].movingtype, bubbles[i].radius, bubbles[i].depth, bubbles[i].steps);
			bubbles[i].Init(Float_BubbleSoundDataCondensed.Init_Mode.Fast_Formatted, true);
		}
		PlayBubbleSound(name, new float[bubbles[0].formatted_data.Length], 1, 41000, true);
	}
	public void PlayBubbleSound(string name, float[] wave_data, int channels = 1, int sampleRate = 41000, bool stream = false)
	{
		clip = AudioClip.Create(name, wave_data.Length, channels, sampleRate, stream, OnAudioRead/*,OnAudioSetPosition*/);
		//	clip.SetData(wave_data, 0);
		PlayBubbleSound(clip);

	}
	public void PlayBubbleSound(AudioClip audioClip)
	{
		audioSource.clip = audioClip;
		audioSource.Play();
	}

	void OnAudioRead(float[] data)
	{
		watch.Start();
		for(int i = 0; i < data.Length; i++)
		{
			float value = 0;
			for (int j = 0; j < bubbles.Length; j++)
				value += bubbles[j].formatted_data[currentStep + i];
			data[i] = ClampToValidRange(value/bubbles.Length);
			
		}
		if (currentStep + data.Length >= bubbles[0].formatted_data.Length)
		{
			currentStep = 0;
		}
		else currentStep += data.Length;
		watch.Stop();
		Debug.Log("Combining "+bubbles.Length+" bubbles took "+watch.ElapsedMilliseconds+"ms");
		watch.Reset();

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
		int maxLength = Math.Min(bufferA.Length, bufferB.Length);
		float[] mixedFloatArray = new float[maxLength];

		for (int i = 0; i < maxLength; i++)
		{
			mixedFloatArray[i] = ClampToValidRange((bufferA[i] + bufferB[i]) / 2);
		}
		return mixedFloatArray;
	}
}