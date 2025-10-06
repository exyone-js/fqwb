using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using fqwb.Config;
using fqwb.FuzzySound;
using fqwb.History;

namespace fqwb
{
    /// <summary>
    /// 输入法主界面类，负责处理用户输入和显示候选词
    /// </summary>
    public partial class input : Form
    {
        // 核心服务与管理器
        private Dictionary.DictionaryService dictionaryService; // 词库服务实例
        private ConfigManager configManager;                  // 配置管理器
        private FuzzySoundProcessor fuzzySoundProcessor;      // 模糊音处理器
        private HistoryManager historyManager;                // 历史记录管理器
        
        // 输入状态管理
        private string currentCode = "";                      // 当前输入的编码
        private List<string> currentCandidates = new List<string>(); // 当前候选词列表
        
        // UI组件
        private Panel candidatePanel;                         // 候选框面板
        private TextBox candidateTextBox;                     // 候选词显示文本框
        
        // 窗口拖动常量
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        /// <summary>
        /// 构造函数，初始化组件和输入法
        /// </summary>
        public input()
        {
            InitializeComponent();
            InitializeInputMethod();
        }

        /// <summary>
        /// 初始化输入法的所有组件和配置
        /// </summary>
        private void InitializeInputMethod()
        {
            // 初始化核心服务
            dictionaryService = new Dictionary.DictionaryService();
            
            // 获取相关管理器实例
            configManager = dictionaryService.GetConfigManager();
            fuzzySoundProcessor = dictionaryService.GetFuzzySoundProcessor();
            historyManager = dictionaryService.GetHistoryManager();
            
            // 初始化词库
            string dictionaryPath = Path.Combine(Application.StartupPath, "data");
            dictionaryService.Initialize(dictionaryPath);
            
            // 设置窗口属性
            InitializeWindowProperties();
            
            // 创建UI组件
            CreateUIComponents();
        }

        /// <summary>
        /// 设置窗口基本属性
        /// </summary>
        private void InitializeWindowProperties()
        {
            this.KeyPreview = true;
            this.KeyPress += inputForm_KeyPress;
            this.KeyDown += inputForm_KeyDown;
            this.FormBorderStyle = FormBorderStyle.None; // 无边框
            this.BackColor = Color.LightYellow; // 设置背景色为浅黄色
            this.TransparencyKey = Color.Red; // 设置透明色
            this.StartPosition = FormStartPosition.Manual; // 手动控制位置
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - 400, 
                                     Screen.PrimaryScreen.Bounds.Height - 150); // 底部居中
            this.ClientSize = new Size(800, 140); // 窗口大小
            this.TopMost = true; // 始终置顶
            this.MouseDown += Form_MouseDown; // 窗口拖动事件
        }

        /// <summary>
        /// 创建所有UI组件
        /// </summary>
        private void CreateUIComponents()
        {
            // 创建主候选框面板
            candidatePanel = new Panel
            {
                Name = "candidatePanel",
                Location = new Point(5, 5),
                Size = new Size(790, 130),
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true
            };
            this.Controls.Add(candidatePanel);
            
            // 添加编码显示标签
            Label codeLabel = new Label
            {
                Name = "codeLabel",
                Location = new Point(10, 10),
                Size = new Size(770, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Text = "输入编码: "
            };
            candidatePanel.Controls.Add(codeLabel);
            
            // 添加候选词显示文本框
            candidateTextBox = new TextBox
            {
                Name = "candidateTextBox",
                Location = new Point(10, 35),
                Size = new Size(770, 40),
                ReadOnly = true,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.None,
                Font = new Font("SimSun", 14),
                TextAlign = HorizontalAlignment.Left
            };
            candidatePanel.Controls.Add(candidateTextBox);
            
            // 添加词库选择器
            AddDictionarySelector();
            
            // 添加功能控制区域
            AddFunctionControls();
            
            // 添加状态栏
            AddStatusBar();
        }

        /// <summary>
        /// 添加词库选择器组件
        /// </summary>
        private void AddDictionarySelector()
        {
            // 词库标签
            Label dictLabel = new Label
            {
                Name = "dictLabel",
                Location = new Point(10, 80),
                Size = new Size(60, 20),
                Text = "词库: ",
                Font = new Font("SimSun", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            candidatePanel.Controls.Add(dictLabel);
            
            // 词库下拉框
            ComboBox dictComboBox = new ComboBox
            {
                Name = "dictComboBox",
                Location = new Point(70, 80),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("SimSun", 9),
                BackColor = Color.White
            };
            
            // 填充词库列表
            List<string> dictionaries = dictionaryService.GetAvailableDictionaries();
            if (dictionaries != null && dictionaries.Count > 0)
            {
                dictComboBox.Items.AddRange(dictionaries.ToArray());
                string currentDict = dictionaryService.GetCurrentDictionary();
                if (!string.IsNullOrEmpty(currentDict))
                {
                    dictComboBox.SelectedItem = currentDict;
                }
            }
            
            // 添加词库切换事件处理
            dictComboBox.SelectedIndexChanged += dictComboBox_SelectedIndexChanged;
            candidatePanel.Controls.Add(dictComboBox);
        }

        /// <summary>
        /// 添加功能控制区域
        /// </summary>
        private void AddFunctionControls()
        {
            // 创建控制区域面板
            FlowLayoutPanel controlsPanel = new FlowLayoutPanel
            {
                Name = "controlsPanel",
                Location = new Point(230, 80),
                Size = new Size(550, 20),
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            candidatePanel.Controls.Add(controlsPanel);
            
            // 添加模糊音开关
            CheckBox fuzzySoundCheckBox = new CheckBox
            {
                Name = "fuzzySoundCheckBox",
                Text = "模糊音",
                Checked = fuzzySoundProcessor.IsEnabled(),
                Font = new Font("SimSun", 9)
            };
            fuzzySoundCheckBox.CheckedChanged += fuzzySoundCheckBox_CheckedChanged;
            controlsPanel.Controls.Add(fuzzySoundCheckBox);
            
            // 添加历史记录开关
            CheckBox historyCheckBox = new CheckBox
            {
                Name = "historyCheckBox",
                Text = "历史记录",
                Checked = historyManager.IsEnabled(),
                Font = new Font("SimSun", 9)
            };
            historyCheckBox.CheckedChanged += historyCheckBox_CheckedChanged;
            controlsPanel.Controls.Add(historyCheckBox);
            
            // 添加设置按钮
            Button settingsButton = new Button
            {
                Name = "settingsButton",
                Text = "设置",
                Size = new Size(50, 20),
                Font = new Font("SimSun", 9)
            };
            settingsButton.Click += settingsButton_Click;
            controlsPanel.Controls.Add(settingsButton);
        }

        /// <summary>
        /// 添加状态栏显示操作提示
        /// </summary>
        private void AddStatusBar()
        {
            Label statusLabel = new Label
            {
                Name = "statusLabel",
                Location = new Point(10, 105),
                Size = new Size(770, 20),
                Font = new Font("SimSun", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray
            };
            
            // 获取配置中的快捷键设置
            var hotkeys = configManager.GetHotkeysConfig();
            statusLabel.Text = $"空格键显示候选词，数字键1-9选择，{hotkeys.ClearInput}清除，{hotkeys.ToggleLanguage}切换输入法";
            
            candidatePanel.Controls.Add(statusLabel);
        }

        /// <summary>
        /// 模糊音开关事件处理
        /// </summary>
        private void fuzzySoundCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            fuzzySoundProcessor.SetEnabled(checkBox.Checked);
            configManager.SaveConfig();
        }

        /// <summary>
        /// 历史记录开关事件处理
        /// </summary>
        private void historyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            historyManager.SetEnabled(checkBox.Checked);
            configManager.SaveConfig();
        }

        /// <summary>
        /// 设置按钮点击事件处理
        /// </summary>
        private void settingsButton_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.Initialize(configManager, fuzzySoundProcessor, historyManager);
            settingsForm.ShowDialog();
        }

        /// <summary>
        /// 鼠标按下事件处理 - 实现无边框窗口的拖动
        /// </summary>
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
        
        // Windows API导入用于无边框窗口拖动
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        /// <summary>
        /// 重写WndProc以支持无边框窗口拖动
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HTCAPTION);
        }
        
        /// <summary>
        /// 词库选择下拉框事件处理
        /// </summary>
        private void dictComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox dictComboBox = sender as ComboBox;
            Label statusLabel = candidatePanel.Controls["statusLabel"] as Label;
            
            if (dictComboBox != null && dictComboBox.SelectedItem != null)
            {
                string selectedDict = dictComboBox.SelectedItem.ToString();
                if (dictionaryService.SwitchDictionary(selectedDict))
                {
                    // 切换成功，更新状态栏信息
                    statusLabel.Text = $"已切换到词库: {selectedDict} | 空格键显示候选词，数字键1-9选择，ESC清除，Ctrl+Shift切换输入法";
                    
                    // 清空当前输入，确保使用新词库
                    ResetInputState();
                }
                else
                {
                    // 切换失败
                    statusLabel.Text = $"切换词库失败: {selectedDict} | 空格键显示候选词，数字键1-9选择，ESC清除，Ctrl+Shift切换输入法";
                }
            }
        }
        
        /// <summary>
        /// 处理按键输入事件
        /// </summary>
        private void inputForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 忽略控制字符
            if (char.IsControl(e.KeyChar))
                return;
            
            Label codeLabel = candidatePanel.Controls["codeLabel"] as Label;
            
            // 空格键显示候选词
            if (e.KeyChar == ' ')
            {
                e.Handled = true;
                if (!string.IsNullOrEmpty(currentCode))
                {
                    DisplayCandidates();
                }
                return;
            }
            
            // 退格键删除最后一个字符
            if (e.KeyChar == '\b')
            {
                e.Handled = true;
                if (currentCode.Length > 0)
                {
                    currentCode = currentCode.Substring(0, currentCode.Length - 1);
                    codeLabel.Text = "输入编码: " + currentCode;
                    candidateTextBox.Text = "";
                }
                return;
            }
            
            // 只接受字母和数字作为编码
            if (char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
                currentCode += char.ToLower(e.KeyChar);
                codeLabel.Text = "输入编码: " + currentCode;
                candidateTextBox.Text = "";
            }
        }
        
        /// <summary>
        /// 处理特殊按键事件
        /// </summary>
        private void inputForm_KeyDown(object sender, KeyEventArgs e)
        {
            Label codeLabel = candidatePanel.Controls["codeLabel"] as Label;
            
            // ESC键清除当前输入
            if (e.KeyCode == Keys.Escape)
            {
                ResetInputState();
            }
            // Enter键确认输入（选择第一个候选词）
            else if (e.KeyCode == Keys.Enter)
            {
                if (currentCandidates.Count > 0)
                {
                    InsertCandidate(0);
                }
            }
            // 数字键1-9选择候选词
            else if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
            {
                int index = e.KeyCode - Keys.D1;
                if (index < currentCandidates.Count)
                {
                    InsertCandidate(index);
                }
                e.Handled = true;
            }
            // 减号键翻页（模拟五笔加加的翻页功能）
            else if (e.KeyCode == Keys.OemMinus && currentCandidates.Count > 0)
            {
                // 这里仅为示例，实际应用中可以实现完整的翻页功能
                MessageBox.Show("翻页功能: 在实际应用中可以实现向前/向后翻页");
                e.Handled = true;
            }
        }
        
        /// <summary>
        /// 显示候选词列表
        /// </summary>
        private void DisplayCandidates()
        {
            string[] candidates = dictionaryService.SearchCode(currentCode).ToArray();
            
            if (candidates.Length > 0)
            {
                currentCandidates.Clear();
                currentCandidates.AddRange(candidates);
                
                // 构建候选词字符串
                string candidateText = currentCode + " ";
                for (int i = 0; i < Math.Min(9, candidates.Length); i++)
                {
                    candidateText += $"{i + 1}.{candidates[i]} ";
                }
                
                candidateTextBox.Text = candidateText;
            }
        }
        
        /// <summary>
        /// 插入指定索引的候选词到系统剪贴板，模拟输入法输入
        /// </summary>
        private void InsertCandidate(int index)
        {
            if (index >= 0 && index < currentCandidates.Count)
            {
                string selectedChar = currentCandidates[index];
                
                // 将选中的文本复制到剪贴板
                Clipboard.SetText(selectedChar);
                
                // 显示提示信息
                candidateTextBox.Text = "已选择: " + selectedChar + " (已复制到剪贴板)";
                
                // 记录到历史记录
                historyManager.RecordInput(currentCode, selectedChar);
            }
            
            // 清空当前编码和候选词
            ResetInputState();
        }
        
        /// <summary>
        /// 重置输入状态
        /// </summary>
        private void ResetInputState()
        {
            currentCode = "";
            currentCandidates.Clear();
            
            Label codeLabel = candidatePanel.Controls["codeLabel"] as Label;
            if (codeLabel != null)
            {
                codeLabel.Text = "输入编码: " + currentCode;
            }
            
            candidateTextBox.Text = "";
        }
    }
}
