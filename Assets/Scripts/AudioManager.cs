using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource[] _orbAudios;
    [SerializeField, Range(0,1)] float _volumen = 1f; 
    void OnEnable()
    {
        Orb.OnSpawn += () => PlayOrbSound(0);
        Orb.OnDespawn += () => PlayOrbSound(1);
        Orb.OnOrbitEnter += () => PlayOrbSound(2);
        Orb.OnOrbitExit += () => PlayOrbSound(3);
    }
    void OnDisable()
    {
        Orb.OnSpawn += () => PlayOrbSound(0);
        Orb.OnDespawn += () => PlayOrbSound(1);
        Orb.OnOrbitEnter += () => PlayOrbSound(2);
        Orb.OnOrbitExit += () => PlayOrbSound(3);
    }
    void PlayOrbSound(int index)
    {
        AudioSource desiredAudio = _orbAudios[index];
        if(desiredAudio.isPlaying) desiredAudio.PlayOneShot(desiredAudio.clip, desiredAudio.volume);
        else desiredAudio.Play();
    }
}
