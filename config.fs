module fqwb.Config

open System
open System.IO
open Newtonsoft.Json

/// <summary>
/// 输入法配置数据结构
/// </summary>
[<CLIMutable>]
type InputMethodConfig =
    {
        /// <summary>
        /// 词库配置
        /// </summary>
        Dictionary : {
            /// <summary>
            /// 当前使用的词库名称
            /// </summary>
            CurrentDictionary : string
            
            /// <summary>
            /// 词库文件路径
            /// </summary>
            DictionaryPath : string
        }
        
        /// <summary>
        /// 界面配置
        /// </summary>
        Interface : {
            /// <summary>
            /// 是否显示候选词窗口
            /// </summary>
            ShowCandidateWindow : bool
            
            /// <summary>
            /// 候选词窗口颜色
            /// </summary>
            CandidateWindowColor : string
        }
        
        /// <summary>
        /// 输入配置
        /// </summary>
        Input : {
            /// <summary>
            /// 是否启用模糊音
            /// </summary>
            EnableFuzzySound : bool
            
            /// <summary>
            /// 是否启用历史记录
            /// </summary>
            EnableHistory : bool
            
            /// <summary>
            /// 历史记录最大数量
            /// </summary>
            HistoryMaxCount : int
            
            /// <summary>
            /// 历史记录最小使用次数
            /// </summary>
            HistoryMinUsage : int
        }
        
        /// <summary>
        /// 快捷键配置
        /// </summary>
        Hotkeys : {
            /// <summary>
            /// 清除输入快捷键
            /// </summary>
            ClearInput : string
            
            /// <summary>
            /// 切换输入法快捷键
            /// </summary>
            ToggleLanguage : string
            
            /// <summary>
            /// 上一页候选词快捷键
            /// </summary>
            PageUp : string
            
            /// <summary>
            /// 下一页候选词快捷键
            /// </summary>
            PageDown : string
        }
    }

/// <summary>
/// 默认配置
/// </summary>
let defaultConfig : InputMethodConfig = {
    Dictionary = {
        CurrentDictionary = "default"
        DictionaryPath = Path.Combine(Environment.CurrentDirectory, "data")
    }
    Interface = {
        ShowCandidateWindow = true
        CandidateWindowColor = "LightYellow"
    }
    Input = {
        EnableFuzzySound = true
        EnableHistory = true
        HistoryMaxCount = 100
        HistoryMinUsage = 3
    }
    Hotkeys = {
        ClearInput = "ESC"
        ToggleLanguage = "Ctrl+Shift"
        PageUp = "."
        PageDown = ","
    }
}

/// <summary>
/// 配置管理器类，负责加载和保存配置
/// </summary>
type ConfigManager() = 
    // 配置文件路径
    let configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json")
    
    // 当前配置
    let mutable currentConfig = defaultConfig
    
    /// <summary>
    /// 初始化配置管理器
    /// </summary>
    member this.Initialize() = 
        // 尝试加载配置文件
        if File.Exists(configFilePath) then
            try
                let json = File.ReadAllText(configFilePath)
                currentConfig <- JsonConvert.DeserializeObject<InputMethodConfig>(json)
            with
            | ex -> 
                // 加载失败，使用默认配置
                currentConfig <- defaultConfig
        else
            // 配置文件不存在，使用默认配置并保存
            currentConfig <- defaultConfig
            this.SaveConfig()
        
        currentConfig
    
    /// <summary>
    /// 获取当前配置
    /// </summary>
    member this.GetConfig() : InputMethodConfig = 
        currentConfig
    
    /// <summary>
    /// 设置新配置
    /// </summary>
    /// <param name="config">新的配置对象</param>
    member this.SetConfig(config : InputMethodConfig) : unit = 
        currentConfig <- config
    
    /// <summary>
    /// 保存配置到文件
    /// </summary>
    member this.SaveConfig() : unit = 
        try
            // 确保配置文件目录存在
            let directory = Path.GetDirectoryName(configFilePath)
            if not (Directory.Exists directory) then
                Directory.CreateDirectory directory |> ignore
                
            // 序列化配置为JSON并保存
            let json = JsonConvert.SerializeObject(currentConfig, Formatting.Indented)
            File.WriteAllText(configFilePath, json)
        with
        | ex -> 
            // 记录保存错误，但不抛出异常
            Console.WriteLine(sprintf "保存配置失败: %s" ex.Message)
    
    /// <summary>
    /// 获取词库配置
    /// </summary>
    member this.GetDictionaryConfig() = 
        currentConfig.Dictionary
    
    /// <summary>
    /// 设置词库配置
    /// </summary>
    /// <param name="dictionaryConfig">新词库配置</param>
    member this.SetDictionaryConfig(dictionaryConfig) = 
        currentConfig <- { currentConfig with Dictionary = dictionaryConfig }
        this.SaveConfig()
    
    /// <summary>
    /// 获取界面配置
    /// </summary>
    member this.GetInterfaceConfig() = 
        currentConfig.Interface
    
    /// <summary>
    /// 设置界面配置
    /// </summary>
    /// <param name="interfaceConfig">新界面配置</param>
    member this.SetInterfaceConfig(interfaceConfig) = 
        currentConfig <- { currentConfig with Interface = interfaceConfig }
        this.SaveConfig()
    
    /// <summary>
    /// 获取输入配置
    /// </summary>
    member this.GetInputConfig() = 
        currentConfig.Input
    
    /// <summary>
    /// 设置输入配置
    /// </summary>
    /// <param name="inputConfig">新输入配置</param>
    member this.SetInputConfig(inputConfig) = 
        currentConfig <- { currentConfig with Input = inputConfig }
        this.SaveConfig()
    
    /// <summary>
    /// 获取快捷键配置
    /// </summary>
    member this.GetHotkeysConfig() = 
        currentConfig.Hotkeys
    
    /// <summary>
    /// 设置快捷键配置
    /// </summary>
    /// <param name="hotkeysConfig">新快捷键配置</param>
    member this.SetHotkeysConfig(hotkeysConfig) = 
        currentConfig <- { currentConfig with Hotkeys = hotkeysConfig }
        this.SaveConfig()
    
    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    member this.ResetToDefault() : unit = 
        currentConfig <- defaultConfig
        this.SaveConfig()