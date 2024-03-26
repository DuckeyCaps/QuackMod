using Godot;
using System;

namespace SoundBinder;

public partial class MainScreen : Panel {
    private MainProgram _program;
    private Label _keys;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    public void SetProgram(MainProgram program) {
        _program = program;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnAddPressed() {
        _program.StartEditing(true);
    }
    
    public void OnRemovePressed() {
        _program.StartEditing(false);
        
    }
    
    public void OnClearPressed() {
        _program.ClearKeys();
    }
}
