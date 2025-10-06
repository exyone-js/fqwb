// fqwb_tsf_example.cpp - 反切五笔输入法TSF接口使用示例

#include "fqwb_tsf.h"
#include <iostream>
#include <string>

// 辅助函数：将宽字符串转换为UTF-8字符串
std::string wstring_to_string(const std::wstring& wstr) {
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
    std::string str(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &str[0], size_needed, NULL, NULL);
    return str;
}

int main() {
    std::cout << "反切五笔输入法TSF接口使用示例\n";
    std::cout << "============================\n";

    // 创建输入法实例
    fqwb_input_method* input_method = new fqwb_input_method();
    if (!input_method) {
        std::cerr << "创建输入法实例失败\n";
        return 1;
    }

    // 初始化输入法
    // 获取当前目录作为数据目录
    WCHAR current_dir[MAX_PATH];
    GetCurrentDirectoryW(MAX_PATH, current_dir);
    std::wstring data_dir = current_dir;
    data_dir += L"\\Data";

    if (!input_method->initialize(data_dir)) {
        std::cerr << "初始化输入法失败\n";
        delete input_method;
        return 1;
    }

    std::cout << "输入法初始化成功，数据目录：" << wstring_to_string(data_dir) << "\n";
    std::cout << "按ESC键退出程序\n";
    std::cout << "输入编码以测试输入法功能：\n";

    // 简单的命令行交互
    bool running = true;
    while (running) {
        // 等待用户按键
        if (_kbhit()) {
            int key = _getch();

            // ESC键退出
            if (key == 27) {
                running = false;
                break;
            }

            // 处理按键输入
            bool handled = false;
            input_method->process_key_input(key, true, &handled);

            if (handled) {
                // 显示当前编码
                std::cout << "\r当前编码: " << wstring_to_string(input_method->get_current_code()) << "\t";

                // 显示候选词
                const std::vector<std::wstring>& candidates = input_method->get_candidates();
                if (!candidates.empty()) {
                    std::cout << "候选词: ";
                    for (size_t i = 0; i < candidates.size(); ++i) {
                        std::cout << (i + 1) << ")" << wstring_to_string(candidates[i]) << " ";
                    }
                }
                std::cout << "\n";
            }
        }

        // 小延迟，避免CPU占用过高
        Sleep(10);
    }

    // 清理资源
    delete input_method;

    std::cout << "程序已退出\n";
    return 0;
}