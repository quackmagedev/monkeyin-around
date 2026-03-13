using Godot;

public partial class Level : Node2D
{
    private Player _player;
    private Camera2D _camera;
    private CanvasLayer _bgLayer;
    private CanvasLayer _winLayer;
    private Label _bananaLabel;
    private int _bananasTotal;
    private int _bananasLeft;
    private AudioStreamPlayer _winSound;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _camera = GetNode<Camera2D>("Camera2D");
        _bgLayer = GetNode<CanvasLayer>("BgLayer");
        _winLayer = GetNode<CanvasLayer>("WinLayer");
        _bananaLabel = GetNode<Label>("HudLayer/BananaLabel");

        _winSound = new AudioStreamPlayer();
        _winSound.Stream = GD.Load<AudioStream>("res://assets/audio/monkey.mp3");
        AddChild(_winSound);

        var music = GetNode<AudioStreamPlayer>("Music");
        ((AudioStreamMP3)music.Stream).Loop = true;

        var bananas = GetTree().GetNodesInGroup("bananas");
        _bananasTotal = bananas.Count;
        _bananasLeft = _bananasTotal;
        UpdateLabel();

        foreach (var node in bananas)
        {
            if (node is Banana b)
                b.Collected += OnBananaCollected;
        }
    }

    private void OnBananaCollected()
    {
        _bananasLeft--;
        UpdateLabel();
        if (_bananasLeft <= 0)
        {
            _player.Freeze = true;
            _player.ProcessMode = Node.ProcessModeEnum.Disabled;
            _winLayer.Visible = true;
            _winSound.Play();
        }
    }

    private void UpdateLabel()
    {
        int collected = _bananasTotal - _bananasLeft;
        _bananaLabel.Text = $"{collected} / {_bananasTotal} bananas";
    }

    public override void _Process(double delta)
    {
        _camera.GlobalPosition = _player.GlobalPosition;
        _bgLayer.Offset = (_camera.GlobalPosition - new Vector2(2215f, -750f)) * 0.02f;
    }
}