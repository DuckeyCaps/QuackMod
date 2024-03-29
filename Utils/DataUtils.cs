using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpHook;
using SharpHook.Native;

namespace SoundBinder.Utils;

public static class DataUtils {
    
    private static string LocalAppDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
    private const string SaveDataFolder = "Duckeys";
    private const string SaveDataFileName = "quackmod.sav";

    public static void SaveData(HashSet<KeyCode> activeKeys) {
        var saveLocation = Path.Join(LocalAppDataPath, SaveDataFolder, SaveDataFileName);
        Directory.CreateDirectory(Path.Join(LocalAppDataPath, SaveDataFolder));
        using var fileStream = File.Open(saveLocation, FileMode.Create);
        using var writer = new BinaryWriter(fileStream, Encoding.UTF8, false);
        
        foreach (var key in activeKeys) {
            writer.Write((ushort)key);
        }
    }
    
    
    public static HashSet<KeyCode> LoadData() {
        var saveLocation = Path.Join(LocalAppDataPath, SaveDataFolder, SaveDataFileName);
        if (!File.Exists(saveLocation)) return new HashSet<KeyCode>();

        using var fileStream = File.Open(ProjectSettings.GlobalizePath(saveLocation), FileMode.Open);
        using var reader = new BinaryReader(fileStream, Encoding.UTF8, false);
        var hasData = true;
        var activeKeys = new HashSet<KeyCode>();
        
        while (hasData) {
            try {
                var keyVal = reader.ReadUInt16();
                activeKeys.Add((KeyCode)keyVal);
            }
            catch (EndOfStreamException e) {
                // Console.WriteLine("End of save data.");
                hasData = false;
            }
        }

        return activeKeys;
    }

    public static bool DoesSaveFileExist() {
        var saveLocation = Path.Join(LocalAppDataPath, SaveDataFolder, SaveDataFileName);
        return File.Exists(saveLocation);
    }
}