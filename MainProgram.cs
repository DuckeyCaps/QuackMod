using System.Collections.Generic;
using Godot;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder;

public partial class MainProgram : Node2D
{
    private AudioStreamPlayer _player;
    private Button _logoButton; 

    private PackedScene _mainScreenScene;

    private MainScreen _mainScreen;
    private PanelContainer _topBar; 

    private Label _mainKeyLabel;
    private Label _editKeyLabel;

    private Button _duckButton;
    private Button _penguinButton;

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
    private readonly HashSet<KeyCode> _hasQuacked = new();

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        _player = GetNode<AudioStreamPlayer>("SFXPlayer");

        _activeKeys = Utils.DataUtils.LoadData();

        _mainScreen = GetNode<MainScreen>("UI/MainScreen");
        _topBar = GetNode<PanelContainer>("UI/MainScreen/TopBar");
        _logoButton = GetNode<Button>("UI/MainScreen/SoundButton");

        // TODO: Figure out how to load and connect to theme buttons from here
        // ... or just... stick MainScreen inside of the UI in the editor and not need to instantiate it...?
        
        _mainScreen.SetProgram(this);
        
        _editState = EditState.NotEditing;

        _globalHook = new SimpleGlobalHook(globalHookType: GlobalHookType.Keyboard);

        _globalHook.KeyPressed += GlobalHookOnKeyPressed;
        _globalHook.KeyReleased += GlobalHookOnKeyReleased;

        InitButtons();

        var savedTheme = Utils.DataUtils.LoadTheme();
        switch (savedTheme) {
            case "Penguin":
                ApplyPenguinTheme();
                break;
            default:
                ApplyDuckTheme();
                break;

            
        }

        await _globalHook.RunAsync();
    }

    private void GlobalHookOnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        var eventData = e.Data;
        GD.Print(e.Data);

        if (_editState != EditState.NotEditing)
        {
            HandleEditKeyPress(eventData);
            return;
        }
        
        if (!_activeKeys.Contains(eventData.KeyCode) || _hasQuacked.Contains(eventData.KeyCode)) return;
        
        Quack();
        _hasQuacked.Add(eventData.KeyCode);
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

    private void InitButtons() {
        _duckButton = GetNode<Button>("UI/MainScreen/ThemeIcons/Duck");
        _duckButton.Pressed += ApplyDuckTheme;
        
        _penguinButton = GetNode<Button>("UI/MainScreen/ThemeIcons/Penguin");
        _penguinButton.Pressed += ApplyPenguinTheme;
    }

    private void ApplyDuckTheme() {
        _player.Stream = GD.Load<AudioStreamWav>("res://Assets/Sounds/Duck.wav");
        _logoButton.Icon = GD.Load<Texture2D>($"res://Assets/Logos/DuckLogo.png");
        _mainScreen.SelfModulate = new Color("#FFFFFF");
        _topBar.SelfModulate = new Color("#FF7118");

        _penguinButton.SelfModulate = new Color("#FFFFFF", 0.5f);
        _penguinButton.Icon = GD.Load<Texture2D>("res://Assets/Icons/Pengu60xNoOutline.png");
        
        _duckButton.SelfModulate = new Color("#FFFFFF", 1f);
        _duckButton.Icon = GD.Load<Texture2D>("res://Assets/Icons/Duckeys60xOutline.png");
        
        Utils.DataUtils.SaveTheme("Duck");
    }

    private void ApplyPenguinTheme() {
        _player.Stream = GD.Load<AudioStreamWav>("res://Assets/Sounds/Penguin.wav");
        _logoButton.Icon = GD.Load<Texture2D>($"res://Assets/Logos/PenguinLogo.png");
        _mainScreen.SelfModulate = new Color("#94D7F5");
        _topBar.SelfModulate = new Color("#2477BD");
        
        _penguinButton.SelfModulate = new Color("#FFFFFF", 1f);
        _penguinButton.Icon = GD.Load<Texture2D>("res://Assets/Icons/Pengu60xOutline.png");
        
        _duckButton.SelfModulate = new Color("#FFFFFF", 0.5f);
        _duckButton.Icon = GD.Load<Texture2D>("res://Assets/Icons/Duckeys60xNoOutline.png");
        
        Utils.DataUtils.SaveTheme("Penguin");
    } 
}
