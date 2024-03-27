using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;

// TODO: Saving/loading (probably using .NET serialization since Godot can't understand SharpHook's classes)

public partial class MainProgram : Node2D {
    private const string SaveDataLocation = "user://quackmod.sav";
    
    private AudioStreamPlayer _player;

    private CanvasLayer _uiLayer;

    private PackedScene _mainScreenScene;
    private PackedScene _editScreenScene;

    private MainScreen _mainScreen;
    private EditScreen _editScreen;

    private Label _mainKeyLabel;
    private Label _editKeyLabel;

    // private readonly Dictionary<KeyCode, KeyboardEventData> _activeKeys = new();
    // private readonly Dictionary<KeyCode, KeyboardEventData> _tempKeys = new();
    
    private readonly HashSet<KeyCode> _activeKeys = new();
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
        
        LoadData();

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
            if (_activeKeys.Contains(eventData.KeyCode)) {
                GD.Print("Quack");
                _player.CallDeferred("play");
            }    
        }
    }

    private void HandleEditKeyPress(KeyboardEventData keyPressed) {
        if (_tempKeys.Contains(keyPressed.KeyCode)) return;
        GD.Print(keyPressed.ToString());

        _tempKeys.Add(keyPressed.KeyCode);
        
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
            _freshStart = false;
        }
        QueueRedraw();
    }
    
    public override void _Notification(int what) {
        if (what != NotificationWMCloseRequest) return;

        CloseGracefully();
    }

    public void CloseGracefully() {
        _globalHook.Dispose();   
        SaveData();
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
        }
        
        _tempKeys.Clear();
        _mainScreen.Visible = true;
        _editScreen.Visible = false;
        
        SetMainKeyLabel();

        _editState = EditState.NotEditing;
    }

    public void ClearKeys() {
        GD.Print("Clearing!");
        _activeKeys.Clear();
        _mainKeyLabel.CallDeferred("set_text", "<None>");
    }
    

    private void SaveData() {
        using var fileStream = File.Open(ProjectSettings.GlobalizePath(SaveDataLocation), FileMode.Create);
        using var writer = new BinaryWriter(fileStream, Encoding.UTF8, false);
        
        foreach (var key in _activeKeys) {
            writer.Write((ushort)key);
        }
    }
    
    
    private void LoadData() {
        if (!File.Exists(ProjectSettings.GlobalizePath(SaveDataLocation))) return;

        using var fileStream = File.Open(ProjectSettings.GlobalizePath(SaveDataLocation), FileMode.Open);
        using var reader = new BinaryReader(fileStream, Encoding.UTF8, false);
        var hasData = true;
        
        while (hasData) {
            try {
                var keyVal = reader.ReadUInt16();
                _activeKeys.Add((KeyCode)keyVal);
            }
            catch (EndOfStreamException e) {
                Console.WriteLine("End of save data.");
                hasData = false;
            }
        } 
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

        var outString = string.Join(", ", tempKeys);
        _mainKeyLabel.CallDeferred("set_text", outString);
    }
}
