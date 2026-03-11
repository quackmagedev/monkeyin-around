using Godot;

public partial class Level : Node2D
{
    private Player _player;
    private Camera2D _camera;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _camera = GetNode<Camera2D>("Camera2D");

        var music = GetNode<AudioStreamPlayer>("Music");
        ((AudioStreamMP3)music.Stream).Loop = true;
    }

    public override void _Process(double delta)
    {
        _camera.GlobalPosition = _player.GlobalPosition;
    }
}
