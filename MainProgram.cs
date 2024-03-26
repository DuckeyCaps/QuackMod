using Godot;
using System;
using System.Collections.Generic;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;

public class KeyInfo {
    public KeyCode KeyCode { get; set; }
    public ushort RawCode { get; set; }
    public char KeyChar { get; set; }

    public KeyInfo(KeyboardEventData data) {
        KeyCode = data.KeyCode;
        RawCode = data.RawCode;
        KeyChar = data.KeyChar;
    }
}

public partial class MainProgram : Node2D {
    private AudioStreamPlayer _player;

    private CanvasLayer _uiLayer;

    private PackedScene _mainScreenScene;
    private PackedScene _editScreenScene;
    
    private MainScreen _mainScreen;
    private Button _addKeysButton;
    private Button _removeKeysButton;
    private Button _clearKeysButton;
    
    private EditScreen _editScreen;

    private List<KeyInfo> _activeKeys = new();

    private SimpleGlobalHook _globalHook;
    
    private enum EditState {
        NotEditing,
        Adding,
        Removing
    }

    private EditState _editState;
    
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready() {
        _player = GetNode<AudioStreamPlayer>("SFXPlayer");
        _uiLayer = GetNode<CanvasLayer>("UI");

        _mainScreenScene = GD.Load<PackedScene>("res://Screens/Main/main_screen.tscn");
        _editScreenScene = GD.Load<PackedScene>("res://Screens/Edit/edit_screen.tscn");

        _mainScreen = _mainScreenScene.Instantiate<MainScreen>();
        _editScreen = _editScreenScene.Instantiate<EditScreen>();
        
        _mainScreen.SetProgram(this);
        _editScreen.SetProgram(this);
        
        _uiLayer.AddChild(_mainScreen);
        // _addKeysButton = _mainScreen.GetNode<Button>("Content/Buttons/Add");
        // _removeKeysButton = _mainScreen.GetNode<Button>("Content/Buttons/Remove");
        // _clearKeysButton = _mainScreen.GetNode<Button>("Content/Buttons/Clear");
        
        _uiLayer.AddChild(_editScreen);
        _editScreen.Visible = false;
        _editState = EditState.NotEditing;

        _globalHook = new SimpleGlobalHook(globalHookType: GlobalHookType.Keyboard);
        
        _globalHook.KeyTyped += GlobalHookOnKeyTyped;
        
        await _globalHook.RunAsync();
    }

    private void GlobalHookOnKeyTyped(object sender, KeyboardHookEventArgs e) {
        var eventData = e.Data;
        var keyInfo = new KeyInfo(eventData);

        if (_editState == EditState.NotEditing) {
            //if (_activeKeys.Contains(keyInfo))
            _player.CallDeferred("play");    
        }
        
        else {
            HandleEditKeyPress(keyInfo);
        }

        
    }

    private void HandleEditKeyPress(KeyInfo keyPressed) {
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        QueueRedraw();
    }
    
    public override void _Notification(int what) {
        if (what != NotificationWMCloseRequest) return;
        
        _globalHook.Dispose();   
        GetTree().Quit();
    }

    public void StartEditing(bool addMode) {
        GD.Print("Editing!");
        _editState = addMode ? EditState.Adding : EditState.Removing;
        _mainScreen.Visible = false;
        _editScreen.Visible = true;
        _editScreen.SetCurrentMode(addMode);
    }

    public void ClearKeys() {
        GD.Print("Clearing!");
        _activeKeys.Clear();
    }
}
