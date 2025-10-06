// fqwb_tsf.h - 反切五笔输入法TSF接口头文件
// 提供基于Windows TSF框架的输入法功能

#ifndef FQWB_TSF_H
#define FQWB_TSF_H

#include <windows.h>
#include <tchar.h>
#include <msctf.h>
#include <vector>
#include <string>
#include <map>

// 定义输入法GUID
extern const GUID g_guidProfile;      // 输入法配置文件GUID
extern const GUID g_guidInputMethod;  // 输入法GUID

// 词库数据结构
struct dictionary_entry {
    std::wstring code;       // 输入编码
    std::wstring characters; // 对应的汉字或词组
};

// 词库管理器类
class dictionary_manager {
private:
    std::map<std::wstring, std::vector<std::wstring>> dict; // 当前词库：编码到汉字的映射
    std::map<std::wstring, std::map<std::wstring, std::vector<std::wstring>>> dictionaries; // 所有词库
    bool initialized;                                       // 是否已初始化
    std::wstring data_dir;                                  // 词库数据目录
    std::wstring current_dict_name;                         // 当前词库名称

public:
    dictionary_manager();
    ~dictionary_manager();

    // 初始化词库
    bool initialize(const std::wstring& dir_path);

    // 搜索编码对应的汉字
    std::vector<std::wstring> search_code(const std::wstring& code);

    // 添加新词到词库
    bool add_word(const std::wstring& code, const std::wstring& characters);

    // 保存用户词库
    bool save_user_dictionary();

    // 清除用户词库
    bool clear_user_dictionary();

    // 获取所有编码
    std::vector<std::wstring> get_all_codes();
    
    // 加载指定词库文件
    bool load_dictionary(const std::wstring& dict_name, const std::wstring& file_path);
    
    // 切换到指定词库
    bool switch_dictionary(const std::wstring& dict_name);
    
    // 获取所有可用词库名称
    std::vector<std::wstring> get_available_dictionaries() const;
    
    // 获取当前词库名称
    std::wstring get_current_dictionary() const;
};

// 输入法核心类
class fqwb_input_method {
private:
    dictionary_manager* dict_manager; // 词库管理器
    std::wstring current_code;        // 当前输入的编码
    std::vector<std::wstring> current_candidates; // 当前候选词列表
    bool initialized;                 // 是否已初始化
    bool auto_commit;                 // 是否启用四码上屏功能
    bool shift_select;                // 是否启用Shift选择重码功能
    int current_page;                 // 当前页码
    int page_size;                    // 每页显示的候选词数量
    static const int MAX_CODE_LENGTH = 4; // 最大编码长度（四码上屏）

public:
    fqwb_input_method();
    ~fqwb_input_method();

    // 初始化输入法
    bool initialize(const std::wstring& data_dir);

    // 处理按键输入
    bool process_key_input(UINT key_code, LPARAM lParam, bool is_down, bool* handled);

    // 获取当前候选词列表
    const std::vector<std::wstring>& get_candidates();

    // 选择候选词
    std::wstring select_candidate(int index);

    // 清除当前输入
    void clear_input();

    // 获取当前输入编码
    const std::wstring& get_current_code() const;

    // 添加用户自定义词汇
    bool add_user_word(const std::wstring& code, const std::wstring& characters);
    
    // 设置四码上屏功能
    void set_auto_commit(bool enable);
    
    // 获取四码上屏功能状态
    bool get_auto_commit() const;
    
    // 设置是否启用Shift选择重码功能
    void set_shift_select(bool enable);
    
    // 获取Shift选择重码功能状态
    bool get_shift_select() const;
    
    // 翻到下一页
    void next_page();
    
    // 翻到上一页
    void prev_page();
    
    // 获取当前页码
    int get_current_page() const;
    
    // 获取总页数
    int get_total_pages() const;
    
    // 设置每页显示的候选词数量
    void set_page_size(int size);
    
    // 获取每页显示的候选词数量
    int get_page_size() const;
    
    // 获取当前页的候选词
    std::vector<std::wstring> get_current_page_candidates() const;
};

// TSF文本服务类的前向声明
class fqwb_text_service;

#endif // FQWB_TSF_H