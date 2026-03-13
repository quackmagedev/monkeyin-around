using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("Center/VBox/PlayButton").Pressed += OnPlayPressed;
	}

	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/Level.tscn");
	}
}