namespace MulderConfig.src.UI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            comboBoxTitle = new ComboBox();
            panelOptions = new Panel();
            btnApply = new Button();
            btnSave = new Button();
            SuspendLayout();
            // 
            // comboBoxTitle
            // 
            comboBoxTitle.BackColor = Color.FromArgb(89, 101, 119);
            comboBoxTitle.Font = new Font("Segoe UI", 9F);
            comboBoxTitle.ForeColor = SystemColors.HighlightText;
            comboBoxTitle.FormattingEnabled = true;
            comboBoxTitle.Location = new Point(12, 12);
            comboBoxTitle.Name = "comboBoxTitle";
            comboBoxTitle.Size = new Size(418, 23);
            comboBoxTitle.TabIndex = 0;
            comboBoxTitle.SelectedIndexChanged += comboBoxTitle_SelectedIndexChanged;
            // 
            // panelOptions
            // 
            panelOptions.AutoSize = true;
            panelOptions.BorderStyle = BorderStyle.FixedSingle;
            panelOptions.ForeColor = SystemColors.Control;
            panelOptions.Location = new Point(12, 53);
            panelOptions.Name = "panelOptions";
            panelOptions.Padding = new Padding(20);
            panelOptions.Size = new Size(600, 60);
            panelOptions.TabIndex = 1;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(537, 12);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(75, 23);
            btnApply.TabIndex = 2;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(436, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(95, 23);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save Config";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.FromArgb(35, 35, 45);
            ClientSize = new Size(624, 341);
            Controls.Add(btnSave);
            Controls.Add(btnApply);
            Controls.Add(panelOptions);
            Controls.Add(comboBoxTitle);
            Name = "Form1";
            Padding = new Padding(0, 0, 0, 20);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxTitle;
        private Panel panelOptions;
        private Button btnApply;
        private Button btnSave;
    }
}
