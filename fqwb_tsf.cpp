// fqwb_tsf.cpp - 反切五笔输入法TSF接口实现文件

#include "fqwb_tsf.h"
#include <fstream>
#include <algorithm>
#include <sstream>

// 定义输入法GUID
const GUID g_guidProfile = {
    0x12345678, 0x1234, 0x1234, {0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0}
};

const GUID g_guidInputMethod = {
    0x87654321, 0x4321, 0x4321, {0x0f, 0xed, 0xcb, 0xa9, 0x87, 0x65, 0x43, 0x21}
};

// dictionary_manager 类实现
dictionary_manager::dictionary_manager() : initialized(false), current_dict_name(L"default") {
}

dictionary_manager::~dictionary_manager() {
}

bool dictionary_manager::initialize(const std::wstring& dir_path) {
    data_dir = dir_path;
    initialized = true;
    
    try {
        // 首先尝试从Data目录加载所有.dic文件作为词库
        WIN32_FIND_DATAW findFileData;
        HANDLE hFind = FindFirstFileW((data_dir + L"\\*.dic").c_str(), &findFileData);
        
        if (hFind != INVALID_HANDLE_VALUE) {
            do {
                if (!(findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
                    std::wstring file_name = findFileData.cFileName;
                    std::wstring dict_name = file_name.substr(0, file_name.find_last_of(L'.'));
                    std::wstring file_path = data_dir + L"\\" + file_name;
                    
                    if (load_dictionary(dict_name, file_path)) {
                        if (dictionaries.size() == 1) {
                            // 如果是第一个加载的词库，自动切换到它
                            switch_dictionary(dict_name);
                        }
                    }
                }
            } while (FindNextFileW(hFind, &findFileData) != 0);
            
            FindClose(hFind);
        }
        
        // 如果没有加载到任何词库，创建一个默认词库
        if (dictionaries.empty()) {
            // 示例词库内容
            add_word(L"abc", L"测试");
            add_word(L"def", L"输入法");
            add_word(L"ghi", L"Windows");
            add_word(L"jkl", L"TSF");
            add_word(L"mno", L"风琴五笔");
            
            // 保存到默认词库
            dictionaries[current_dict_name] = dict;
        }
        
        return true;
    }
    catch (...) {
        return false;
    }
}

std::vector<std::wstring> dictionary_manager::search_code(const std::wstring& code) {
    std::vector<std::wstring> result;
    
    if (!initialized) {
        return result;
    }
    
    auto it = dict.find(code);
    if (it != dict.end()) {
        result = it->second;
    }
    
    return result;
}

bool dictionary_manager::add_word(const std::wstring& code, const std::wstring& characters) {
    if (!initialized) {
        return false;
    }
    
    dict[code].push_back(characters);
    
    // 同时更新当前词库在dictionaries中的副本
    if (!current_dict_name.empty()) {
        dictionaries[current_dict_name][code].push_back(characters);
    }
    
    return true;
}

bool dictionary_manager::save_user_dictionary() {
    if (!initialized) {
        return false;
    }
    
    try {
        // 实际应用中应该保存到文件
        // 这里我们简化处理，直接返回true
        return true;
    }
    catch (...) {
        return false;
    }
}

bool dictionary_manager::clear_user_dictionary() {
    if (!initialized) {
        return false;
    }
    
    try {
        // 实际应用中应该清除用户词库文件中的内容
        // 这里我们简化处理，直接返回true
        return true;
    }
    catch (...) {
        return false;
    }
}

std::vector<std::wstring> dictionary_manager::get_all_codes() {
    std::vector<std::wstring> result;
    
    if (!initialized) {
        return result;
    }
    
    for (const auto& pair : dict) {
        result.push_back(pair.first);
    }
    
    return result;
}

// 加载指定词库文件
bool dictionary_manager::load_dictionary(const std::wstring& dict_name, const std::wstring& file_path) {
    try {
        std::wifstream file(file_path);
        if (!file.is_open()) {
            return false;
        }
        
        std::map<std::wstring, std::vector<std::wstring>> new_dict;
        std::wstring line;
        
        while (std::getline(file, line)) {
            if (line.empty()) {
                continue;
            }
            
            // 查找空格分隔符
            size_t pos = line.find(L' ');
            if (pos != std::wstring::npos && pos > 0 && pos < line.size() - 1) {
                std::wstring code = line.substr(0, pos);
                std::wstring characters = line.substr(pos + 1);
                
                // 移除可能的空白字符
                code.erase(std::remove_if(code.begin(), code.end(), ::iswspace), code.end());
                characters.erase(std::remove_if(characters.begin(), characters.end(), ::iswspace), characters.end());
                
                if (!code.empty() && !characters.empty()) {
                    new_dict[code].push_back(characters);
                }
            }
        }
        
        file.close();
        
        if (!new_dict.empty()) {
            dictionaries[dict_name] = new_dict;
            return true;
        }
        
        return false;
    }
    catch (...) {
        return false;
    }
}

// 切换到指定词库
bool dictionary_manager::switch_dictionary(const std::wstring& dict_name) {
    if (!initialized || dictionaries.find(dict_name) == dictionaries.end()) {
        return false;
    }
    
    current_dict_name = dict_name;
    dict = dictionaries[dict_name];
    return true;
}

// 获取所有可用词库名称
std::vector<std::wstring> dictionary_manager::get_available_dictionaries() const {
    std::vector<std::wstring> result;
    
    if (!initialized) {
        return result;
    }
    
    for (const auto& pair : dictionaries) {
        result.push_back(pair.first);
    }
    
    return result;
}

// 获取当前词库名称
std::wstring dictionary_manager::get_current_dictionary() const {
    return current_dict_name;
}

// fqwb_input_method 类实现
fqwb_input_method::fqwb_input_method() : dict_manager(nullptr), initialized(false), auto_commit(true), shift_select(true), current_page(0), page_size(9) {
    dict_manager = new dictionary_manager();
}

fqwb_input_method::~fqwb_input_method() {
    if (dict_manager) {
        delete dict_manager;
        dict_manager = nullptr;
    }
}

bool fqwb_input_method::initialize(const std::wstring& data_dir) {
    if (dict_manager) {
        initialized = dict_manager->initialize(data_dir);
    }
    return initialized;
}

// 设置四码上屏功能
void fqwb_input_method::set_auto_commit(bool enable) {
    auto_commit = enable;
}

// 获取四码上屏功能状态
bool fqwb_input_method::get_auto_commit() const {
    return auto_commit;
}

// 设置是否启用Shift选择重码功能
void fqwb_input_method::set_shift_select(bool enable) {
    shift_select = enable;
}

// 获取Shift选择重码功能状态
bool fqwb_input_method::get_shift_select() const {
    return shift_select;
}

// 翻到下一页
void fqwb_input_method::next_page() {
    int total_pages = get_total_pages();
    if (current_page < total_pages - 1) {
        current_page++;
    }
}

// 翻到上一页
void fqwb_input_method::prev_page() {
    if (current_page > 0) {
        current_page--;
    }
}

// 获取当前页码
int fqwb_input_method::get_current_page() const {
    return current_page;
}

// 获取总页数
int fqwb_input_method::get_total_pages() const {
    if (page_size <= 0 || current_candidates.empty()) {
        return 1;
    }
    return (current_candidates.size() + page_size - 1) / page_size;
}

// 设置每页显示的候选词数量
void fqwb_input_method::set_page_size(int size) {
    if (size > 0) {
        page_size = size;
        current_page = 0; // 重置到第一页
    }
}

// 获取每页显示的候选词数量
int fqwb_input_method::get_page_size() const {
    return page_size;
}

// 获取当前页的候选词
std::vector<std::wstring> fqwb_input_method::get_current_page_candidates() const {
    std::vector<std::wstring> result;
    
    if (current_candidates.empty() || page_size <= 0) {
        return result;
    }
    
    int start_index = current_page * page_size;
    int end_index = std::min(start_index + page_size, static_cast<int>(current_candidates.size()));
    
    for (int i = start_index; i < end_index; i++) {
        result.push_back(current_candidates[i]);
    }
    
    return result;
}

bool fqwb_input_method::process_key_input(UINT key_code, LPARAM lParam, bool is_down, bool* handled) {
    if (!initialized || !handled) {
        *handled = false;
        return false;
    }
    
    *handled = true;
    
    // 获取Shift键状态
    bool shift_pressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
    
    // 处理按键输入
    if (is_down) {
        // 字母键（A-Z）
        if (key_code >= 'A' && key_code <= 'Z') {
            current_code += static_cast<wchar_t>(key_code);
            current_candidates = dict_manager->search_code(current_code);
            
            // 实现四码上屏功能
            if (auto_commit && current_code.length() == MAX_CODE_LENGTH && !current_candidates.empty()) {
                select_candidate(0);
            }
            
            return true;
        }
        // 数字键（1-9）- 用于选择候选词
        else if (key_code >= '1' && key_code <= '9') {
            int index = key_code - '1';
            
            // 如果启用了Shift选择重码功能，并且Shift键被按下，则选择下一页的候选词
            if (shift_select && shift_pressed) {
                // 假设每页显示9个候选词
                const int PAGE_SIZE = 9;
                index += PAGE_SIZE;
            }
            
            if (index < current_candidates.size()) {
                select_candidate(index);
            }
            return true;
        }
        // 退格键
        else if (key_code == VK_BACK) {
            if (!current_code.empty()) {
                current_code.pop_back();
                if (!current_code.empty()) {
                    current_candidates = dict_manager->search_code(current_code);
                } else {
                    current_candidates.clear();
                }
            }
            return true;
        }
        // ESC键 - 清除输入
        else if (key_code == VK_ESCAPE) {
            clear_input();
            return true;
        }
        // PageDown键 - 翻到下一页
        else if (key_code == VK_NEXT) {
            if (!current_candidates.empty()) {
                next_page();
            }
            return true;
        }
        // PageUp键 - 翻到上一页
        else if (key_code == VK_PRIOR) {
            if (!current_candidates.empty()) {
                prev_page();
            }
            return true;
        }
        // Enter键 - 确认输入
        else if (key_code == VK_RETURN) {
            if (!current_candidates.empty()) {
                select_candidate(0);
            }
            return true;
        }
        // 空格键 - 显示更多候选词或确认输入
        else if (key_code == VK_SPACE) {
            if (!current_candidates.empty()) {
                select_candidate(0);
            }
            return true;
        }
    }
    
    *handled = false;
    return true;
}

const std::vector<std::wstring>& fqwb_input_method::get_candidates() {
    return current_candidates;
}

std::wstring fqwb_input_method::select_candidate(int index) {
    if (index >= 0 && index < current_candidates.size()) {
        std::wstring selected = current_candidates[index];
        clear_input();
        return selected;
    }
    return L"";
}

void fqwb_input_method::clear_input() {
    current_code.clear();
    current_candidates.clear();
    current_page = 0; // 清除输入时重置到第一页
}

const std::wstring& fqwb_input_method::get_current_code() const {
    return current_code;
}

bool fqwb_input_method::add_user_word(const std::wstring& code, const std::wstring& characters) {
    if (!initialized || !dict_manager) {
        return false;
    }
    
    return dict_manager->add_word(code, characters);
}

// TSF文本服务类实现
class fqwb_text_service : public ITfTextInputProcessor, public ITfThreadMgrEventSink, public ITfKeyEventSink {
private:
    LONG ref_count;
    ITfThreadMgr* thread_mgr;
    DWORD thread_mgr_cookie;
    DWORD key_event_cookie;
    fqwb_input_method* input_method;
    bool is_active;

public:
    fqwb_text_service() : ref_count(1), thread_mgr(nullptr), thread_mgr_cookie(0), 
                         key_event_cookie(0), input_method(nullptr), is_active(false) {
        input_method = new fqwb_input_method();
    }
    
    ~fqwb_text_service() {
        if (input_method) {
            delete input_method;
            input_method = nullptr;
        }
    }
    
    // IUnknown 方法实现
    STDMETHODIMP QueryInterface(REFIID riid, void **ppvObj) {
        if (ppvObj == nullptr) {
            return E_INVALIDARG;
        }
        
        *ppvObj = nullptr;
        
        if (IsEqualIID(riid, IID_IUnknown) || IsEqualIID(riid, IID_ITfTextInputProcessor)) {
            *ppvObj = static_cast<ITfTextInputProcessor*>(this);
        }
        else if (IsEqualIID(riid, IID_ITfThreadMgrEventSink)) {
            *ppvObj = static_cast<ITfThreadMgrEventSink*>(this);
        }
        else if (IsEqualIID(riid, IID_ITfKeyEventSink)) {
            *ppvObj = static_cast<ITfKeyEventSink*>(this);
        }
        
        if (*ppvObj) {
            AddRef();
            return S_OK;
        }
        
        return E_NOINTERFACE;
    }
    
    STDMETHODIMP_(ULONG) AddRef() {
        return InterlockedIncrement(&ref_count);
    }
    
    STDMETHODIMP_(ULONG) Release() {
        ULONG ulCount = InterlockedDecrement(&ref_count);
        if (ulCount == 0) {
            delete this;
        }
        return ulCount;
    }
    
    // ITfTextInputProcessor 方法实现
    STDMETHODIMP Activate(ITfThreadMgr *pThreadMgr, TfClientId tfClientId) {
        if (pThreadMgr == nullptr) {
            return E_INVALIDARG;
        }
        
        thread_mgr = pThreadMgr;
        thread_mgr->AddRef();
        
        // 初始化输入法
        if (input_method) {
            // 获取应用程序数据目录
            WCHAR szModuleName[MAX_PATH];
            GetModuleFileNameW(nullptr, szModuleName, ARRAYSIZE(szModuleName));
            PathRemoveFileSpecW(szModuleName);
            std::wstring data_dir = szModuleName;
            data_dir += L"\\Data";
            
            input_method->initialize(data_dir);
        }
        
        // 注册线程管理器事件接收器
        if (SUCCEEDED(thread_mgr->AdviseSink(this, &IID_ITfThreadMgrEventSink, &thread_mgr_cookie))) {
            // 注册键盘事件接收器
            ITfSource *pSource = nullptr;
            if (SUCCEEDED(thread_mgr->QueryInterface(IID_ITfSource, (void **)&pSource))) {
                pSource->AdviseSink(IID_ITfKeyEventSink, this, &key_event_cookie);
                pSource->Release();
            }
        }
        
        is_active = true;
        return S_OK;
    }
    
    STDMETHODIMP Deactivate() {
        if (thread_mgr != nullptr) {
            // 取消注册事件接收器
            if (thread_mgr_cookie != 0) {
                thread_mgr->UnadviseSink(thread_mgr_cookie);
                thread_mgr_cookie = 0;
            }
            
            if (key_event_cookie != 0) {
                ITfSource *pSource = nullptr;
                if (SUCCEEDED(thread_mgr->QueryInterface(IID_ITfSource, (void **)&pSource))) {
                    pSource->UnadviseSink(key_event_cookie);
                    pSource->Release();
                }
                key_event_cookie = 0;
            }
            
            thread_mgr->Release();
            thread_mgr = nullptr;
        }
        
        is_active = false;
        return S_OK;
    }
    
    // ITfThreadMgrEventSink 方法实现
    STDMETHODIMP OnInitDocumentMgr(ITfDocumentMgr *pDocMgr) {
        return S_OK;
    }
    
    STDMETHODIMP OnUninitDocumentMgr(ITfDocumentMgr *pDocMgr) {
        return S_OK;
    }
    
    STDMETHODIMP OnSetFocus(ITfDocumentMgr *pDocMgrFocus, ITfDocumentMgr *pDocMgrPrevFocus) {
        return S_OK;
    }
    
    STDMETHODIMP OnPushContext(ITfContext *pContext) {
        return S_OK;
    }
    
    STDMETHODIMP OnPopContext(ITfContext *pContext) {
        return S_OK;
    }
    
    // ITfKeyEventSink 方法实现
    STDMETHODIMP OnKeyDown(ITfContext *pContext, WPARAM wParam, LPARAM lParam, BOOL *pfEaten) {
        if (!is_active || !input_method || !pfEaten) {
            *pfEaten = FALSE;
            return S_OK;
        }
        
        bool handled = false;
        if (input_method->process_key_input(wParam, lParam, true, &handled)) {
            *pfEaten = handled ? TRUE : FALSE;
        } else {
            *pfEaten = FALSE;
        }
        
        return S_OK;
    }
    
    STDMETHODIMP OnKeyUp(ITfContext *pContext, WPARAM wParam, LPARAM lParam, BOOL *pfEaten) {
        if (!is_active || !input_method || !pfEaten) {
            *pfEaten = FALSE;
            return S_OK;
        }
        
        bool handled = false;
        if (input_method->process_key_input(wParam, lParam, false, &handled)) {
            *pfEaten = handled ? TRUE : FALSE;
        } else {
            *pfEaten = FALSE;
        }
        
        return S_OK;
    }
    
    STDMETHODIMP OnTestKeyDown(ITfContext *pContext, WPARAM wParam, LPARAM lParam, BOOL *pfEaten) {
        *pfEaten = FALSE;
        return S_OK;
    }
    
    STDMETHODIMP OnTestKeyUp(ITfContext *pContext, WPARAM wParam, LPARAM lParam, BOOL *pfEaten) {
        *pfEaten = FALSE;
        return S_OK;
    }
};

// DllMain 函数 - 简化版本
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

// TSF接口导出函数
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppv) {
    if (!IsEqualCLSID(rclsid, g_guidInputMethod)) {
        return CLASS_E_CLASSNOTAVAILABLE;
    }
    
    if (ppv == nullptr) {
        return E_INVALIDARG;
    }
    
    *ppv = nullptr;
    
    fqwb_text_service *pTextService = new fqwb_text_service();
    if (pTextService == nullptr) {
        return E_OUTOFMEMORY;
    }
    
    HRESULT hr = pTextService->QueryInterface(riid, ppv);
    pTextService->Release();
    
    return hr;
}

STDAPI DllCanUnloadNow(void) {
    // 简化实现，实际应用中需要跟踪组件的引用计数
    return S_FALSE;
}

STDAPI DllRegisterServer(void) {
    // 简化实现，实际应用中需要注册输入法组件
    return S_OK;
}

STDAPI DllUnregisterServer(void) {
    // 简化实现，实际应用中需要注销输入法组件
    return S_OK;
}