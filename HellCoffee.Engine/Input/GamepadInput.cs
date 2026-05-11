using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HellCoffee.Engine.Input;

public class GamepadInput
{
    private GamePadState _previous;
    private GamePadState _current;
    private float _vibrationTimer;

    public PlayerIndex PlayerIndex { get; }
    public bool IsConnected => _current.IsConnected;

    public Vector2 LeftStick => _current.ThumbSticks.Left;
    public Vector2 RightStick => _current.ThumbSticks.Right;
    public float LeftTrigger => _current.Triggers.Left;
    public float RightTrigger => _current.Triggers.Right;

    public GamepadInput(PlayerIndex index = PlayerIndex.One)
    {
        PlayerIndex = index;
    }

    public void Update(GameTime gameTime)
    {
        _previous = _current;
        _current = GamePad.GetState(PlayerIndex);

        if (_vibrationTimer > 0)
        {
            _vibrationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_vibrationTimer <= 0)
                GamePad.SetVibration(PlayerIndex, 0f, 0f);
        }
    }

    public bool IsDown(Buttons button) => _current.IsButtonDown(button);
    public bool IsUp(Buttons button) => _current.IsButtonUp(button);
    public bool JustPressed(Buttons button) => _current.IsButtonDown(button) && _previous.IsButtonUp(button);
    public bool JustReleased(Buttons button) => _current.IsButtonUp(button) && _previous.IsButtonDown(button);

    public void Vibrate(float leftMotor, float rightMotor, float duration)
    {
        GamePad.SetVibration(PlayerIndex, leftMotor, rightMotor);
        _vibrationTimer = duration;
    }

    public void StopVibration()
    {
        GamePad.SetVibration(PlayerIndex, 0f, 0f);
        _vibrationTimer = 0f;
    }
}
