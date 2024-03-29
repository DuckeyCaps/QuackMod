using Godot;


namespace SoundBinder;

public partial class MainScreen : Panel {
    private MainProgram _program;
    private Label _keys;

    private VBoxContainer _mainContent;
    private VBoxContainer _helpContent;
    private PanelContainer _topBar;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //_keys = GetNode<Label>("Content/KeyLabels/Keys");
        _keys = GetNode<Label>("Content/Keys");
        _topBar = GetNode<PanelContainer>("TopBar");
        _mainContent = GetNode<VBoxContainer>("Content");
        _helpContent = GetNode<VBoxContainer>("HelpContent");
    }

    public void SetProgram(MainProgram program) {
        _program = program;
    }

    public void CheckFirstScreen() {
        if (Utils.DataUtils.DoesSaveFileExist()) return;
        
        OnHelpPressed();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public Label GetLabel() {
        return _keys;
    }

    public void OnQuackPressed() {
        _program.Quack();
    }

    public void OnHelpPressed() {
        _helpContent.Visible = true;
        _mainContent.Visible = false;
        _topBar.Visible = false;
    }

    public void OnCloseHelpPressed() {
        _helpContent.Visible = false;
        _mainContent.Visible = true;
        _topBar.Visible = true;
    }

    public void OnMinPressed() {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
    }
    
    public void OnClosePressed() {
        _program.CloseGracefully();
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
