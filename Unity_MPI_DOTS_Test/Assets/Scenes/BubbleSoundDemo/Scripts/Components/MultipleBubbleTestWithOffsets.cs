
using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.Profiling;
using System;
using System.Collections.Generic;
using NAudio.Wave;
using System.IO;

public class MultipleBubbleTestWithOffsets : MonoBehaviour
{

	public Float_BubbleSoundDataCondensedDynamic[] bubbles;
	public int sampleRate = 41000;
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
	private string path;
	public void PlayBubbleSound()
	{
		watch.Start();
		for (int i = 0; i < bubbles.Length; i++)
		{
			bubbles[i] = new Float_BubbleSoundDataCondensedDynamic(bubbles[i].interfacetype, bubbles[i].movingtype, bubbles[i].radius, bubbles[i].depth,bubbles[i].from,bubbles[i].to,bubbles[i].start,bubbles[i].end, bubbles[i].steps);
			bubbles[i].Init(Float_BubbleSoundDataCondensedDynamic.Init_Mode.Fast_Formatted, true,true);
			if (SaveToFile)
			{
				path = Application.dataPath + "/Wave_Outputs/";
				WaveFormat waveFormat = new WaveFormat(bubbles[i].steps, 1);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				//	if(!File.Exists(path + SoundFileName + ".wav"))
				//		File.Create(path + SoundFileName + ".wav");
				
				using (WaveFileWriter writer = new WaveFileWriter(path+SoundFileName+".wav", waveFormat))
				{
					writer.WriteSamples(bubbles[i].formatted_data, 0, bubbles[i].formatted_data.Length);
				}
				Debug.Log("saved file to" + Application.dataPath + "/Wav_Outputs/text"+i+".wav");
			}
		}
		watch.Stop();
		Debug.Log("AAAAAA "+watch.ElapsedMilliseconds+"ms");
		watch.Reset();
		PlayBubbleSound(name, bubbles[0].formatted_data, 1, sampleRate, true);
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
		Debug.Log($"data length: {data.Length}, currentStep {currentStep}," +
			$" max {bubbles[0].formatted_data.Length}");
		int total = bubbles[0].formatted_data.Length - currentStep - data.Length;
		if (total > data.Length) total = data.Length;
		else if (total <= 0 && currentStep < bubbles[0].formatted_data.Length)
			total = data.Length + total;
        
		//if (currentStep + data.Length >= bubbles[0].formatted_data.Length)
		if(total <= 0)
		{
			Debug.Log("exceeded data lnegth, no more!");
			for (int i = 0; i < data.Length; i++)
				data[i] = 0;
			return;
		}
		else
		{
			//	watch.Start();
			for (int i = 0; i < total; i++)
			{
				float value = 0;
				for (int j = 0; j < bubbles.Length; j++)
					value += bubbles[j].formatted_data[currentStep + i];
				data[i] = ClampToValidRange(value);

			}/*
		if (currentStep + data.Length >= bubbles[0].formatted_data.Length)
		{
			Debug.Log("Exceeded data length");
			currentStep = bubbles[0].formatted_data.Length;
		}
		else*/
			currentStep += data.Length;
		}
	//	watch.Stop();
	//	Debug.Log("Combining " + bubbles.Length + " bubbles took " + watch.ElapsedMilliseconds + "ms");
	//	watch.Reset();

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
