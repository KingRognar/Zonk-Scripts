using UnityEngine;
using UnityEngine.Audio;

public class SoundManager_Scr : MonoBehaviour
{
    public static SoundManager_Scr instance;

    [SerializeField] private GameObject singleSfx_pref;

    [SerializeField] [Range(0, 1)] private float sfxPitchVariance = 0.1f;


    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void PlaySingleSound(AudioResource audioRes,float volume, Vector3 position)
    {
        GameObject newSfx = Instantiate(singleSfx_pref, position, Quaternion.identity);
        AudioSource newSfx_AS = newSfx.GetComponent<AudioSource>();
        newSfx_AS.resource = audioRes;
        newSfx_AS.volume = volume;
        newSfx_AS.pitch = 1 + Random.Range(-sfxPitchVariance, sfxPitchVariance);
        newSfx_AS.Play();
        Destroy(newSfx, newSfx_AS.clip.length);
    }
}
