using Godot;

public partial class Player : RigidBody2D
{
	private GrabHand _leftHand;
	private GrabHand _rightHand;
	private Vector2 _aimPos;

	private const float MoveForce = 5000f;
	private const float ClimbForce = 25000f;
	private const float AimRadius = 120f;
	private const float HandSpread = 18f;

	public override void _Ready()
	{
		_leftHand = new GrabHand(this, GetNode<Marker2D>("LeftHandMarker"));
		_rightHand = new GrabHand(this, GetNode<Marker2D>("RightHandMarker"));
		_aimPos = GlobalPosition + Vector2.Up * 150f;
	}

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

		QueueRedraw();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("grab_left"))
			_leftHand.TryGrab(ArmTarget(-1));
		if (@event.IsActionReleased("grab_left"))
			_leftHand.Release();

		if (@event.IsActionPressed("grab_right"))
			_rightHand.TryGrab(ArmTarget(1));
		if (@event.IsActionReleased("grab_right"))
			_rightHand.Release();
	}

	public override void _Draw()
	{
		var armBrown = new Color(0.651f, 0.443f, 0.243f); // #a6713e
		var handTan  = new Color(0.910f, 0.729f, 0.569f); // #e8ba91

		// Left arm
		Vector2 leftTip = _leftHand.IsGrabbing && _leftHand.GrabPoint.HasValue
			? ToLocal(_leftHand.GrabPoint.Value)
			: ToLocal(ArmTarget(-1));
		DrawLine(new Vector2(-15, 10), leftTip, armBrown, 7f);
		DrawCircle(leftTip, _leftHand.IsGrabbing ? 5f : 10f, handTan);

		// Left label — counter-rotate so it stays upright
		DrawSetTransform(leftTip, -Rotation, Vector2.One);
		DrawString(ThemeDB.FallbackFont, new Vector2(-30, -14), "Left Click", HorizontalAlignment.Left, -1, 10, Colors.White);
		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);

		// Right arm
		Vector2 rightTip = _rightHand.IsGrabbing && _rightHand.GrabPoint.HasValue
			? ToLocal(_rightHand.GrabPoint.Value)
			: ToLocal(ArmTarget(1));
		DrawLine(new Vector2(15, 10), rightTip, armBrown, 7f);
		DrawCircle(rightTip, _rightHand.IsGrabbing ? 5f : 10f, handTan);

		// Right label — counter-rotate so it stays upright
		DrawSetTransform(rightTip, -Rotation, Vector2.One);
		DrawString(ThemeDB.FallbackFont, new Vector2(5, -14), "Right Click", HorizontalAlignment.Left, -1, 10, Colors.White);
		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}
}
