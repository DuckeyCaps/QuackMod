using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundBinder;

public partial class EditScreen : Panel {

    private Label _keys;
    private Label _mode;
    private MainProgram _program;
    
    private bool _followMouse;
    private Vector2 _dragStartPos;
    private PanelContainer _topBar;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // _keys = GetNode<Label>("Content/KeyLabels/Keys");
        _keys = GetNode<Label>("Content/Keys");
        _mode = GetNode<Label>("Content/Current Mode");
        _topBar = GetNode<PanelContainer>("TopBar");
        _followMouse = false;
    }
    
    public void SetProgram(MainProgram program) {
        _program = program;
    }
    
    public Label GetLabel() {
        return _keys;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_followMouse)
            DisplayServer.WindowSetPosition(DisplayServer.WindowGetPosition() + 
                                            (Vector2I)(GetGlobalMousePosition() - _dragStartPos));
        QueueRedraw();
    }
    
    public void SetCurrentMode(bool isAdding) {
        _mode.Text = isAdding ? "Currently Adding" : "Currently Removing";
    }

    public void OnSavePressed() {
        _program.StopEditing(true);
    }

    public void OnDiscardPressed() {
        _program.StopEditing(false);
    }
    
    public void OnTopBarGuiInput(InputEvent e) {
        // GD.Print(e);
        if (e.GetType() != typeof(InputEventMouseButton)) return;
        var mouseEvent = (InputEventMouseButton)e;

        if (mouseEvent.ButtonIndex != MouseButton.Left) return;
        
        _followMouse = !_followMouse;
        if (_followMouse)
            _dragStartPos = GetLocalMousePosition();

    }
    
    public void OnMinPressed() {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
    }
    
    public void OnClosePressed() {
        _program.CloseGracefully();
    }    
}
