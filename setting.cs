using System;
using System.Windows.Forms;
using fqwb.Config;
using fqwb.FuzzySound;
using fqwb.History;

namespace fqwb
{
    /// <summary>
    /// 设置界面表单，用于配置输入法的各项功能
    /// </summary>
    public partial class SettingsForm : Form
    {
        // 配置管理器和处理器
        private ConfigManager configManager;          // 配置管理器
        private FuzzySoundProcessor fuzzySoundProcessor;  // 模糊音处理器
        private HistoryManager historyManager;        // 历史记录管理器
        
        // 配置数据
        private InputMethodConfig config;             // 当前配置数据

        /// <summary>
        /// 构造函数，初始化设置表单
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// 初始化设置表单的数据和控件
        /// </summary>
        public void Initialize(ConfigManager cm, FuzzySoundProcessor fsp, HistoryManager hm)
        {
            // 设置管理器引用
            configManager = cm;
            fuzzySoundProcessor = fsp;
            historyManager = hm;
            
            // 加载当前配置
            LoadCurrentConfig();
        }

        /// <summary>
        /// 加载当前配置数据到界面控件
        /// </summary>
        private void LoadCurrentConfig()
        {
            // 获取当前配置
            config = configManager.GetConfig();
            
            // 加载基本设置
            chkFuzzySound.Checked = fuzzySoundProcessor.IsEnabled();
            chkHistory.Enabled = true;
            chkHistory.Checked = historyManager.IsEnabled();
            
            // 加载快捷键设置
            txtClearInput.Text = config.Hotkeys.ClearInput;
            txtSwitchInputMethod.Text = config.Hotkeys.ToggleLanguage;
            txtPageUp.Text = config.Hotkeys.PageUp;
            txtPageDown.Text = config.Hotkeys.PageDown;
        }

        /// <summary>
        /// 保存当前配置到配置文件
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                // 保存基本设置
                fuzzySoundProcessor.SetEnabled(chkFuzzySound.Checked);
                historyManager.SetEnabled(chkHistory.Checked);
                
                // 更新快捷键配置
                config.Hotkeys.ClearInput = txtClearInput.Text.Trim();
                config.Hotkeys.ToggleLanguage = txtSwitchInputMethod.Text.Trim();
                config.Hotkeys.PageUp = txtPageUp.Text.Trim();
                config.Hotkeys.PageDown = txtPageDown.Text.Trim();
                
                // 保存配置
                configManager.SetConfig(config);
                configManager.SaveConfig();
                
                // 显示保存成功消息
                MessageBox.Show("设置已成功保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // 显示保存失败消息
                MessageBox.Show("保存设置时出错: " + ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化表单组件
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 表单基本设置
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = "设置 - 风琴五笔输入法";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            
            // 创建标签页控件
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            
            // 添加基本设置标签页
            TabPage basicTab = CreateBasicSettingsTab();
            
            // 添加快捷键设置标签页
            TabPage hotkeyTab = CreateHotkeySettingsTab();
            
            // 添加标签页到标签页控件
            tabControl.TabPages.Add(basicTab);
            tabControl.TabPages.Add(hotkeyTab);
            
            // 创建底部按钮区域
            Panel buttonPanel = CreateButtonPanel();
            
            // 添加控件到表单
            this.Controls.Add(tabControl);
            this.Controls.Add(buttonPanel);
            
            this.ResumeLayout(false);
        }

        /// <summary>
        /// 创建基本设置标签页
        /// </summary>
        private TabPage CreateBasicSettingsTab()
        {
            TabPage tab = new TabPage("基本设置");
            tab.Padding = new Padding(10);
            
            // 创建表格布局面板
            TableLayoutPanel layoutPanel = new TableLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.ColumnCount = 1;
            layoutPanel.RowCount = 2;
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            
            // 添加模糊音复选框
            chkFuzzySound = new CheckBox();
            chkFuzzySound.Text = "启用模糊音功能";
            chkFuzzySound.AutoSize = true;
            chkFuzzySound.Dock = DockStyle.Left;
            layoutPanel.Controls.Add(chkFuzzySound, 0, 0);
            
            // 添加历史记录复选框
            chkHistory = new CheckBox();
            chkHistory.Text = "启用历史记录功能";
            chkHistory.AutoSize = true;
            chkHistory.Dock = DockStyle.Left;
            layoutPanel.Controls.Add(chkHistory, 0, 1);
            
            tab.Controls.Add(layoutPanel);
            return tab;
        }

        /// <summary>
        /// 创建快捷键设置标签页
        /// </summary>
        private TabPage CreateHotkeySettingsTab()
        {
            TabPage tab = new TabPage("快捷键设置");
            tab.Padding = new Padding(10);
            
            // 创建表格布局面板
            TableLayoutPanel layoutPanel = new TableLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.ColumnCount = 2;
            layoutPanel.RowCount = 4;
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            // 添加清除输入快捷键设置
            AddHotkeyRow(layoutPanel, 0, "清除输入:", ref txtClearInput);
            
            // 添加切换输入法快捷键设置
            AddHotkeyRow(layoutPanel, 1, "切换输入法:", ref txtSwitchInputMethod);
            
            // 添加候选词向上翻页快捷键设置
            AddHotkeyRow(layoutPanel, 2, "候选词上翻:", ref txtPageUp);
            
            // 添加候选词向下翻页快捷键设置
            AddHotkeyRow(layoutPanel, 3, "候选词下翻:", ref txtPageDown);
            
            tab.Controls.Add(layoutPanel);
            return tab;
        }

        /// <summary>
        /// 添加快捷键设置行
        /// </summary>
        private void AddHotkeyRow(TableLayoutPanel panel, int row, string labelText, ref TextBox textBox)
        {
            // 创建标签
            Label label = new Label();
            label.Text = labelText;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            label.Dock = DockStyle.Fill;
            panel.Controls.Add(label, 0, row);
            
            // 创建文本框
            textBox = new TextBox();
            textBox.Dock = DockStyle.Fill;
            panel.Controls.Add(textBox, 1, row);
        }

        /// <summary>
        /// 创建底部按钮区域
        /// </summary>
        private Panel CreateButtonPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 40;
            panel.Padding = new Padding(10, 5, 10, 5);
            
            // 创建保存按钮
            btnSave = new Button();
            btnSave.Text = "保存";
            btnSave.Size = new System.Drawing.Size(75, 25);
            btnSave.Location = new System.Drawing.Point(170, 5);
            btnSave.Click += btnSave_Click;
            
            // 创建取消按钮
            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.Location = new System.Drawing.Point(255, 5);
            btnCancel.Click += btnCancel_Click;
            
            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnCancel);
            
            return panel;
        }

        /// <summary>
        /// 保存按钮点击事件处理
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        /// <summary>
        /// 取消按钮点击事件处理
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // UI控件声明
        private CheckBox chkFuzzySound;
        private CheckBox chkHistory;
        private TextBox txtClearInput;
        private TextBox txtSwitchInputMethod;
        private TextBox txtPageUp;
        private TextBox txtPageDown;
        private Button btnSave;
        private Button btnCancel;
    }
}