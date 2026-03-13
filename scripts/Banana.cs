using Godot;

public partial class Banana : Area2D
{
	[Signal]
	public delegate void CollectedEventHandler();

	public override void _Ready()
	{
		AddToGroup("bananas");
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			Monitoring = false;
			EmitSignal(SignalName.Collected);
			PlayCollectAnimation();
		}
	}

	private void PlayCollectAnimation()
	{
		var eatSound = new AudioStreamPlayer();
		eatSound.Stream = GD.Load<AudioStream>("res://assets/audio/eat.mp3");
		GetParent().AddChild(eatSound);
		eatSound.Play();
		eatSound.Finished += eatSound.QueueFree;

		var sprite = GetNode<Sprite2D>("Sprite2D");
		var tween = CreateTween();

		// Shake
		tween.TweenProperty(sprite, "rotation", 0.45f, 0.05f);
		tween.TweenProperty(sprite, "rotation", -0.45f, 0.05f);
		tween.TweenProperty(sprite, "rotation", 0.25f, 0.04f);
		tween.TweenProperty(sprite, "rotation", 0f, 0.04f);

		// Shrink with a snappy ease-in
		tween.TweenProperty(sprite, "scale", Vector2.Zero, 0.13f)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Back);

		tween.TweenCallback(Callable.From(QueueFree));
	}
}