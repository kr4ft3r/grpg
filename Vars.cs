using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vars
{
    private Dictionary<string, Vector3> _vectorVars;
    private Dictionary<string, string> _stringVars;
    private Dictionary<string, bool> _boolVars;
    private Dictionary<string, int> _intVars;
    private Dictionary<string, float> _floatVars;

    public Vars()
    {
        _vectorVars = new Dictionary<string, Vector3>();
        _stringVars = new Dictionary<string, string>();
        _boolVars = new Dictionary<string, bool>();
        _intVars = new Dictionary<string, int>();
        _floatVars = new Dictionary<string, float>();
    }

    public void SetVector(string index, Vector3 pos)
    {
        _vectorVars[index] = pos;
    }

    public Vector3 GetVector(string index)
    {
        return _vectorVars[index];
    }

    public bool IsVectorSet(string index)
    {
        return _vectorVars.ContainsKey(index);
    }

    public void SetString(string index, string value)
    {
        _stringVars[index] = value;
    }

    public string GetString(string index)
    {
        return _stringVars[index];
    }

    public bool IsStringSet(string index)
    {
        return _stringVars.ContainsKey(index);
    }

    public void SetBool(string index, bool value)
    {
        _boolVars[index] = value;
    }

    public bool GetBool(string index)
    {
        return _boolVars[index];
    }

    public bool IsBoolSet(string index)
    {
        return _boolVars.ContainsKey(index);
    }

    public void SetInt(string index, int value)
    {
        _intVars[index] = value;
    }

    public int GetInt(string index)
    {
        return _intVars[index];
    }

    public bool IsIntSet(string index)
    {
        return _intVars.ContainsKey(index);
    }

    public void SetFloat(string index, float value)
    {
        _floatVars[index] = value;
    }

    public float GetFloat(string index)
    {
        return _floatVars[index];
    }

    public bool IsFloatSet(string index)
    {
        return _floatVars.ContainsKey(index);
    }
}
