using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class GenerateBubbleButtonComponent : MonoBehaviour
{
    public TMPro.TMP_InputField InputRadius;
    public TMPro.TMP_InputField InputDepth;
    public TMPro.TMP_InputField InputFrom;
    public TMPro.TMP_InputField InputTo;
    public TMPro.TMP_InputField InputStart;
    public TMPro.TMP_InputField InputEnd;
    public TMPro.TMP_InputField InputSteps;
    public TMPro.TMP_InputField InputTimeLeft;
    public TMPro.TMP_Dropdown InputMoving;
    public TMPro.TMP_Dropdown InputInterface;
    public Button button;

    public AudioSource audioSource;

    private PlayBubbleSoundSystem play_bubble_sound_system;
    private EntityManager em;
    private EntityArchetype BubbleArchtype;
    private int sampleRate = 48000;
    // Start is called before the first frame update
    void Start()
    {
        if (button == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated button! If this was intended please ignore.");
        else
            button.onClick.AddListener(OnSubmit);
        if (InputDepth == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputDepth! If this was intended please ignore.");
        if (InputRadius == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputRadius! If this was intended please ignore.");
        if (InputFrom == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputFrom! If this was intended please ignore.");
        if (InputTo == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputTo! If this was intended please ignore.");
        if (InputStart == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputStart! If this was intended please ignore.");
        if (InputEnd == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputEnd! If this was intended please ignore.");
        if (InputSteps == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputSteps! If this was intended please ignore.");
        if (InputTimeLeft == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputTimeLeft! If this was intended please ignore.");
        if (InputInterface == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputInterface! If this was intended please ignore.");
        if (InputMoving == null)
            Debug.LogError("GenerateBubbleButtonComponent::Failed to get assocoated InputMoving! If this was intended please ignore.");

        GetBubbleSystem();
    }
    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }
    private void GetBubbleSystem()
    {
        Debug.LogWarning("Getting Play Bubble System");
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        BubbleArchtype = em.CreateArchetype(typeof(DOTS_Bubble_Data), typeof(BubbleGenerationRequest));
        play_bubble_sound_system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PlayBubbleSoundSystem>();
        if (play_bubble_sound_system != null)
        {
            play_bubble_sound_system.audioSource = audioSource;
            play_bubble_sound_system.sampleRate = sampleRate;
        }
    }
    public void OnSubmit()
    {
        NativeArray<Entity> entities = new NativeArray<Entity>(1, Allocator.TempJob);
        em.CreateEntity(BubbleArchtype, entities);
        for (int j = 0; j < entities.Length; j++)
        {
            em.SetComponentData(entities[j], new DOTS_Bubble_Data((DOTS_Bubble_Data.InterfaceType)InputInterface.value,
                (DOTS_Bubble_Data.MovingType)InputMoving.value,float.Parse(InputRadius.text),
               float.Parse(InputDepth.text), int.Parse(InputFrom.text), int.Parse(InputTo.text),
               float.Parse(InputStart.text), float.Parse(InputEnd.text), int.Parse(InputSteps.text), float.Parse(InputTimeLeft.text),
               entities[j])
            );
        }
    }
    private DOTS_Bubble_Data.InterfaceType stringToIterfaceType(string a)
    {
        switch (a)
        {
            case "Fluid": return DOTS_Bubble_Data.InterfaceType.Fluid;
            case "Rigid": return DOTS_Bubble_Data.InterfaceType.Rigid;
            default: return DOTS_Bubble_Data.InterfaceType.None;
        }
    }
    private DOTS_Bubble_Data.MovingType StringToMovingType(string a)
    {
        switch (a)
        {
            case "Static": return DOTS_Bubble_Data.MovingType.Static;
            case "Rising": return DOTS_Bubble_Data.MovingType.Rising;
            default: return DOTS_Bubble_Data.MovingType.None;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (play_bubble_sound_system != null)
        {
           
        }
        else GetBubbleSystem();
    }
}
