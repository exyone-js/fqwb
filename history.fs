module fqwb.History

open System
open System.Collections.Generic
open System.IO
open System.Text.Json

/// <summary>
/// 输入记录类，用于存储用户的输入历史
/// </summary>
type InputRecord = {
    /// <summary>
    /// 输入的编码
    /// </summary>
    Code : string
    
    /// <summary>
    /// 输出的字符
    /// </summary>
    Output : string
    
    /// <summary>
    /// 使用次数
    /// </summary>
    UsageCount : int
    
    /// <summary>
    /// 最后使用时间
    /// </summary>
    LastUsedTime : DateTime
}

/// <summary>
/// 历史记录管理器类，负责管理用户的输入历史记录
/// </summary>
type HistoryManager() = 
    // 存储输入记录的列表
    let mutable records : List<InputRecord> = new List<InputRecord>()
    
    // 历史记录文件路径
    let historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.json")
    
    // 历史记录最大数量
    let maxHistoryCount = 1000
    
    /// <summary>
    /// 初始化历史记录管理器
    /// </summary>
    member this.Initialize() = 
        // 加载历史记录
        this.LoadHistory()
    
    /// <summary>
    /// 保存历史记录到文件
    /// </summary>
    member this.SaveHistory() = 
        try
            // 确保目录存在
            let directory = Path.GetDirectoryName(historyFilePath)
            if not (Directory.Exists(directory)) then
                Directory.CreateDirectory(directory) |> ignore
            
            // 将记录序列化为JSON并保存
            let options = JsonSerializerOptions(WriteIndented = true)
            let jsonString = JsonSerializer.Serialize(records, options)
            File.WriteAllText(historyFilePath, jsonString)
        with
        | ex -> 
            // 保存失败时记录错误日志，但不抛出异常
            Console.WriteLine(sprintf "保存历史记录失败: %s" ex.Message)
    
    /// <summary>
    /// 从文件加载历史记录
    /// </summary>
    member private this.LoadHistory() = 
        try
            // 检查文件是否存在
            if File.Exists(historyFilePath) then
                // 读取JSON并反序列化为记录列表
                let jsonString = File.ReadAllText(historyFilePath)
                records <- JsonSerializer.Deserialize<List<InputRecord>>(jsonString) 
                            |> Option.defaultValue (new List<InputRecord>())
            else
                // 文件不存在时初始化空列表
                records <- new List<InputRecord>()
        with
        | ex -> 
            // 加载失败时记录错误日志，并初始化空列表
            Console.WriteLine(sprintf "加载历史记录失败: %s" ex.Message)
            records <- new List<InputRecord>()
    
    /// <summary>
    /// 限制历史记录数量，保持在最大数量以内
    /// </summary>
    member private this.LimitHistorySize() = 
        // 如果记录数量超过最大值，删除最旧的记录
        while records.Count > maxHistoryCount do
            // 找到使用时间最早的记录
            let oldestRecord = records |> Seq.minBy (fun r -> r.LastUsedTime)
            records.Remove(oldestRecord) |> ignore
    
    /// <summary>
    /// 记录用户的输入
    /// </summary>
    /// <param name="code">输入的编码</param>
    /// <param name="output">输出的字符</param>
    member this.RecordInput(code : string, output : string) = 
        // 查找是否已存在相同的编码和输出
        let existingRecord = records.Find(fun r -> r.Code = code && r.Output = output)
        
        if existingRecord <> Unchecked.defaultof<InputRecord> then
            // 如果存在，更新使用次数和最后使用时间
            records.Remove(existingRecord) |> ignore
            let updatedRecord = {
                Code = code
                Output = output
                UsageCount = existingRecord.UsageCount + 1
                LastUsedTime = DateTime.Now
            }
            records.Add(updatedRecord)
        else
            // 如果不存在，添加新记录
            let newRecord = {
                Code = code
                Output = output
                UsageCount = 1
                LastUsedTime = DateTime.Now
            }
            records.Add(newRecord)
        
        // 限制历史记录数量
        this.LimitHistorySize()
        
        // 保存历史记录
        this.SaveHistory()
    
    /// <summary>
    /// 获取指定编码的带权重的字符列表
    /// </summary>
    /// <param name="code">输入的编码</param>
    /// <returns>带权重的字符列表</returns>
    member this.GetWeightedCharacters(code : string) : List<string * float> = 
        // 过滤出指定编码的记录
        let codeRecords = records.FindAll(fun r -> r.Code = code)
        
        // 如果没有记录，返回空列表
        if codeRecords.Count = 0 then
            new List<string * float>()
        else
            // 计算总使用次数
            let totalUsage = codeRecords |> Seq.sumBy (fun r -> r.UsageCount)
            
            // 计算每个输出字符的权重
            let weightedChars = new List<string * float>()
            for record in codeRecords do
                // 计算基础权重（基于使用次数）
                let baseWeight = float record.UsageCount / float totalUsage
                
                // 计算时间衰减因子（最近使用的记录权重更高）
                let timeSpan = DateTime.Now - record.LastUsedTime
                let timeDecay = Math.Max(0.1, Math.Exp(-timeSpan.TotalDays / 7.0))
                
                // 计算最终权重
                let finalWeight = baseWeight * timeDecay
                
                weightedChars.Add((record.Output, finalWeight))
            
            // 按权重排序
            weightedChars.Sort(fun (_, w1) (_, w2) -> w2.CompareTo(w1))
            
            weightedChars
    
    /// <summary>
    /// 清空历史记录
    /// </summary>
    member this.ClearHistory() = 
        records.Clear()
        this.SaveHistory()
    
    /// <summary>
    /// 设置历史记录的最大数量
    /// </summary>
    /// <param name="maxCount">最大记录数量</param>
    member this.SetMaxHistoryCount(maxCount : int) = 
        let validMaxCount = Math.Max(100, Math.Min(10000, maxCount))
        records <- new List<InputRecord>(records)
        this.LimitHistorySize()
    
    /// <summary>
    /// 获取历史记录的当前数量
    /// </summary>
    /// <returns>历史记录数量</returns>
    member this.GetHistoryCount() : int = 
        records.Count