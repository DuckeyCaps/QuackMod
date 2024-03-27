using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;

// TODO: Saving/loading (probably using .NET serialization since Godot can't understand SharpHook's classes)
// TODO: KeyCode -> String util class

public partial class MainProgram : Node2D {
    private AudioStreamPlayer _player;

    private CanvasLayer _uiLayer;

    private PackedScene _mainScreenScene;
    private PackedScene _editScreenScene;

    private MainScreen _mainScreen;
    private EditScreen _editScreen;

    private Label _mainKeyLabel;
    private Label _editKeyLabel;

    private readonly Dictionary<KeyCode, KeyboardEventData> _activeKeys = new();
    private readonly Dictionary<KeyCode, KeyboardEventData> _tempKeys = new();

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
        _uiLayer.AddChild(_editScreen);
        
        _mainKeyLabel = _mainScreen.GetLabel();
        _editKeyLabel = _editScreen.GetLabel();
        
        _editScreen.Visible = false;
        _editState = EditState.NotEditing;

        _globalHook = new SimpleGlobalHook(globalHookType: GlobalHookType.Keyboard);
        
        _globalHook.KeyTyped += GlobalHookOnKeyTyped;
        
        await _globalHook.RunAsync();
    }

    private void GlobalHookOnKeyTyped(object sender, KeyboardHookEventArgs e) {
        var eventData = e.Data;

        if (_editState != EditState.NotEditing) {
            HandleEditKeyPress(eventData);
        }
        
        else {
            GD.Print(_activeKeys.Count);
            if (_activeKeys.ContainsKey(eventData.KeyCode)) {
                GD.Print("Quack");
                _player.CallDeferred("play");
            }    
        }
    }

    private void HandleEditKeyPress(KeyboardEventData keyPressed) {
        if (_tempKeys.ContainsKey(keyPressed.KeyCode)) return;
        GD.Print(keyPressed.ToString());

        _tempKeys.Add(keyPressed.KeyCode, keyPressed);
        
        var tempKeys = new List<string>();
        foreach (var key in _tempKeys.Keys) {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }

        var outString = string.Join(", ", tempKeys);
        _editKeyLabel.CallDeferred("set_text", outString);
        // _editScreen.CallDeferred("queue_redraw");
        // _editScreen.CallDeferred(nameof(_editScreen.SetCurrentKeys), _tempKeys);
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
        _editState = addMode ? EditState.Adding : EditState.Removing;
        _mainScreen.Visible = false;
        _editScreen.Visible = true;
        _editScreen.SetCurrentMode(addMode);
        _editKeyLabel.CallDeferred("set_text", "<None>");
    }

    public void StopEditing(bool shouldSave) {
        if (shouldSave) {
            foreach (var key in _tempKeys.Keys) {
                if (_activeKeys.ContainsKey(key)) {
                    if (_editState == EditState.Removing)
                        _activeKeys.Remove(key);
                }

                else {
                    if (_editState == EditState.Adding)
                        _activeKeys.Add(key, _tempKeys[key]);
                }
            }
        }
        
        _tempKeys.Clear();
        _mainScreen.Visible = true;
        _editScreen.Visible = false;
        
        var tempKeys = new List<string>();
        foreach (var key in _activeKeys.Keys) {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }

        var outString = string.Join(", ", tempKeys);
        _mainKeyLabel.CallDeferred("set_text", outString);

        _editState = EditState.NotEditing;
        // _mainScreen.CallDeferred("queue_redraw");
    }

    public void ClearKeys() {
        GD.Print("Clearing!");
        _activeKeys.Clear();
    }

    // public void SaveData() {
    //     using (var fs = new FileStream("data.dat", FileMode.Create))
    //     {
    //         var formatter = new BinaryFormatter();
    //         formatter.Serialize(fs, myObject);
    //     }
    // }
    //
    // public void LoadData() {
    //     FileStream fs = new FileStream("data.dat", FileMode.Open); 
    //     BinaryFormatter formatter = new BinaryFormatter(); 
    //     MyClass myObject = (MyClass)formatter.Deserialize(fs);
    // }
}
