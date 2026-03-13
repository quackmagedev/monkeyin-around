using Godot;

public class GrabHand
{
	public bool IsGrabbing { get; private set; }
	public bool WantsGrab { get; private set; }
	public Vector2? GrabPoint { get; private set; }

	private readonly RigidBody2D _body;
	private readonly Marker2D _shoulder;
	private PinJoint2D _joint;
	private StaticBody2D _anchor;

	public GrabHand(RigidBody2D body, Marker2D shoulder)
	{
		_body = body;
		_shoulder = shoulder;
	}

	public void Press() => WantsGrab = true;

	public void Release()
	{
		WantsGrab = false;
		if (!IsGrabbing) return;
		if (GodotObject.IsInstanceValid(_joint))
			_joint.QueueFree();
		if (GodotObject.IsInstanceValid(_anchor))
			_anchor.QueueFree();
		_joint = null;
		_anchor = null;
		GrabPoint = null;
		IsGrabbing = false;
	}

	public void TryGrab(Vector2 worldTarget)
	{
		if (IsGrabbing) return;

		var spaceState = _body.GetWorld2D().DirectSpaceState;
		var exclude = new Godot.Collections.Array<Rid> { _body.GetRid() };

		// Check if the hand is overlapping a grabbable surface
		var pointQuery = new PhysicsPointQueryParameters2D();
		pointQuery.Position = worldTarget;
		pointQuery.CollisionMask = 3;
		pointQuery.Exclude = exclude;

		var pointResults = spaceState.IntersectPoint(pointQuery);
		if (pointResults.Count == 0) return;

		// Cast back toward the player to find the surface contact point,
		// so the joint sits on the platform face rather than inside it
		var rayQuery = PhysicsRayQueryParameters2D.Create(worldTarget, _body.GlobalPosition, collisionMask: 3);
		rayQuery.Exclude = exclude;
		var rayResult = spaceState.IntersectRay(rayQuery);

		Vector2 jointPos = rayResult.Count > 0 ? (Vector2)rayResult["position"] : worldTarget;

		GrabPoint = jointPos;

		// Create an invisible anchor at the grab point instead of pinning
		// directly to the platform — this avoids the joint disabling
		// collision between the player and the platform
		_anchor = new StaticBody2D();
		_anchor.CollisionLayer = 0;
		_anchor.CollisionMask = 0;
		_anchor.GlobalPosition = jointPos;
		_body.GetParent().AddChild(_anchor);

		_joint = new PinJoint2D();
		_joint.GlobalPosition = jointPos;
		_body.GetParent().AddChild(_joint);

		_joint.NodeA = _joint.GetPathTo(_anchor);
		_joint.NodeB = _joint.GetPathTo(_body);
		_joint.Softness = 0f;

		IsGrabbing = true;
		(_body as Player)?.PlayGrabSound();
	}

}
