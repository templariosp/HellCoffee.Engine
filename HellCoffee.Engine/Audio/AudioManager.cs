using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace HellCoffee.Engine.Audio;

public class AudioManager
{
    private readonly List<SoundEffectInstance> _activeSounds = new();
    private bool _muted;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 1f;

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Math.Clamp(value, 0f, 1f);
            if (!_muted) MediaPlayer.Volume = _musicVolume;
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Math.Clamp(value, 0f, 1f);
            SoundEffect.MasterVolume = _muted ? 0f : _sfxVolume;
        }
    }

    public bool IsMuted => _muted;

    public void PlayMusic(Song song, bool repeat = true)
    {
        MediaPlayer.IsRepeating = repeat;
        MediaPlayer.Volume = _muted ? 0f : _musicVolume;
        MediaPlayer.Play(song);
    }

    public void PauseMusic() => MediaPlayer.Pause();
    public void ResumeMusic() => MediaPlayer.Resume();
    public void StopMusic() => MediaPlayer.Stop();

    public SoundEffectInstance PlaySound(SoundEffect sfx, float volume = 1f, float pitch = 0f, float pan = 0f, bool loop = false)
    {
        if (_muted) return null;
        var instance = sfx.CreateInstance();
        instance.Volume = volume * _sfxVolume;
        instance.Pitch = pitch;
        instance.Pan = pan;
        instance.IsLooped = loop;
        instance.Play();
        _activeSounds.Add(instance);
        return instance;
    }

    public void StopAllSounds()
    {
        foreach (var s in _activeSounds)
            s.Stop();
        _activeSounds.Clear();
    }

    public void Mute()
    {
        _muted = true;
        MediaPlayer.Volume = 0f;
        SoundEffect.MasterVolume = 0f;
    }

    public void Unmute()
    {
        _muted = false;
        MediaPlayer.Volume = _musicVolume;
        SoundEffect.MasterVolume = _sfxVolume;
    }

    public void ToggleMute()
    {
        if (_muted) Unmute(); else Mute();
    }

    public void Update(GameTime gameTime)
    {
        for (int i = _activeSounds.Count - 1; i >= 0; i--)
        {
            if (_activeSounds[i].State == SoundState.Stopped)
            {
                _activeSounds[i].Dispose();
                _activeSounds.RemoveAt(i);
            }
        }
    }
}
