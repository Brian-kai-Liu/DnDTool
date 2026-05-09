using GameConfig;
using Newtonsoft.Json.Linq;
using TEngine;
using UnityEngine;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigSystem
{
    private static ConfigSystem _instance;

    public static ConfigSystem Instance => _instance ??= new ConfigSystem();

    private bool _init = false;

    private Tables _tables;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Load();
            }

            return _tables;
        }
    }
    
    private IResourceModule _resourceModule;

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
    {
        _tables = new Tables(LoadJsonArray);
        _init = true;
    }

    /// <summary>
    /// 加载 JSON 配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>JArray</returns>
    private JArray LoadJsonArray(string file)
    {
        if (_resourceModule == null)
        {
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();
        }

        TextAsset textAsset = _resourceModule.LoadAsset<TextAsset>(file);
        if (textAsset == null)
        {
            throw new System.InvalidOperationException($"Config asset '{file}' was not found.");
        }

        JToken token = JToken.Parse(textAsset.text);
        if (token is JArray array)
        {
            return array;
        }

        throw new System.InvalidOperationException($"Config asset '{file}' root node is not a JSON array.");
    }
}
