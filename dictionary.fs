// F#词库解释模块
namespace fqwb
open System
open System.Collections.Generic
open System.IO
open Config
open FuzzySound
open History

module Dictionary =
    // 词库数据结构
    type DictionaryData = 
        private {
            // 使用字典存储编码和对应的中文字符
            dict: Dictionary<string, string list>
            // 数据文件夹路径
            directoryPath: string
        }
    
    // 公共接口
    type DictionaryService() = 
        // 配置管理器
        let configManager = new ConfigManager()
        // 模糊音处理器
        let fuzzySoundProcessor = new FuzzySoundProcessor()
        // 历史记录管理器
        let historyManager = new HistoryManager()
        // 内部词库数据
        let mutable dictionary: DictionaryData option = None
        // 存储所有词库
        let mutable dictionaries: Dictionary<string, Dictionary<string, string list>> = new Dictionary<string, Dictionary<string, string list>>()
        // 当前词库名称
        let mutable currentDictName = "default"
        
        // 初始化词库 - 现在支持加载多个词库
        member this.Initialize(directoryPath: string) = 
            // 初始化配置管理器
            configManager.Initialize()
            // 初始化历史记录管理器
            historyManager.Initialize()
            
            // 获取配置
            let config = configManager.GetConfig()
            // 确保目录存在
            if not (Directory.Exists(directoryPath)) then
                Directory.CreateDirectory(directoryPath) |> ignore
                printfn "词库目录不存在，已创建: %s" directoryPath
                
                // 创建示例词库文件
                let sampleDictPath = Path.Combine(directoryPath, "sample_dict.txt")
                use writer = new StreamWriter(sampleDictPath)
                writer.WriteLine("// 五笔加加词库示例")
                writer.WriteLine("wbjj 五笔加加")
                writer.WriteLine("fqwb 反切五笔")
                writer.WriteLine("srfh 输入法")
                writer.WriteLine("ymrf 编码")
                writer.WriteLine("thnn 词组")
                writer.Close()
                
                // 创建第二个示例词库文件，用于演示词库切换功能
                let sampleDictPath2 = Path.Combine(directoryPath, "example.dic")
                use writer2 = new StreamWriter(sampleDictPath2)
                writer2.WriteLine("abc 测试")
                writer2.WriteLine("def 输入法")
                writer2.WriteLine("ghi Windows")
                writer2.WriteLine("jkl TSF")
                writer2.WriteLine("mno 反切五笔")
                writer2.Close()
            
            // 清空之前的词库数据
            dictionaries.Clear()
            
            // 加载词库 - 读取目录中的所有txt和dic文件
            try
                // 先加载所有.dic文件
                let dicFiles = Directory.GetFiles(directoryPath, "*.dic", SearchOption.AllDirectories)
                for file in dicFiles do
                    try
                        let dictName = Path.GetFileNameWithoutExtension(file)
                        try
                            let dict = new Dictionary<string, string list>()
                            for line in File.ReadAllLines(file) do
                                // 跳过空行和注释
                                if not (String.IsNullOrWhiteSpace(line)) && not (line.StartsWith("//")) then
                                    // 解析行: 编码+空格+输出的字符
                                    let parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)
                                    if parts.Length >= 2 then
                                        let code = parts.[0].ToLower()
                                        let chars = parts.[1]
                                        
                                        if dict.ContainsKey(code) then
                                            // 避免重复添加相同的词组
                                            if not (Seq.contains chars dict.[code]) then
                                                dict.[code] <- chars :: dict.[code]
                                        else
                                            dict.Add(code, [chars])
                            dictionaries.Add(dictName, dict)
                        with
                        | ex -> printfn "加载文件 %s 时出错: %s" file ex.Message
                    with
                    | ex -> printfn "加载.dic文件 %s 时出错: %s" file ex.Message
                
                // 再加载所有.txt文件作为一个名为"txt_dicts"的词库
                let txtFiles = Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories)
                if txtFiles.Length > 0 then
                    let txtDict = new Dictionary<string, string list>()
                    for file in txtFiles do
                        try
                            printfn "正在加载词库文件: %s" file
                            for line in File.ReadAllLines(file) do
                                // 跳过空行和注释
                                if not (String.IsNullOrWhiteSpace(line)) && not (line.StartsWith("//")) then
                                    // 解析行: 编码+空格+输出的字符
                                    let parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)
                                    if parts.Length >= 2 then
                                        let code = parts.[0].ToLower()
                                        let chars = parts.[1]
                                        
                                        if txtDict.ContainsKey(code) then
                                            // 避免重复添加相同的词组
                                            if not (Seq.contains chars txtDict.[code]) then
                                                txtDict.[code] <- chars :: txtDict.[code]
                                        else
                                            txtDict.Add(code, [chars])
                        with
                        | ex -> printfn "加载文件 %s 时出错: %s" file ex.Message
                    
                    // 保存txt词库
                    dictionaries.Add("txt_dicts", txtDict)
                    printfn "已加载 %d 个txt词库文件" txtFiles.Length
                
                // 如果没有加载到任何词库，创建默认词库
                if dictionaries.Count = 0 then
                    let defaultDict = new Dictionary<string, string list>()
                    defaultDict.Add("wbjj", ["五笔加加"])
                    defaultDict.Add("fqwb", ["反切五笔"])
                    defaultDict.Add("srfh", ["输入法"])
                    dictionaries.Add("default", defaultDict)
                
                // 默认使用配置中指定的词库或第一个词库
                if dictionaries.Count > 0 then
                    let defaultDictName = 
                        if dictionaries.ContainsKey(config.Dictionary.DefaultDictionary) then
                            config.Dictionary.DefaultDictionary
                        else
                            dictionaries.Keys |> Seq.head
                    
                    // 切换到默认词库
                    this.SwitchDictionary(defaultDictName) |> ignore
                    printfn "共加载 %d 个词库" dictionaries.Count
                
                // 初始化模糊音处理器
                let fuzzyConfig = config.Input
                if fuzzyConfig.EnableFuzzySound && fuzzyConfig.FuzzySoundMap.Count > 0 then
                    fuzzySoundProcessor.Initialize(fuzzyConfig.FuzzySoundMap, true)
                    printfn "模糊音功能已启用"
                
            with
            | ex -> printfn "加载词库时出错: %s" ex.Message
            
        // 根据编码查找对应的字符（不使用模糊音）
        member private this.SearchCodeWithoutFuzzy(code: string) : string list =
            match dictionary with
            | Some d ->
                let lowerCode = code.ToLower()
                if d.dict.ContainsKey(lowerCode) then
                    d.dict.[lowerCode]
                else
                    []
            | None ->
                // 如果词库未初始化，返回空列表
                []
        
        // 根据编码查找对应的字符（支持模糊音）
        member this.SearchCode(code: string) : string list = 
            let results = 
                if fuzzySoundProcessor.IsEnabled() then
                    fuzzySoundProcessor.SearchWithFuzzySound(code, this.SearchCodeWithoutFuzzy)
                else
                    this.SearchCodeWithoutFuzzy(code)
            
            // 使用历史记录对结果进行排序
            historyManager.GetWeightedCharacters(code, results)
            match dictionary with
            | Some d ->
                let lowerCode = code.ToLower()
                if d.dict.ContainsKey(lowerCode) then
                    if not (Seq.contains chars d.dict.[lowerCode]) then
                        d.dict.[lowerCode] <- chars :: d.dict.[lowerCode]
                else
                    d.dict.Add(lowerCode, [chars])
                
                // 同时更新当前词库在dictionaries中的副本
                if dictionaries.ContainsKey(currentDictName) then
                    let currentDict = dictionaries.[currentDictName]
                    if currentDict.ContainsKey(lowerCode) then
                        if not (Seq.contains chars currentDict.[lowerCode]) then
                            currentDict.[lowerCode] <- chars :: currentDict.[lowerCode]
                    else
                        currentDict.Add(lowerCode, [chars])
                
                // 记录用户输入到历史记录
                historyManager.RecordInput(code, chars)
                
                // 保存词库到用户词库文件
                try
                    let config = configManager.GetDictionaryConfig()
                    let userDictPath = 
                        if String.IsNullOrEmpty(config.UserDictionaryPath) then
                            Path.Combine(d.directoryPath, "user_dict.txt")
                        else
                            Path.Combine(d.directoryPath, config.UserDictionaryPath)
                    // 只保存用户添加的词到用户词库文件
                    use writer = new StreamWriter(userDictPath, false)
                    for kvp in d.dict do
                        for value in kvp.Value do
                            writer.WriteLine("{0} {1}", kvp.Key, value)
                    writer.Close()
                    printfn "用户词库已保存到: %s" userDictPath
                with
                | ex -> printfn "保存词库时出错: %s" ex.Message
            | None ->
                printfn "词库未初始化，无法添加新词"
        
        // 获取所有编码
        member this.GetAllCodes() : string seq =
            match dictionary with
            | Some d -> d.dict.Keys :> seq<_>
            | None -> Seq.empty
        
        // 清空词库
        member this.Clear() =
            match dictionary with
            | Some d -> 
                d.dict.Clear()
                // 清空用户词库文件
                try
                    let userDictPath = Path.Combine(d.directoryPath, "user_dict.txt")
                    if File.Exists(userDictPath) then
                        File.WriteAllText(userDictPath, "")
                        printfn "用户词库已清空"
                with
                | ex -> printfn "清空词库文件时出错: %s" ex.Message
            | None -> printfn "词库未初始化，无法清空词库"
        
        // 获取配置管理器
        member this.GetConfigManager() = configManager
        
        // 获取模糊音处理器
        member this.GetFuzzySoundProcessor() = fuzzySoundProcessor
        
        // 获取历史记录管理器
        member this.GetHistoryManager() = historyManager
        
        // 加载指定词库文件
        member this.LoadDictionary(dictName: string, filePath: string) =
            try
                let newDict = new Dictionary<string, string list>()
                for line in File.ReadAllLines(filePath) do
                    if not (String.IsNullOrWhiteSpace(line)) && not (line.StartsWith("//")) then
                        let parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)
                        if parts.Length >= 2 then
                            let code = parts.[0].ToLower()
                            let chars = parts.[1]
                            
                            if newDict.ContainsKey(code) then
                                if not (Seq.contains chars newDict.[code]) then
                                    newDict.[code] <- chars :: newDict.[code]
                            else
                                newDict.Add(code, [chars])
                
                if newDict.Count > 0 then
                    dictionaries.[dictName] <- newDict
                    printfn "已加载词库: %s (%d 个编码)" dictName newDict.Count
                    true
                else
                    printfn "词库文件 %s 不包含有效数据" filePath
                    false
            with
            | ex -> 
                printfn "加载词库 %s 时出错: %s" dictName ex.Message
                false
        
        // 切换到指定词库
        member this.SwitchDictionary(dictName: string) =
            if dictionaries.ContainsKey(dictName) then
                try
                    // 深拷贝当前词库到dictionary对象
                    let currentDict = dictionaries.[dictName]
                    let newDict = new Dictionary<string, string list>()
                    for kvp in currentDict do
                        newDict.Add(kvp.Key, List.ofSeq (kvp.Value))
                    
                    match dictionary with
                    | Some d ->
                        dictionary <- Some { d with dict = newDict }
                    | None ->
                        // 如果dictionary还未初始化，使用默认路径
                        let defaultPath = Environment.CurrentDirectory
                        dictionary <- Some { dict = newDict; directoryPath = defaultPath }
                    
                    currentDictName <- dictName
                    printfn "已切换到词库: %s" dictName
                    true
                with
                | ex ->
                    printfn "切换词库时出错: %s" ex.Message
                    false
            else
                printfn "未找到词库: %s" dictName
                false
        
        // 获取所有可用词库名称
        member this.GetAvailableDictionaries() : string list =
            dictionaries.Keys |> Seq.toList
        
        // 获取当前使用的词库名称
        member this.GetCurrentDictionary() : string =
            currentDictName