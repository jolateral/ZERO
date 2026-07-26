using UnityEngine;
using AK.Wwise;

public class AudioManager : MonoBehaviour
{
    [Header("Music and Ambience Events")]
        [SerializeField] private AK.Wwise.Event playMusic;
    
    [Header("Scene Objects")]
        [SerializeField] private GameObject Listener;

    void Start()
    {
        // SETUP //
        AkSoundEngine.SetState("GameState", "Title");
        AkSoundEngine.SetState("CompletePuzzleA", "False");
        AkSoundEngine.SetState("CompletePuzzleB", "False");
        AkSoundEngine.SetState("CompletePuzzleC", "False");
        AkSoundEngine.SetState("CompletePuzzleD", "False");
        AkSoundEngine.SetState("CompletePuzzleE", "False");
        AkSoundEngine.SetState("PuzzleView", "False");

        playMusic.Post(Listener);
        //Debug.Log("Menu Music is playing");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameMusic()
    {
        AkSoundEngine.SetState("GameState", "Puzzle");
        //Debug.Log("Game Music is playing");
    }

    public void PuzzleView()
    {
        AkSoundEngine.SetState("PuzzleView", "True");
    }

    public void HallwayView()
    {
        AkSoundEngine.SetState("PuzzleView", "False");
    }

    public void PuzzleCompleted(int remainingCount)
    {
        if (remainingCount == 4) AkSoundEngine.SetState("CompletePuzzleA", "True");
        if (remainingCount == 3) AkSoundEngine.SetState("CompletePuzzleB", "True");
        if (remainingCount == 2) AkSoundEngine.SetState("CompletePuzzleC", "True");
        if (remainingCount == 1) AkSoundEngine.SetState("CompletePuzzleD", "True");
        if (remainingCount == 0) AkSoundEngine.SetState("CompletePuzzleE", "True");
    }
}
