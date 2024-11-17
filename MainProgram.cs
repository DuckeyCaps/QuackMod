using System.Collections.Generic;
using Godot;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;
public struct ThemeInfo
{
    public string GreenButtonHex { get; }
    public string OrangeButtonHex { get; }
    public string RedButtonHex { get; }

    public string BackgroundHex { get; }
    public string TopBarHex { get; }

    public string IconName { get; }
    public string SoundName { get; }

    public ThemeInfo(string green, string orange, string red, string back, string top, string icon, string sound)
    {
        GreenButtonHex = green;
        OrangeButtonHex = orange;
        RedButtonHex = red;

        BackgroundHex = back;
        TopBarHex = top;

        IconName = icon;
        SoundName = sound;
    }
}

public partial class MainProgram : Node2D
{
    public Dictionary<string, ThemeInfo> Themes;

    private AudioStreamPlayer _player;

    private CanvasLayer _uiLayer;

    private PackedScene _mainScreenScene;

    private MainScreen _mainScreen;

    private Label _mainKeyLabel;
    private Label _editKeyLabel;

    private HashSet<KeyCode> _activeKeys = new();
    private readonly HashSet<KeyCode> _tempKeys = new();

    private SimpleGlobalHook _globalHook;

    private enum EditState
    {
        NotEditing,
        Adding,
        Removing
    }

    private EditState _editState;

    private bool _freshStart = true;
    private HashSet<KeyCode> _hasQuacked = new();

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        _player = GetNode<AudioStreamPlayer>("SFXPlayer");
        _uiLayer = GetNode<CanvasLayer>("UI");

        _activeKeys = Utils.DataUtils.LoadData();

        _mainScreenScene = GD.Load<PackedScene>("res://Screens/Main/main_screen.tscn");
        _mainScreen = _mainScreenScene.Instantiate<MainScreen>();

        _mainScreen.SetProgram(this);

        _uiLayer.AddChild(_mainScreen);
        
        _editState = EditState.NotEditing;

        _globalHook = new SimpleGlobalHook(globalHookType: GlobalHookType.Keyboard);

        _globalHook.KeyPressed += GlobalHookOnKeyPressed;
        _globalHook.KeyReleased += GlobalHookOnKeyReleased;

        await _globalHook.RunAsync();
    }

    private void GlobalHookOnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        var eventData = e.Data;
        GD.Print(e.Data);

        if (_editState != EditState.NotEditing)
        {
            HandleEditKeyPress(eventData);
        }

        else
        {
            if (_activeKeys.Contains(eventData.KeyCode) && !_hasQuacked.Contains(eventData.KeyCode))
            {
                Quack();
                _hasQuacked.Add(eventData.KeyCode);
            }
        }
    }

    private void GlobalHookOnKeyReleased(object sender, KeyboardHookEventArgs e)
    {
        var eventKey = e.Data.KeyCode;
        if (_hasQuacked.Contains(eventKey))
            _hasQuacked.Remove(eventKey);
    }

    private void HandleEditKeyPress(KeyboardEventData keyPressed)
    {
        if (!_tempKeys.Add(keyPressed.KeyCode)) return;

        var tempKeys = new List<string>();
        foreach (var key in _tempKeys)
        {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }
        var outString = string.Join(", ", tempKeys);
        _mainScreen.SetEditKeyLabel(outString);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_freshStart)
        {
            SetMainKeyLabel();
            _mainScreen.CheckFirstScreen();
            _freshStart = false;
        }
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationWMCloseRequest) return;

        CloseGracefully();
    }

    public void Quack()
    {
        _player.CallDeferred("play");
    }

    public void CloseGracefully()
    {
        _globalHook.Dispose();
        Utils.DataUtils.SaveData(_activeKeys);
        GetTree().Quit();
    }

    public void StartEditing(bool addMode)
    {
        _editState = addMode ? EditState.Adding : EditState.Removing;
        _mainScreen.ShowEditScreen();
        _mainScreen.SetEditMode(addMode);
        _mainScreen.SetEditKeyLabel("<None>");
    }

    public void StopEditing(bool shouldSave)
    {
        if (shouldSave)
        {
            foreach (var key in _tempKeys)
            {
                if (_activeKeys.Contains(key))
                {
                    if (_editState == EditState.Removing)
                        _activeKeys.Remove(key);
                }

                else
                {
                    if (_editState == EditState.Adding)
                        _activeKeys.Add(key);
                }
            }
            Utils.DataUtils.SaveData(_activeKeys);
        }

        _tempKeys.Clear();
        _mainScreen.ShowMainScreen();

        SetMainKeyLabel();

        _editState = EditState.NotEditing;
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
        _mainScreen.SetMainKeyLabel("<None>");
    }


    private void SetMainKeyLabel()
    {
        if (_activeKeys.Count == 0)
        {
            _mainScreen.SetMainKeyLabel("<None>");
            return;
        }

        var tempKeys = new List<string>();
        foreach (var key in _activeKeys)
        {
            tempKeys.Add(Utils.StringUtils.GetKeyCodeString(key));
        }
        var outString = string.Join(", ", tempKeys);
        _mainScreen.SetMainKeyLabel(outString);
    }
}
