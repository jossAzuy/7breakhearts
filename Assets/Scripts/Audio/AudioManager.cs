using UnityEngine;
using System.Collections.Generic;

// AudioManager ULTRA SIMPLE (AudioSources manuales)
// - Asignas tú los AudioSource en el inspector (por ejemplo SFX1, SFX2, SFX3…)
// - El manager SOLO los reutiliza: no crea ni destruye objetos.
// - Métodos: Play2D, Play3D, PlayCollectible.
// - Collectibles: tabla opcional (CollectibleSO -> AudioClip) usada por Collectible.cs
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Basico")] [Range(0f,1f)] public float masterVolume = 1f;
    [Tooltip("AudioSources que vas a reutilizar (asignar manualmente)")] public List<AudioSource> audioSources = new();

    [Header("Collectibles (Opcional)")] public List<CollectibleEntry> collectibleClips = new();

    [System.Serializable]
    public class CollectibleEntry
    {
        public CollectibleSO collectible;
        public AudioClip clip;
    }

    private Dictionary<CollectibleSO, AudioClip> _collectibleMap;
    private int _nextIndex;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        BuildCollectibleMap();
    }

    private void BuildCollectibleMap()
    {
        _collectibleMap = new Dictionary<CollectibleSO, AudioClip>();
        foreach (var e in collectibleClips)
        {
            if (e == null || e.collectible == null || e.clip == null) continue;
            if (!_collectibleMap.ContainsKey(e.collectible))
                _collectibleMap.Add(e.collectible, e.clip);
        }
    }

    private AudioSource GetFree()
    {
        if (audioSources == null || audioSources.Count == 0) return null;
        for (int i = 0; i < audioSources.Count; i++)
        {
            int idx = (_nextIndex + i) % audioSources.Count;
            var candidate = audioSources[idx];
            if (candidate == null) continue;
            if (!candidate.isPlaying)
            {
                _nextIndex = (idx + 1) % audioSources.Count;
                return candidate;
            }
        }
        // Si todos están ocupados, recicla el siguiente válido
        for (int i = 0; i < audioSources.Count; i++)
        {
            int idx = (_nextIndex + i) % audioSources.Count;
            if (audioSources[idx] != null)
            {
                _nextIndex = (idx + 1) % audioSources.Count;
                return audioSources[idx];
            }
        }
        return null;
    }

    public void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        var src = GetFree();
        if (src == null) return; // no hay fuentes asignadas
        src.Stop();
        src.clip = clip;
        src.volume = masterVolume * volume;
        src.pitch = 1f;
        src.loop = false;
        src.spatialBlend = 0f;
        src.Play();
    }

    public void Play3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        var src = GetFree();
        if (src == null) return;
        src.Stop();
        src.clip = clip;
        src.volume = masterVolume * volume;
        src.pitch = 1f;
        src.loop = false;
        src.spatialBlend = 1f;
        src.transform.position = position;
        src.minDistance = 1f;
        src.maxDistance = 20f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.Play();
    }

    /// <summary>
    /// Reproduce un clip 3D y hace que el AudioSource siga al transform objetivo parentándolo.
    /// - Usa una fuente del pool. Si todas están ocupadas, recicla como en GetFree().
    /// - Parentar significa que si el enemigo se destruye, el sonido se corta (comportamiento deseado para voces cortas).
    /// - Si prefieres que el sonido continúe, clone/instancia otro GO o implementa una lista de seguimiento sin parenting.
    /// </summary>
    public AudioSource Play3DFollow(AudioClip clip, Transform follow, float volume = 1f, bool resetParentAfter = false)
    {
        if (clip == null || follow == null) return null;
        var src = GetFree();
        if (src == null) return null;
        src.Stop();
        // Parentar manteniendo posición local cero
        src.transform.SetParent(follow, worldPositionStays: false);
        src.transform.localPosition = Vector3.zero;
        src.clip = clip;
        src.volume = masterVolume * volume;
        src.pitch = 1f;
        src.loop = false;
        src.spatialBlend = 1f;
        src.minDistance = 1f;
        src.maxDistance = 20f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.dopplerLevel = 0f;
        src.Play();
        if (resetParentAfter) { Instance.StartCoroutine(UnparentAfter(src, clip.length)); }
        return src;
    }

    private System.Collections.IEnumerator UnparentAfter(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (src != null)
        {
            // Sólo desparentar si sigue teniendo padre (para evitar romper reasignaciones posteriores).
            if (src.transform.parent != null)
            {
                src.transform.SetParent(transform, worldPositionStays: true);
            }
        }
    }

    public void PlayCollectible(CollectibleSO collectible, Vector3 worldPos)
    {
        if (collectible == null || _collectibleMap == null) return;
        if (_collectibleMap.TryGetValue(collectible, out var clip) && clip != null)
        {
            // Por simplicidad: reproducimos 2D. Cambiar a Play3D si quisieras espacial.
            Play2D(clip, 1f);
        }
    }

    // Si editas la lista en runtime y quieres refrescar:
    public void RebuildCollectibleTable() => BuildCollectibleMap();
}
