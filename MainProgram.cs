using Godot;
using System.Collections.Generic;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;

public partial class MainProgram : Node2D {
    private AudioStreamPlayer _player;

    private CanvasLayer _uiLayer;

    private PackedScene _mainScreenScene;
    private PackedScene _editScreenScene;

    private MainScreen _mainScreen;
    private EditScreen _editScreen;

    private Label _mainKeyLabel;
    private Label _editKeyLabel;
    
    private HashSet<KeyCode> _activeKeys = new();
    private readonly HashSet<KeyCode> _tempKeys = new();

    private SimpleGlobalHook _globalHook;
    
    private enum EditState {
        NotEditing,
        Adding,
        Removing
    }

    private EditState _editState;

    private bool _freshStart = true;
    
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready() {
        _player = GetNode<AudioStreamPlayer>("SFXPlayer");
        _uiLayer = GetNode<CanvasLayer>("UI");
        
        _activeKeys = Utils.DataUtils.LoadData();

        _mainScreenScene = GD.Load<PackedScene>("res://Screens/Main/main_screen.tscn");
        _editScreenScene = GD.Load<PackedScene>("res://Screens/Edit/edit_screen.tscn");

        _mainScreen = _mainScreenScene.Instantiate<MainScreen>();
        _editScreen = _editScreenScene.Instantiate<EditScreen>();
        
        _mainScreen.SetProgram(this);
        _editScreen.SetProgram(this);
        
        _uiLayer.AddChild(_mainScreen);
        _uiLayer.AddChild(_editScreen);
        
        _mainKeyLabel = _mainScreen.GetLabel();
        _editKeyLabel = _editScreen.GetLabel();
        
        _editScreen.Visible = false;
        _editState = EditState.NotEditing;

        _globalHook = new SimpleGlobalHook(globalHookType: GlobalHookType.Keyboard);
        
        _globalHook.KeyPressed += GlobalHookOnKeyPressed;
        
        await _globalHook.RunAsync();
    }

    private void GlobalHookOnKeyPressed(object sender, KeyboardHookEventArgs e) {
        var eventData = e.Data;
        // GD.Print(e.Data);

        if (_editState != EditState.NotEditing) {
            HandleEditKeyPress(eventData);
        }
        
        else {
            if (_activeKeys.Contains(eventData.KeyCode)) {
                Quack();
            }    
        }
    }

    private void HandleEditKeyPress(KeyboardEventData keyPressed) {
        if (!_tempKeys.Add(keyPressed.KeyCode)) return;

        var tempKeys = new List<string>();
        foreach (var key in _tempKeys) {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }

        var outString = string.Join(", ", tempKeys);
        _editKeyLabel.CallDeferred("set_text", outString);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_freshStart) {
            SetMainKeyLabel();
            _mainScreen.CheckFirstScreen();
            _freshStart = false;
        }
        QueueRedraw();
    }
    
    public override void _Notification(int what) {
        if (what != NotificationWMCloseRequest) return;

        CloseGracefully();
    }

    public void Quack() {
        _player.CallDeferred("play");
    }

    public void CloseGracefully() {
        _globalHook.Dispose();   
        Utils.DataUtils.SaveData(_activeKeys);
        GetTree().Quit();
    }

    public void StartEditing(bool addMode) {
        _editState = addMode ? EditState.Adding : EditState.Removing;
        _mainScreen.Visible = false;
        _editScreen.Visible = true;
        _editScreen.SetCurrentMode(addMode);
        _editKeyLabel.CallDeferred("set_text", "<None>");
    }

    public void StopEditing(bool shouldSave) {
        if (shouldSave) {
            foreach (var key in _tempKeys) {
                if (_activeKeys.Contains(key)) {
                    if (_editState == EditState.Removing)
                        _activeKeys.Remove(key);
                }

                else {
                    if (_editState == EditState.Adding)
                        _activeKeys.Add(key);
                }
            }
            Utils.DataUtils.SaveData(_activeKeys);
        }
        
        _tempKeys.Clear();
        _mainScreen.Visible = true;
        _editScreen.Visible = false;
        
        SetMainKeyLabel();

        _editState = EditState.NotEditing;
    }

    public void ClearKeys() {
        _activeKeys.Clear();
        _mainKeyLabel.CallDeferred("set_text", "<None>");
    }
    

    private void SetMainKeyLabel() {
        if (_activeKeys.Count == 0) {
            _mainKeyLabel.CallDeferred("set_text", "<None>");
            return;
        }
        
        var tempKeys = new List<string>();
        foreach (var key in _activeKeys) {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }
        
        // tempKeys.Sort();

        var outString = string.Join(", ", tempKeys);
        _mainKeyLabel.CallDeferred("set_text", outString);
    }
}
