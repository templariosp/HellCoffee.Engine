using Microsoft.Xna.Framework;

namespace HellCoffee.Engine.Input;

public class InputManager
{
    public KeyboardInput Keyboard { get; } = new();
    public MouseInput Mouse { get; } = new();
    public GamepadInput Gamepad { get; } = new();
    public GamepadInput[] Gamepads { get; }

    private readonly Dictionary<string, InputAction> _actions = new();

    public InputManager()
    {
        Gamepads = new[]
        {
            Gamepad,
            new GamepadInput(Microsoft.Xna.Framework.PlayerIndex.Two),
            new GamepadInput(Microsoft.Xna.Framework.PlayerIndex.Three),
            new GamepadInput(Microsoft.Xna.Framework.PlayerIndex.Four),
        };
    }

    public void Update(GameTime gameTime)
    {
        Keyboard.Update();
        Mouse.Update();
        foreach (var gp in Gamepads)
            gp.Update(gameTime);
    }

    /// <summary>Registra uma ação nomeada para uso em código de gameplay.</summary>
    public InputAction RegisterAction(string name)
    {
        var action = new InputAction(name);
        _actions[name] = action;
        return action;
    }

    public InputAction GetAction(string name)
    {
        if (!_actions.TryGetValue(name, out var action))
            throw new KeyNotFoundException($"InputAction '{name}' not registered.");
        return action;
    }

    public bool ActionDown(string name) => GetAction(name).IsDown(this);
    public bool ActionJustPressed(string name) => GetAction(name).JustPressed(this);
    public bool ActionJustReleased(string name) => GetAction(name).JustReleased(this);
}
