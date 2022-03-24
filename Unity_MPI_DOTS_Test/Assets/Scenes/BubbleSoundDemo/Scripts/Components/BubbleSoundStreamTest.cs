using UnityEngine;
using System.Collections;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.Profiling;

public class BubbleSoundStreamTest : MonoBehaviour
{
	public Float_BubbleSoundDataCondensed.Init_Mode Data_Mode;
	public double start = 0;
	public double end = 1;
	[Tooltip("the radius of the bubble")]
	[Range(1, 16)]
	public float Radius = 1;
	[Tooltip("how deep the bubble is within the liquid (water)")]
	[Range(1, 16)]
	public float Depth = 1;
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
	private Float_BubbleSoundDataCondensed bsd;
	int audioPosition = 0;
	int currentStep = 0;
	System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
	Vector<double> lastValue = Float_BubbleSoundDataCondensed.DEFAULT_INITIAL_VALUE;

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
	void sStart()
    {
		watch = System.Diagnostics.Stopwatch.StartNew();
	}
	void sStopAndPrint(object a = null)
    {
		watch.Stop();
		Debug.Log($"{a.ToString()}: {watch.Elapsed.TotalMilliseconds} ms, {watch.Elapsed.Ticks} ticks");
    }
	public void PlayBubbleSound()
	{	
		sStart();
		bsd = new Float_BubbleSoundDataCondensed(RigidInterface ? Float_BubbleSoundDataCondensed.rigid_interface : Float_BubbleSoundDataCondensed.fluid_interface,
			StaticMoving ? Float_BubbleSoundDataCondensed.static_moving : Float_BubbleSoundDataCondensed.rising_moving, Radius, Depth,96000);
		
		bsd.Init(Data_Mode, true);
	//	Float_BubbleSoundDataCondensed.GenerateBubble(bsd, out float[] wave_data, out int channels, out int sampleRate);
		
		PlayBubbleSound(name, new float[bsd.formatted_data.Length], 1, 41000,true);
		sStopAndPrint("Total Time");
	}
	public void PlayBubbleSound(string name, float[] wave_data, int channels = 1, int sampleRate = 41000, bool stream = false)
	{
		clip = AudioClip.Create(name, wave_data.Length, channels, sampleRate, stream,OnAudioRead/*,OnAudioSetPosition*/);
	//	clip.SetData(wave_data, 0);
		PlayBubbleSound(clip);

	}
	public void PlayBubbleSound(AudioClip audioClip)
	{
		audioSource.clip = audioClip;
		audioSource.Play();
		Profiler.EndSample();
	}

	void OnAudioRead(float[] data)
	{

		int count = 0;
		while(count < data.Length)
		{
			data[count] = bsd.formatted_data[currentStep + count];
			count++;
		}
		if (currentStep + data.Length >= bsd.formatted_data.Length)
		{
			currentStep = 0;
			lastValue = Float_BubbleSoundDataCondensed.DEFAULT_INITIAL_VALUE;
		}
		else currentStep += data.Length;
		
	}

	void OnAudioSetPosition(int newPosition)
	{

	}
	

}

