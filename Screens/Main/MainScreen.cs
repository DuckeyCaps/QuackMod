using Godot;
using System;
using System.Collections.Generic;

namespace SoundBinder;

public partial class MainScreen : Panel {
    private MainProgram _program;
    private Label _keys;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _keys = GetNode<Label>("Content/KeyLabels/Keys");
    }

    public void SetProgram(MainProgram program) {
        _program = program;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public Label GetLabel() {
        return _keys;
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
