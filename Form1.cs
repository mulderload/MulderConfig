using MulderLauncher.Services;

namespace MulderLauncher
{
    public partial class Form1 : Form
    {
        private readonly int? steamAddonId;
        private readonly ConfigProvider configProvider = new();
        private readonly ExeWrapper exeWrapper;
        private readonly FileActionManager fileActionManager = new();
        private readonly FormBuilder formBuilder;
        private readonly FormValidator formValidator;
        private readonly FormStateManager formStateManager;
        private readonly LaunchManager launchManager;
        private readonly SaveManager saveManager;

        public Form1(int? steamAddonId)
        {
            this.steamAddonId = steamAddonId;
            exeWrapper = new(configProvider);
            formStateManager = new(configProvider);
            formValidator = new(configProvider, formStateManager);
            formBuilder = new(formValidator, formStateManager);
            launchManager = new(configProvider, formStateManager);
            saveManager = new(configProvider, fileActionManager, formStateManager);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var config = configProvider.GetConfig();
            this.Text = config.Game.Title;
            formBuilder.BuildAddons(config, comboBoxAddon);

            if (steamAddonId != null)
            {
                var match = config.Addons.FirstOrDefault(a => a.SteamId == steamAddonId);
                if (match != null)
                {
                    int index = comboBoxAddon.Items.IndexOf(match.Title);
                    if (index >= 0)
                    {
                        comboBoxAddon.SelectedIndex = index;
                    }
                }
            }

            formBuilder.BuildForm(config, panelOptions, UpdateButtons);
            saveManager.LoadChoices();
            UpdateButtons();

            if (exeWrapper.IsWrapping())
            {
                btnLaunch.PerformClick();
                Application.Exit();
            }
        }

        private void comboBoxAddon_SelectedIndexChanged(object sender, EventArgs e)
        {
            formStateManager.SetAddon(comboBoxAddon.SelectedItem?.ToString());
            saveManager.LoadChoices();
            formValidator.ApplyWhenConstraints();
            UpdateButtons();
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (!formValidator.IsValid())
            {
                // Should not happen since button is disabled
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            launchManager.Launch();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!formValidator.IsValid())
            {
                // Should not happen since button is disabled
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saveManager.SaveChoices();
            MessageBox.Show($"Configuration saved for {formStateManager.GetAddon()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (!exeWrapper.IsWrapped())
            {
                if (exeWrapper.CanWrap())
                {
                    exeWrapper.Wrap();
                }
            }
        }

        private void UpdateButtons()
        {
            var isValid = formValidator.IsValid();
            btnLaunch.Enabled = isValid;
            btnSave.Enabled = isValid;
        }
    }
}
