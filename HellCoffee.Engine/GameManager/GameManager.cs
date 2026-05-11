using HellCoffee.Engine;
using HellCoffee.Engine.Scene;

namespace HellCoffee.Engine.GameManager;

/// <summary>
/// Ponto central de controle do jogo.
/// Gerencia estado global, configurações, saves e transições de cena.
/// Singleton acessível de qualquer lugar.
/// </summary>
public class GameManager
{
    public static GameManager Instance { get; private set; }

    public GameSettings Settings { get; private set; }
    public int Score { get; set; }
    public int Lives { get; set; }
    public int CurrentLevel { get; set; }
    public bool IsPaused { get; private set; }

    public event Action OnPaused;
    public event Action OnResumed;
    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;

    public static void Create()
    {
        Instance = new GameManager();
        Instance.Settings = GameSettings.Load();
    }

    public void Pause()
    {
        IsPaused = true;
        OnPaused?.Invoke();
    }

    public void Resume()
    {
        IsPaused = false;
        OnResumed?.Invoke();
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void TriggerGameOver() => OnGameOver?.Invoke();

    public void GoToScene(Scene.Scene scene)
        => Core.Scenes.ChangeScene(scene);

    public void SaveGame(string slot = "default")
    {
        var data = new DefaultSaveData
        {
            Score = Score,
            Lives = Lives,
            CurrentLevel = CurrentLevel
        };
        SaveSystem.Save(slot, data);
        Settings.Save();
    }

    public void LoadGame(string slot = "default")
    {
        var data = SaveSystem.Load<DefaultSaveData>(slot);
        Score = data.Score;
        Lives = data.Lives;
        CurrentLevel = data.CurrentLevel;
    }

    public void Reset()
    {
        Score = 0;
        Lives = 3;
        CurrentLevel = 1;
        IsPaused = false;
    }
}

/// <summary>Dados de save padrão. Estenda ou substitua por sua própria classe.</summary>
public class DefaultSaveData
{
    public int Score { get; set; }
    public int Lives { get; set; }
    public int CurrentLevel { get; set; }
}
