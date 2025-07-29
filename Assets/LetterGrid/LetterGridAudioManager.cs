using UnityEngine;

public class LetterGridAudioManager : MonoBehaviour {
    public static LetterGridAudioManager Instance;

    public AudioSource bgMusicSource;      // Assign in Inspector
    public AudioSource sfxSource;          // Assign in Inspector

    public AudioClip bgMusicClip;          // Assign in Inspector
    public AudioClip clickClip;            // Assign in Inspector
    public AudioClip tileAddClip;          // Assign in Inspector

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Music persists between scenes
        }
        else {
            Destroy(gameObject);
        }
    }

    void OnEnable() {
        LetterGridGameAudioEvents.OnClick += PlayClick;
        LetterGridGameAudioEvents.OnTileAdded += PlayTileAdd;
    }

    void OnDisable() {
        LetterGridGameAudioEvents.OnClick -= PlayClick;
        LetterGridGameAudioEvents.OnTileAdded -= PlayTileAdd;
    }

    void Start() {
        PlayMusic();
    }

    public void PlayMusic() {
        if (bgMusicSource && bgMusicClip) {
            bgMusicSource.clip = bgMusicClip;
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }
    }

    public void PlayClick() {
        if (sfxSource && clickClip)
            sfxSource.PlayOneShot(clickClip);
    }

    public void PlayTileAdd() {
        if (sfxSource && tileAddClip)
            sfxSource.PlayOneShot(tileAddClip);
    }
}
