using Godot;


namespace SoundBinder;


public partial class MainScreen : Panel
{
    private MainProgram _program;
    private Label _mainKeys;
    private Label _editKeys;
    private Label _editMode;

    private VBoxContainer _mainContent;
    private VBoxContainer _helpContent;
    private VBoxContainer _editContent;

    private HBoxContainer _themeIcons;

    private PanelContainer _topBar;

    private Button _helpButton;
    private Label _macHelpText;

    private Button _acceptButton;
    private Button _editButton;
    private Button _clearButton;

    private Button _icon;

    private bool _followMouse;
    private Vector2 _dragStartPos;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _mainKeys = GetNode<Label>("MainContent/Keys");
        _editKeys = GetNode<Label>("EditContent/Keys");
        _editMode = GetNode<Label>("EditContent/Mode");

        _topBar = GetNode<PanelContainer>("TopBar");
        _helpButton = GetNode<Button>("TopBar/Help");

        _mainContent = GetNode<VBoxContainer>("MainContent");
        _icon = GetNode<Button>("SoundButton");

        _editContent = GetNode<VBoxContainer>("EditContent");

        _helpContent = GetNode<VBoxContainer>("HelpContent");
        _macHelpText = GetNode<Label>("HelpContent/LabelMac");

        _themeIcons = GetNode<HBoxContainer>("ThemeIcons");
    }

    //public void ApplyTheme(ThemeInfo newTheme)
    //{

    //}

    public void SetProgram(MainProgram program)
    {
        _program = program;
    }

    public void CheckFirstScreen()
    {
        if (Utils.DataUtils.DoesEitherSaveFileExist()) return;

        OnHelpPressed();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_followMouse)
            DisplayServer.WindowSetPosition(DisplayServer.WindowGetPosition() +
                                            (Vector2I)(GetGlobalMousePosition() - _dragStartPos));
    }

    public void ShowMainScreen()
    {
        _mainContent.Visible = true;
        _icon.Visible = true;
        _themeIcons.Visible = true;

        _editContent.Visible = false;

        _helpContent.Visible = false;
        _helpButton.Visible = true;
        _macHelpText.Visible = false;
    }

    public void ShowEditScreen()
    {
        _mainContent.Visible = false;
        _icon.Visible = false;
        _themeIcons.Visible = false;

        _editContent.Visible = true;

        _helpContent.Visible = false;
        _helpButton.Visible = false;
        _macHelpText.Visible = false;
    }

    private void ShowHelpScreen()
    {
        _mainContent.Visible = false;
        _icon.Visible = false; 
        _themeIcons.Visible = false;
        
        _editContent.Visible = false;

        _helpContent.Visible = true;
        _helpButton.Visible = false;
        _macHelpText.Visible = OS.GetName() == "macOS";
    }

    public void SetMainKeyLabel(string tempKeys)
    {
        _mainKeys.CallDeferred("set_text", tempKeys);
    }

    public void SetEditKeyLabel(string tempKeys)
    {
        _editKeys.CallDeferred("set_text", tempKeys);
    }

    public void SetEditMode(bool isAdding) {
        _editMode.Text = isAdding ? "Currently Adding" : "Currently Removing";
    }

    private void OnTopBarGuiInput(InputEvent e)
    {
        // GD.Print(e);
        if (e.GetType() != typeof(InputEventMouseButton)) return;
        var mouseEvent = (InputEventMouseButton)e;

        if (mouseEvent.ButtonIndex != MouseButton.Left) return;

        _followMouse = !_followMouse;
        if (_followMouse)
            _dragStartPos = GetLocalMousePosition();

    }

    private void OnQuackPressed()
    {
        _program.Quack();
    }

    private void OnHelpPressed()
    {
        ShowHelpScreen();
    }

    private void OnCloseHelpPressed()
    {
        ShowMainScreen();
    }
    
    private void OnSavePressed()
    {
        _program.StopEditing(true);
    }

    private void OnDiscardPressed()
    {
        _program.StopEditing(false);
    }

    private static void OnMinPressed()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
    }

    private void OnClosePressed()
    {
        _program.CloseGracefully();
    }

    private void OnAddPressed()
    {
        _program.StartEditing(true);
    }

    private void OnRemovePressed()
    {
        _program.StartEditing(false);
    }

    private void OnClearPressed()
    {
        _program.ClearKeys();
    }
}
