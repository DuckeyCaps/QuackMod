using Godot;
using System;
using System.Collections.Generic;

namespace SoundBinder;

public partial class EditScreen : Panel {

    private Label _keys;
    private Label _mode;
    private MainProgram _program;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        _keys = GetNode<Label>("Content/KeyLabels/Keys");
        _mode = GetNode<Label>("Content/Current Mode");
    }
    
    public void SetProgram(MainProgram program) {
        _program = program;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetCurrentKeys(List<KeyInfo> currentKeys) {
        var tempKeys = new List<char>();
        foreach (var key in currentKeys) {
            tempKeys.Add(key.KeyChar);
        }
    }

    public void SetCurrentMode(bool isAdding) {
        _mode.Text = isAdding ? "Currently Adding" : "Currently Removing";
    }
}
