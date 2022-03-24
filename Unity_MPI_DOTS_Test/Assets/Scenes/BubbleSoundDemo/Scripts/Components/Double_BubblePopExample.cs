using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Double_BubblePopExample : MonoBehaviour
{
	[Tooltip("the radius of the bubble")]
	[Range(1,16)]
	public double Radius = 1;
	[Tooltip("how deep the bubble is within the liquid (water)")]
	[Range(1,16)]
	public double Depth = 1;
	[Tooltip("true = Rigid Interface, false = Fluid Interface")]
	public bool RigidInterface = false;
	[Tooltip("true = Static Moving, fasle = Rising Moving")]
	public bool StaticMoving = true;
	public bool PlayOnStart = true;
	[Tooltip("Set to true to save to a file")]
	public bool SaveToFile = false;
	public string SoundFileName = "pop";
	private AudioSource audioSource;
	private AudioClip clip;
	private Double_BubbleSoundData bsd;
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

		if (PlayOnStart)
		{
			PlayBubbleSound();
			PlayOnStart = false;
		}
	}
	public void PlayBubbleSound()
	{
		var watch = new System.Diagnostics.Stopwatch();

		bsd = new Double_BubbleSoundData(RigidInterface ? Double_BubbleSoundData.rigid_interface : Double_BubbleSoundData.fluid_interface,
			StaticMoving ? Double_BubbleSoundData.static_moving : Double_BubbleSoundData.rising_moving, Radius, Depth);
		
		watch.Start();
		Double_BubbleSoundData.GenerateBubble(bsd, out float[] wave_data, out int channels, out int sampleRate);
		
		watch.Stop();
		PlayBubbleSound(name,wave_data,channels,sampleRate,false);

		Debug.Log("Execution = "+watch.ElapsedMilliseconds+"ms");
	}
	public void PlayBubbleSound(string name,float[] wave_data,int channels,int sampleRate = 41000,bool stream = false)
	{
		clip = AudioClip.Create(name, wave_data.Length, channels, sampleRate, stream);
		clip.SetData(wave_data, 0);
		PlayBubbleSound(clip);
		
	}
	public void PlayBubbleSound(AudioClip audioClip)
	{
		if(audioClip != null && audioClip.loadState == AudioDataLoadState.Loaded)
		{
			audioSource.PlayOneShot(audioClip);
		}
	}
}
