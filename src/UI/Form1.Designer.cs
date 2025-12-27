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
            comboBoxAddon = new ComboBox();
            panelOptions = new Panel();
            btnApply = new Button();
            btnSave = new Button();
            SuspendLayout();
            // 
            // comboBoxAddon
            // 
            comboBoxAddon.BackColor = Color.FromArgb(89, 101, 119);
            comboBoxAddon.Font = new Font("Segoe UI", 9F);
            comboBoxAddon.ForeColor = SystemColors.HighlightText;
            comboBoxAddon.FormattingEnabled = true;
            comboBoxAddon.Location = new Point(12, 12);
            comboBoxAddon.Name = "comboBoxAddon";
            comboBoxAddon.Size = new Size(418, 23);
            comboBoxAddon.TabIndex = 0;
            comboBoxAddon.SelectedIndexChanged += comboBoxAddon_SelectedIndexChanged;
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
            Controls.Add(comboBoxAddon);
            Name = "Form1";
            Padding = new Padding(0, 0, 0, 20);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxAddon;
        private Panel panelOptions;
        private Button btnApply;
        private Button btnSave;
    }
}
