using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour {

    private static AudioManager _instance;
    public static AudioManager instance { get => _instance; }

    [Header("Musics")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    [Header("AudioEffeccts")]
    [SerializeField] private AudioClip shotSound;
    [SerializeField] private AudioClip explosionSound, motherShipSound, playerDeathSound;
    private AudioSource source, currentSceneSource;
    public void Awake() {
        if(_instance == null) {
            _instance = this;
            DontDestroyOnLoad(this);
            source = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else if(_instance != this) {
            Destroy(gameObject);
        }
    }

    public static void PlayShotSound() {
        instance.source.PlayOneShot(instance.shotSound); 
    }

    public static void EnemyExploding() {
        instance.source.PlayOneShot(instance.explosionSound);
    }

    public static void PlayerDeath() {
        instance.source.PlayOneShot(instance.playerDeathSound);
    }

    public static void ChangeMusicSpeed(float speed) {
        instance.MusicSpeed(speed);
    }

    private void MusicSpeed(float speed) {
        if(currentSceneSource.pitch == speed) return;
        currentSceneSource.pitch = speed;
        currentSceneSource.outputAudioMixerGroup.audioMixer.SetFloat("Pitch",1f / speed);
    }

    void OnSceneLoaded(Scene scene,LoadSceneMode mode) {
        currentSceneSource = Camera.main.GetComponent<AudioSource>();
        if(SceneManager.GetActiveScene().name == "MainMenu") {
            currentSceneSource.clip = menuMusic;
        } else {
            currentSceneSource.clip = gameMusic;
        }
        MusicSpeed(1);
        currentSceneSource.loop = true;
        currentSceneSource.Play();
    }
}
