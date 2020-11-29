using UnityEngine;

public class AudioManager : MonoBehaviour
{
  public static AudioManager instance = null;

  public AudioSource musicSource;
  public AudioSource fxSource;

  public AudioClip headClip;
  public AudioClip ballsClip;
  public AudioClip limbClip;
  public AudioClip celebrateClip;

  public float pitchRandomness = 0.2f;
  private float originalFXPitch;

  private void Awake()
  {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
    DontDestroyOnLoad(gameObject);
  }

  private void Start()
  {
    originalFXPitch = fxSource.pitch;
  }

  // Head
  public static void PlayHead()
  {
    if (instance == null) {
      return;
    }
    instance.InstancePlayHead();
  }

  private void InstancePlayHead()
  {
    PlayFxClip(headClip);
  }

  // Balls
  public static void PlayBalls()
  {
    if (instance == null) {
      return;
    }
    instance.InstancePlayBalls();
  }

  private void InstancePlayBalls()
  {
    PlayFxClip(ballsClip);
  }

  // Limb
  public static void PlayLimb()
  {
    if (instance == null) {
      return;
    }
    instance.InstancePlayLimb();
  }

  private void InstancePlayLimb()
  {
    PlayFxClip(limbClip);
  }

  // Celebrate
  public static void PlayCelebrate()
  {
    if (instance == null) {
      return;
    }
    instance.InstancePlayCelebrate();
  }

  private void InstancePlayCelebrate()
  {
    PlayFxClip(celebrateClip);
  }

  private void PlayFxClip(AudioClip clip)
  {
    fxSource.pitch = originalFXPitch + Random.Range(-pitchRandomness, pitchRandomness);
    fxSource.PlayOneShot(clip);
  }
}