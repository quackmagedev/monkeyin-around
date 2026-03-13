using Godot;

public partial class Player : RigidBody2D
{
	private GrabHand _leftHand;
	private GrabHand _rightHand;
	private Vector2 _aimPos;
	private AudioStreamPlayer _grabSound;

	private const float MoveForce = 5000f;
	private const float ClimbForce = 25000f;
	private const float AimRadius = 120f;
	private const float HandSpread = 18f;

	public override void _Ready()
	{
		_leftHand = new GrabHand(this, GetNode<Marker2D>("LeftHandMarker"));
		_rightHand = new GrabHand(this, GetNode<Marker2D>("RightHandMarker"));
		_aimPos = GlobalPosition + Vector2.Up * 150f;

		_grabSound = new AudioStreamPlayer();
		_grabSound.Stream = GD.Load<AudioStream>("res://assets/audio/grab.mp3");
		AddChild(_grabSound);
	}

	public void PlayGrabSound() => _grabSound.Play();

	// Returns a slightly spread target for each hand.
	// side: -1 = left, +1 = right
	private Vector2 ArmTarget(float side)
	{
		Vector2 dir = (_aimPos - GlobalPosition);
		if (dir.LengthSquared() < 1f) dir = Vector2.Up;
		Vector2 perp = new Vector2(-dir.Normalized().Y, dir.Normalized().X);
		return _aimPos + perp * side * HandSpread;
	}

	public override void _PhysicsProcess(double delta)
	{
		// WASD applies force to body — stronger when hanging from a grab
		Vector2 moveDir = Input.GetVector("aim_left", "aim_right", "aim_up", "aim_down");
		if (moveDir != Vector2.Zero)
		{
			bool hanging = _leftHand.IsGrabbing || _rightHand.IsGrabbing;
			ApplyCentralForce(moveDir * (hanging ? ClimbForce : MoveForce));
		}

		// Aim cursor always follows mouse
		_aimPos = GetGlobalMousePosition();

		// Clamp aim to max radius from player
		Vector2 offset = _aimPos - GlobalPosition;
		if (offset.Length() > AimRadius)
			_aimPos = GlobalPosition + offset.Normalized() * AimRadius;

		if (_leftHand.WantsGrab)  _leftHand.TryGrab(ArmTarget(-1));
		if (_rightHand.WantsGrab) _rightHand.TryGrab(ArmTarget(1));

		QueueRedraw();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("grab_left"))  _leftHand.Press();
		if (@event.IsActionReleased("grab_left")) _leftHand.Release();

		if (@event.IsActionPressed("grab_right"))  _rightHand.Press();
		if (@event.IsActionReleased("grab_right")) _rightHand.Release();
	}

	// Draws a bezier-curved arm. bowSign=+1 bows left of the direction, -1 bows right.
	private void DrawElasticArm(Vector2 from, Vector2 to, Color color, float bowSign = 1f)
	{
		Vector2 dir = to - from;
		float len = dir.Length();
		if (len < 1f) return;

		// Control point bows perpendicular to the arm; more bow when stretched
		Vector2 perp = new Vector2(-dir.Y, dir.X) / len;
		Vector2 ctrl = (from + to) * 0.5f + perp * bowSign * Mathf.Min(len * 0.25f, 18f);

		const int Steps = 12;
		var pts = new Vector2[Steps + 1];
		for (int i = 0; i <= Steps; i++)
		{
			float t = i / (float)Steps;
			float u = 1f - t;
			pts[i] = u * u * from + 2f * u * t * ctrl + t * t * to;
		}
		DrawPolyline(pts, color, 7f, false);
	}

	public override void _Draw()
	{
		var armBrown = new Color(0.600f, 0.400f, 0.200f); // #996633
		var handTan  = new Color(0.910f, 0.729f, 0.569f); // #e8ba91
		var font     = ThemeDB.FallbackFont;

		// Left arm
		Vector2 leftTip = _leftHand.IsGrabbing && _leftHand.GrabPoint.HasValue
			? ToLocal(_leftHand.GrabPoint.Value)
			: ToLocal(ArmTarget(-1));
		DrawElasticArm(new Vector2(-13, 10), leftTip, armBrown, -1f);
		float leftR = (_leftHand.IsGrabbing || _leftHand.WantsGrab) ? 5f : 10f;
		DrawCircle(leftTip, leftR, handTan);

		// "L" centered in hand — counter-rotate so it stays upright
		DrawSetTransform(leftTip, -Rotation, Vector2.One);
		DrawString(font, new Vector2(-3f, 4f), "L", HorizontalAlignment.Center, -1, 9, Colors.White);
		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);

		// Right arm
		Vector2 rightTip = _rightHand.IsGrabbing && _rightHand.GrabPoint.HasValue
			? ToLocal(_rightHand.GrabPoint.Value)
			: ToLocal(ArmTarget(1));
		DrawElasticArm(new Vector2(13, 10), rightTip, armBrown);
		float rightR = (_rightHand.IsGrabbing || _rightHand.WantsGrab) ? 5f : 10f;
		DrawCircle(rightTip, rightR, handTan);

		// "R" centered in hand — counter-rotate so it stays upright
		DrawSetTransform(rightTip, -Rotation, Vector2.One);
		DrawString(font, new Vector2(-3f, 4f), "R", HorizontalAlignment.Center, -1, 9, Colors.White);
		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}
}
