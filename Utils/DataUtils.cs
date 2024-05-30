using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpHook.Native;
using FileAccess = Godot.FileAccess;

namespace SoundBinder.Utils;

public static class DataUtils {

    private const string GodotSaveFileLocation = "user://quackmod.sav";
    
    private const string OldSaveDataFolder = "Duckeys";
    private const string SaveDataFileName = "quackmod.sav";

    public static void SaveData(HashSet<KeyCode> activeKeys) {
        using var saveFile = FileAccess.Open(GodotSaveFileLocation, FileAccess.ModeFlags.Write);
        foreach (var key in activeKeys) {
            saveFile.Store16((ushort)key);
        }
    }

    public static HashSet<KeyCode> LoadData() {
        if (!DoesSaveFileExist() && DoesOldSaveFileExist()) {
            MigrateOldSaveData();
        }

        var keys = new HashSet<KeyCode>();
        if (!DoesSaveFileExist())
            return keys;

        using var saveFile = FileAccess.Open(GodotSaveFileLocation, FileAccess.ModeFlags.Read);
        while (saveFile.GetPosition() < saveFile.GetLength()) {
            keys.Add((KeyCode)saveFile.Get16());
        }
        return keys;
    }
    
    
    public static HashSet<KeyCode> LoadDataOld() {
        var saveLocation = Path.Join(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), OldSaveDataFolder, SaveDataFileName);
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
            catch (EndOfStreamException) {
                // Console.WriteLine("End of save data.");
                hasData = false;
            }
        }

        return activeKeys;
    }

    public static bool DoesEitherSaveFileExist() {
        if (DoesSaveFileExist())
            return true;

        return DoesOldSaveFileExist() && MigrateOldSaveData();
    }

    private static bool DoesSaveFileExist() {
        return FileAccess.FileExists(GodotSaveFileLocation);
    }

    private static bool DoesOldSaveFileExist() {
        if (OS.GetName() != "Windows")
            return false;
        var saveLocation = Path.Join(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), OldSaveDataFolder, SaveDataFileName);
        return File.Exists(saveLocation);
    }

    private static bool MigrateOldSaveData() {
        var oldKeyList = LoadDataOld();
        if (oldKeyList.Count == 0)
            return false;
        
        SaveData(oldKeyList);
        GD.Print("Data successfully migrated!");
        return true;
    }
}