using Godot;

public class GrabHand
{
    public bool IsGrabbing { get; private set; }
    public Vector2? GrabPoint { get; private set; }

    private readonly RigidBody2D _body;
    private readonly Marker2D _shoulder;
    private PinJoint2D _joint;

    public GrabHand(RigidBody2D body, Marker2D shoulder)
    {
        _body = body;
        _shoulder = shoulder;
    }

    public void TryGrab(Vector2 worldTarget)
    {
        if (IsGrabbing) return;

        var spaceState = _body.GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            _shoulder.GlobalPosition,
            worldTarget,
            collisionMask: 3
        );
        query.Exclude = new Godot.Collections.Array<Rid> { _body.GetRid() };

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return;

        Vector2 hitPoint = (Vector2)result["position"];
        GrabPoint = hitPoint;

        _joint = new PinJoint2D();
        _joint.GlobalPosition = hitPoint;
        _body.GetParent().AddChild(_joint);

        var hitBody = (Node2D)result["collider"];
        _joint.NodeA = _joint.GetPathTo(hitBody);
        _joint.NodeB = _joint.GetPathTo(_body);
        _joint.Softness = 0f;

        IsGrabbing = true;
    }

    public void Release()
    {
        if (!IsGrabbing) return;
        if (GodotObject.IsInstanceValid(_joint))
            _joint.QueueFree();
        _joint = null;
        GrabPoint = null;
        IsGrabbing = false;
    }
}
