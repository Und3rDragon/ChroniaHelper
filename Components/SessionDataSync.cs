using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroniaHelper.Cores;
using ChroniaHelper.Utils;
using ChroniaHelper.Utils.ChroniaSystem;

namespace ChroniaHelper.Components;

public class SessionSliderSync : BaseComponent
{
    public string Key;
    public Func<float> GetData;
    public Action<float> SetData;
    private float _data;

    private bool syncing = false;

    public SessionSliderSync(string key, Func<float> getter, Action<float> setter)
    {
        Key = key;
        SetData = setter;
        GetData = getter;

        _data = GetData();
        _sessionValue = SessionValue;
    }

    private float SessionValue => Key.GetSlider();
    private float _sessionValue;

    private const float threshold = 0.00001f;
    public override void Update()
    {
        if (syncing) return;

        float value = SessionValue;
        float data = GetData();

        bool diffData = (_data - data).GetAbs() > threshold;
        bool diffSession = (_sessionValue - value).GetAbs() > threshold;

        if(diffSession)
        {
            syncing = true;
            SetData(value);
            syncing = false;
        }
        else if (diffData)
        {
            syncing = true;
            Key.SetSlider(data);
            syncing = false;
        }

        _sessionValue = SessionValue;
        _data = GetData();
    }
}

public class SessionFlagSync : BaseComponent
{
    public string Key;
    public Func<bool> GetData;
    public Action<bool> SetData;
    private bool _data;

    private bool syncing = false;

    public SessionFlagSync(string key, Func<bool> getter, Action<bool> setter)
    {
        Key = key;
        SetData = setter;
        GetData = getter;

        _data = GetData();
        _sessionValue = SessionValue;
    }

    private bool SessionValue => Key.GetFlag();
    private bool _sessionValue;

    public override void Update()
    {
        if (syncing) return;

        bool value = SessionValue;
        bool data = GetData();

        bool diffData = _data != data;
        bool diffSession = _sessionValue != value;

        if (diffSession)
        {
            syncing = true;
            SetData(value);
            syncing = false;
        }
        else if (diffData)
        {
            syncing = true;
            Key.SetFlag(data);
            syncing = false;
        }

        _sessionValue = SessionValue;
        _data = GetData();
    }
}

public class SessionCounterSync : BaseComponent
{
    public string Key;
    public Func<int> GetData;
    public Action<int> SetData;
    private int _data;

    private bool syncing = false;

    public SessionCounterSync(string key, Func<int> getter, Action<int> setter)
    {
        Key = key;
        SetData = setter;
        GetData = getter;

        _data = GetData();
        _sessionValue = SessionValue;
    }

    private int SessionValue => Key.GetCounter();
    private int _sessionValue;

    public override void Update()
    {
        if (syncing) return;

        int value = SessionValue;
        int data = GetData();

        bool diffData = _data != data;
        bool diffSession = _sessionValue != value;

        if (diffSession)
        {
            syncing = true;
            SetData(value);
            syncing = false;
        }
        else if (diffData)
        {
            syncing = true;
            Key.SetCounter(data);
            syncing = false;
        }

        _sessionValue = SessionValue;
        _data = GetData();
    }
}

public class SessionKeySync : BaseComponent
{
    public string Key;
    public Func<string> GetData;
    public Action<string> SetData;
    private string _data;

    private bool syncing = false;

    public SessionKeySync(string key, Func<string> getter, Action<string> setter)
    {
        Key = key;
        SetData = setter;
        GetData = getter;

        _data = GetData();
        _sessionValue = SessionValue;
    }

    private string SessionValue => Md.Session.keystrings.GetValueOrDefault(Key, "");
    private string _sessionValue;

    public override void Update()
    {
        if (syncing) return;

        string value = SessionValue;
        string data = GetData();

        bool diffData = _data != data;
        bool diffSession = _sessionValue != value;

        if (diffSession)
        {
            syncing = true;
            SetData(value);
            syncing = false;
        }
        else if (diffData)
        {
            syncing = true;
            Md.Session.keystrings[Key] = data;
            syncing = false;
        }

        _sessionValue = SessionValue;
        _data = GetData();
    }
}
