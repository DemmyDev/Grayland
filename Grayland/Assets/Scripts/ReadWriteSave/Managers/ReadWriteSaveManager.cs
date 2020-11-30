using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Read Write Save Manager v1.0.2
/// Made by Yukon Wainczak. Please credit me if you use any of my scripts!
/// </summary>
public class ReadWriteSaveManager : Singleton<ReadWriteSaveManager>
{
    [Header("Properties")]
    [SerializeField]
    private int version = 1;
    [SerializeField]
    private string fileName = "data";
    private string path = "";

    [Header("Events")]
    public UnityEvent OnWrite = null;

    [Header("Data")]
    [SerializeField]
    private List<DataBool> bools = null;
    [SerializeField]
    private List<DataInt> ints = null;
    [SerializeField]
    private List<DataFloat> floats = null;
    [SerializeField]
    private List<DataString> strings = null;

    private DataContainer dataContainer = new DataContainer();

    public override void Awake()
    {
        base.Awake();

        path = Application.persistentDataPath + "/" + fileName + ".dat";

        Read();
    }

    #region Read, Write, & Wipe
    /// <summary>
    /// Writes the data to a file.
    /// </summary>
    public void Write()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Create(path);

        ListToArray();

        dataContainer.version = version;

        binaryFormatter.Serialize(fileStream, dataContainer);
        fileStream.Close();

        DebugPlus.Log("Wrote save file to \"" + path + "\".", LogType.System, gameObject);

        OnWrite.Invoke();
    }

    /// <summary>
    /// Reads save data from file.
    /// Will delete save file if the current data version is higher.
    /// </summary>
    public void Read()
    {
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(path, FileMode.Open);

            DataContainer savedData = (DataContainer)binaryFormatter.Deserialize(fileStream);

            if (version > savedData.version)
            {
                fileStream.Close();

                DebugPlus.Log("Save data version (" + savedData.version + ") does not match current data version (" + version + "). Wiping save data...", LogType.System, gameObject);

                Wipe();
            }
            else
            {
                dataContainer = savedData;

                DebugPlus.Log("Loaded save file at \"" + path + "\".", LogType.System, gameObject);

                ArrayToList();

                fileStream.Close();
            }
        }
        else
        {
            DebugPlus.Log("Could not find a save file at \"" + path + "\", creating one...", LogType.System, gameObject);

            Write();
        }
    }

    /// <summary>
    /// Resets the current save file's values.
    /// </summary>
    public void Wipe()
    {
        dataContainer = new DataContainer();

        ArrayToList();

        DebugPlus.Log("Wiped save data... Saving...", LogType.System, gameObject);

        Write();

        Read();
    }
    #endregion

    #region Array & List Conversion
    /// <summary>
    /// Converts the arrays in the data class to the lists the game edits.
    /// </summary>
    public void ArrayToList()
    {
        bools.Clear();
        for (int i = 0; i < dataContainer.bools.Length; i++)
        {
            bools.Add(dataContainer.bools[i]);
        }

        ints.Clear();
        for (int i = 0; i < dataContainer.ints.Length; i++)
        {
            ints.Add(dataContainer.ints[i]);
        }

        floats.Clear();
        for (int i = 0; i < dataContainer.floats.Length; i++)
        {
            floats.Add(dataContainer.floats[i]);
        }

        strings.Clear();
        for (int i = 0; i < dataContainer.strings.Length; i++)
        {
            strings.Add(dataContainer.strings[i]);
        }

        DebugPlus.Log("Updated list values.", LogType.System, gameObject);
    }

    /// <summary>
    /// Converts the lists the game edits to the arrays in the data class.
    /// </summary>
    public void ListToArray()
    {
        dataContainer.bools = new DataBool[bools.Count];
        for (int i = 0; i < bools.Count; i++)
        {
            dataContainer.bools[i] = bools[i];
        }

        dataContainer.ints = new DataInt[ints.Count];
        for (int i = 0; i < ints.Count; i++)
        {
            dataContainer.ints[i] = ints[i];
        }

        dataContainer.floats = new DataFloat[floats.Count];
        for (int i = 0; i < floats.Count; i++)
        {
            dataContainer.floats[i] = floats[i];
        }

        dataContainer.strings = new DataString[strings.Count];
        for (int i = 0; i < strings.Count; i++)
        {
            dataContainer.strings[i] = strings[i];
        }

        DebugPlus.Log("Updated data array values.", LogType.System, gameObject);
    }
    #endregion

    #region Get Data
    /// <summary>
    /// Searches for a saved data value.
    /// </summary>
    /// <param name="name">The name of the value stored in the data.</param>
    /// <param name="fallback">What value should be returned if there is no value found with that name?</param>
    /// <param name="saveFallback">Should Read Write Save Manager save the fallback value if no other value is found?</param>
    public bool GetData(string name, bool fallback, bool saveFallback = false)
    {
        foreach (DataBool dataBool in bools)
        {
            if (dataBool.name == name)
            {
                return dataBool.value;
            }
        }

        if (saveFallback)
        {
            SetData(name, fallback, true);
        }

        return fallback;
    }

    /// <summary>
    /// Searches for a saved data value.
    /// </summary>
    /// <param name="name">The name of the value stored in the data.</param>
    /// <param name="fallback">What value should be returned if there is no value found with that name?</param>
    /// <param name="saveFallback">Should Read Write Save Manager save the fallback value if no other value is found?</param>
    public int GetData(string name, int fallback, bool saveFallback = false)
    {
        foreach (DataInt dataInt in ints)
        {
            if (dataInt.name == name)
            {
                return dataInt.value;
            }
        }

        if (saveFallback)
        {
            SetData(name, fallback, true);
        }

        return fallback;
    }

    /// <summary>
    /// Searches for a saved data value.
    /// </summary>
    /// <param name="name">The name of the value stored in the data.</param>
    /// <param name="fallback">What value should be returned if there is no value found with that name?</param>
    /// <param name="saveFallback">Should Read Write Save Manager save the fallback value if no other value is found?</param>
    public float GetData(string name, float fallback, bool saveFallback = false)
    {
        foreach (DataFloat dataFloat in floats)
        {
            if (dataFloat.name == name)
            {
                return dataFloat.value;
            }
        }

        if (saveFallback)
        {
            SetData(name, fallback, true);
        }

        return fallback;
    }

    /// <summary>
    /// Searches for a saved data value.
    /// </summary>
    /// <param name="name">The name of the value stored in the data.</param>
    /// <param name="fallback">What value should be returned if there is no value found with that name?</param>
    /// <param name="saveFallback">Should Read Write Save Manager save the fallback value if no other value is found?</param>
    public string GetData(string name, string fallback, bool saveFallback = false)
    {
        foreach (DataString dataString in strings)
        {
            if (dataString.name == name)
            {
                return dataString.value;
            }
        }

        if (saveFallback)
        {
            SetData(name, fallback, true);
        }

        return fallback;
    }
    #endregion

    #region Set Data
    /// <summary>
    /// Saves an data value.
    /// </summary>
    /// <param name="name">The name of the value you are trying to save.</param>
    /// <param name="value">What value should be saved under this name?</param>
    /// <param name="save">Should Read Write Save Manager save the value?</param>
    public void SetData(string name, bool value, bool save = false)
    {
        foreach (DataBool dataBool in bools)
        {
            if (dataBool.name == name)
            {
                dataBool.value = value;

                DebugPlus.Log("Set bool data value \"" + name + "\" to \"" + value + "\".", LogType.System, gameObject);

                if (save)
                {
                    Write();
                }

                return;
            }
        }

        bools.Add(new DataBool(name, value));

        DebugPlus.Log("Added bool data value \"" + name + "\" with a value of \"" + value + "\".", LogType.System, gameObject);

        if (save)
        {
            Write();
        }

        return;
    }

    /// <summary>
    /// Saves an data value.
    /// </summary>
    /// <param name="name">The name of the value you are trying to save.</param>
    /// <param name="value">What value should be saved under this name?</param>
    /// <param name="save">Should Read Write Save Manager save the value?</param>
    public void SetData(string name, int value, bool save = false)
    {
        foreach (DataInt dataInt in ints)
        {
            if (dataInt.name == name)
            {
                dataInt.value = value;

                DebugPlus.Log("Set int data value \"" + name + "\" to \"" + value + "\".", LogType.System, gameObject);

                if (save)
                {
                    Write();
                }

                return;
            }
        }

        ints.Add(new DataInt(name, value));

        DebugPlus.Log("Added int data value \"" + name + "\" with a value of \"" + value + "\".", LogType.System, gameObject);

        if (save)
        {
            Write();
        }

        return;
    }

    /// <summary>
    /// Saves an data value.
    /// </summary>
    /// <param name="name">The name of the value you are trying to save.</param>
    /// <param name="value">What value should be saved under this name?</param>
    /// <param name="save">Should Read Write Save Manager save the value?</param>
    public void SetData(string name, float value, bool save = false)
    {
        foreach (DataFloat dataFloat in floats)
        {
            if (dataFloat.name == name)
            {
                dataFloat.value = value;

                DebugPlus.Log("Set float data value \"" + name + "\" to \"" + value + "\".", LogType.System, gameObject);

                if (save)
                {
                    Write();
                }

                return;
            }
        }

        floats.Add(new DataFloat(name, value));

        DebugPlus.Log("Added float data value \"" + name + "\" with a value of \"" + value + "\".", LogType.System, gameObject);

        if (save)
        {
            Write();
        }

        return;
    }

    /// <summary>
    /// Saves an data value.
    /// </summary>
    /// <param name="name">The name of the value you are trying to save.</param>
    /// <param name="value">What value should be saved under this name?</param>
    /// <param name="save">Should Read Write Save Manager save the value?</param>
    public void SetData(string name, string value, bool save = false)
    {
        foreach (DataString dataString in strings)
        {
            if (dataString.name == name)
            {
                dataString.value = value;

                DebugPlus.Log("Set string data value \"" + name + "\" to \"" + value + "\".", LogType.System, gameObject);

                if (save)
                {
                    Write();
                }

                return;
            }
        }

        strings.Add(new DataString(name, value));

        DebugPlus.Log("Added string data value \"" + name + "\" with a value of \"" + value + "\".", LogType.System, gameObject);

        if (save)
        {
            Write();
        }

        return;
    }
    #endregion
}

[System.Serializable]
public class DataContainer
{
    public int version;

    public DataBool[] bools = new DataBool[0];
    public DataInt[] ints = new DataInt[0];
    public DataFloat[] floats = new DataFloat[0];
    public DataString[] strings = new DataString[0];
}

[System.Serializable]
public class DataBool
{
    public string name;
    public bool value;

    public DataBool(string _name, bool _value)
    {
        name = _name;
        value = _value;
    }
}

[System.Serializable]
public class DataInt
{
    public string name;
    public int value;

    public DataInt(string _name, int _value)
    {
        name = _name;
        value = _value;
    }
}

[System.Serializable]
public class DataFloat
{
    public string name;
    public float value;

    public DataFloat(string _name, float _value)
    {
        name = _name;
        value = _value;
    }
}

[System.Serializable]
public class DataString
{
    public string name;
    public string value;

    public DataString(string _name, string _value)
    {
        name = _name;
        value = _value;
    }
}