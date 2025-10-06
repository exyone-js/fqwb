module fqwb.FuzzySound

open System
open System.Collections.Generic

/// <summary>
/// 模糊音处理器类，负责处理输入法中的模糊音功能
/// </summary>
type FuzzySoundProcessor() = 
    // 模糊音映射表
    let mutable fuzzySoundMap : Dictionary<string, List<string>> = new Dictionary<string, List<string>>()
    
    // 是否启用模糊音
    let mutable isEnabled = false
    
    /// <summary>
    /// 初始化模糊音处理器
    /// </summary>
    member this.Initialize() = 
        // 加载默认模糊音映射
        fuzzySoundMap <- GetDefaultFuzzySoundMap()
        
        // 默认启用模糊音
        isEnabled <- true
    
    /// <summary>
    /// 更新模糊音设置
    /// </summary>
    /// <param name="map">新的模糊音映射表</param>
    member this.UpdateSettings(map : Dictionary<string, List<string>>) = 
        fuzzySoundMap <- map
    
    /// <summary>
    /// 启用或禁用模糊音功能
    /// </summary>
    /// <param name="enabled">是否启用模糊音</param>
    member this.SetEnabled(enabled : bool) = 
        isEnabled <- enabled
    
    /// <summary>
    /// 检查模糊音功能是否已启用
    /// </summary>
    /// <returns>模糊音功能状态</returns>
    member this.IsEnabled() : bool = 
        isEnabled
    
    /// <summary>
    /// 生成一个编码的所有可能的模糊音变体
    /// </summary>
    /// <param name="code">输入编码</param>
    /// <returns>编码的所有模糊音变体列表</returns>
    member this.GenerateFuzzyVariants(code : string) : List<string> = 
        // 如果模糊音功能未启用，直接返回原编码
        if not isEnabled || code.Length = 0 then
            let result = new List<string>()
            result.Add(code)
            result
        else
            // 调用递归函数生成所有变体
            let rec generateVariants(currentCode : string, position : int, results : List<string>) = 
                // 如果已经处理完所有字符，将结果添加到列表中
                if position >= currentCode.Length then
                    if not (results.Contains(currentCode)) then
                        results.Add(currentCode)
                    
                else
                    // 获取当前位置的字符
                    let currentChar = currentCode[position].ToString()
                    
                    // 尝试替换当前字符为其模糊音变体
                    if fuzzySoundMap.ContainsKey(currentChar) then
                        let variants = fuzzySoundMap[currentChar]
                        
                        // 对于每个变体，递归处理剩余字符
                        for variant in variants do
                            let newCode = currentCode.Substring(0, position) + variant + currentCode.Substring(position + 1)
                            generateVariants(newCode, position + 1, results)
                    
                    // 不替换当前字符，继续处理下一个字符
                    generateVariants(currentCode, position + 1, results)
            
            let results = new List<string>()
            generateVariants(code, 0, results)
            results
    
    /// <summary>
    /// 使用模糊音进行搜索
    /// </summary>
    /// <param name="code">输入编码</param>
    /// <param name="searchFunction">搜索函数</param>
    /// <returns>搜索结果列表</returns>
    member this.SearchWithFuzzySound(code : string, searchFunction : string -> List<string>) : List<string> = 
        // 如果模糊音功能未启用，直接使用原编码搜索
        if not isEnabled || code.Length = 0 then
            searchFunction code
        else
            // 生成所有可能的模糊音变体
            let variants = this.GenerateFuzzyVariants(code)
            let results = new List<string>()
            
            // 对每个变体进行搜索
            for variant in variants do
                let variantResults = searchFunction variant
                for result in variantResults do
                    if not (results.Contains(result)) then
                        results.Add(result)
            
            results

/// <summary>
/// 获取默认的模糊音映射表
/// 包含常见的模糊音配置，如平翘舌、前后鼻音等
/// </summary>
/// <returns>默认模糊音映射表</returns>
let GetDefaultFuzzySoundMap() : Dictionary<string, List<string>> = 
    let map = new Dictionary<string, List<string>>()
    
    // 平翘舌音模糊
    map.Add("z", new List<string>([ "z"; "zh" ]))
    map.Add("c", new List<string>([ "c"; "ch" ]))
    map.Add("s", new List<string>([ "s"; "sh" ]))
    map.Add("zh", new List<string>([ "z"; "zh" ]))
    map.Add("ch", new List<string>([ "c"; "ch" ]))
    map.Add("sh", new List<string>([ "s"; "sh" ]))
    
    // 前后鼻音模糊
    map.Add("an", new List<string>([ "an"; "ang" ]))
    map.Add("en", new List<string>([ "en"; "eng" ]))
    map.Add("in", new List<string>([ "in"; "ing" ]))
    map.Add("ang", new List<string>([ "an"; "ang" ]))
    map.Add("eng", new List<string>([ "en"; "eng" ]))
    map.Add("ing", new List<string>([ "in"; "ing" ]))
    
    // 其他常见模糊音
    map.Add("l", new List<string>([ "l"; "n" ]))
    map.Add("n", new List<string>([ "l"; "n" ]))
    map.Add("f", new List<string>([ "f"; "h" ]))
    map.Add("h", new List<string>([ "f"; "h" ]))
    
    // 尖团音模糊
    map.Add("j", new List<string>([ "j"; "z" ]))
    map.Add("q", new List<string>([ "q"; "c" ]))
    map.Add("x", new List<string>([ "x"; "s" ]))
    
    map