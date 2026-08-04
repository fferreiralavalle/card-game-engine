using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AssetsManager : MonoBehaviour
{
    public static AssetsManager Instance;

    public string spritesBasePath = "Sprites";

    // Memory caches to avoid re-reading files from disk repeatedly
    private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
    private readonly Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        Instance = this;
    }

    public string GetAssetsSpritesFullPath()
    {
        string spritesPath = Path.Combine(Application.dataPath, "StreamingAssets", spritesBasePath);
        string fullPath = Path.GetFullPath(spritesPath);
        return fullPath;
    }

    #region --- IMAGE LOADING ---

    /// <summary>
    /// Loads a PNG/JPG image from StreamingAssets as a Sprite.
    /// Example path: "Cards/Fireball.png"
    /// </summary>
    public async Task<Sprite> GetSpriteAsync(string relativePath, bool useCache = true)
    {
        if (useCache && _spriteCache.TryGetValue(relativePath, out Sprite cachedSprite))
        {
            return cachedSprite;
        }
        string finalPath = Path.Combine(spritesBasePath, relativePath);
        Texture2D texture = await LoadTextureAsync(finalPath);
        if (texture == null)
        {
            Debug.LogError($"Could not find Sprite in {finalPath}");
            return null;
        }

        // Convert Texture2D into a UI/2D-ready Sprite
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        if (useCache)
        {
            _spriteCache[relativePath] = sprite;
        }

        return sprite;
    }

    /// <summary>
    /// Returns arts paths
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    public List<string> GetAllImages(string folder)
    {
        string finalPath = Path.Combine(Application.streamingAssetsPath, spritesBasePath, folder);
        List<string> files = new[] { "*.png", "*.jpg", "*.jpeg" }
        .SelectMany(pattern => Directory.GetFiles(finalPath, pattern, SearchOption.AllDirectories)).Select(path => Path.GetFullPath(path))
        .ToList();

        return files;
    }

    private async Task<Texture2D> LoadTextureAsync(string relativePath)
    {
        string fullPath = GetStreamingAssetsUrl(relativePath);

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[StreamingAssetsManager] Failed to load Image at '{fullPath}': {request.error}");
                return null;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (texture != null)
            {
                // --- CRISP GRAPHICS FIX ---

                // For Pixel Art / Icons: Use Point filter for crisp, sharp edges
                // For HD Illustrations: Use Bilinear filter without blur
                texture.filterMode = FilterMode.Point; // Change to FilterMode.Bilinear if not pixel art

                texture.wrapMode = TextureWrapMode.Clamp;

                // Re-apply texture settings to update GPU memory
                texture.Apply(false, true);
            }

            return texture;
        }
    }

    #endregion

    #region --- AUDIO LOADING ---

    /// <summary>
    /// Loads an audio file (WAV, MP3, OGG) from StreamingAssets as an AudioClip.
    /// Example path: "Audio/CardDraw.wav"
    /// </summary>
    public async Task<AudioClip> GetAudioClipAsync(string relativePath, AudioType audioType = AudioType.UNKNOWN, bool useCache = true)
    {
        if (useCache && _audioCache.TryGetValue(relativePath, out AudioClip cachedClip))
        {
            return cachedClip;
        }

        if (audioType == AudioType.UNKNOWN)
        {
            audioType = GetAudioTypeFromExtension(relativePath);
        }

        string fullPath = GetStreamingAssetsUrl(relativePath);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, audioType))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[StreamingAssetsManager] Failed to load Audio at '{relativePath}': {request.error}");
                return null;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = Path.GetFileNameWithoutExtension(relativePath);

            if (useCache)
            {
                _audioCache[relativePath] = clip;
            }

            return clip;
        }
    }

    #endregion

    #region --- HELPERS & CACHE MANAGEMENT ---

    /// <summary>
    /// Builds the correct file URL across all platforms (Windows, Standalone, Android, iOS, WebGL).
    /// </summary>
    private string GetStreamingAssetsUrl(string relativePath)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

        #if UNITY_ANDROID && !UNITY_EDITOR
            return fullPath;
        #else
        // System.Uri handles all file:/// formatting and slash direction automatically
        return new System.Uri(fullPath).AbsoluteUri;
        #endif
    }

    private AudioType GetAudioTypeFromExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        return ext switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN
        };
    }

    /// <summary>
    /// Clears memory caches when changing scenes or clearing unused assets.
    /// </summary>
    public void ClearCache()
    {
        _spriteCache.Clear();
        _audioCache.Clear();
    }

    #endregion
}
