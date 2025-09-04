using UnityEngine;

public enum BGMType {Stage, Boss}

public class BGMController : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] bgmClips;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ChangeBGM(BGMType index)
    {
        audioSource.Stop();

        audioSource.clip = bgmClips[(int)index];

        audioSource.Play();
    }
}
