using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DataHolder : MonoBehaviour
{
	public string dataString;
	public List<float> values = new List<float>();
	public List<byte> bytes;
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
	public void AddValue(float v) { values.Add(v); }
	public void AddValue(double v) { values.Add((float)v); }
	public void AddByte(byte b) { bytes.Add(b); }
	public void SetBytes(byte b) { bytes.Add(b); }
	public void SetData(string a)
	{
		dataString = a;
	}
	public void SaveWav()
	{
		Debug.Log("bytes = "+bytes.ToString());


	/*	Debug.Log("GET TRIGGERED! " + dataString);
		float[] waveData = values.ToArray();
		WaveFormat waveFormat = new WaveFormat(waveData.Length - 1, 1);
		using (WaveFileWriter writer = new WaveFileWriter(Application.dataPath + "/Wav_Outputs/test.wav", waveFormat))
		{
			writer.WriteSamples(waveData, 0, waveData.Length);
		}
		Debug.Log("saved file to" + Application.dataPath + "/Wav_Outputs/text.wav");
		*/
		
	}
}
