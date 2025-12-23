using MulderLauncher.Services;

namespace MulderLauncher.UI
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
            launchManager = new(configProvider, formStateManager, exeWrapper);
            saveManager = new(formStateManager);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var config = configProvider.GetConfig();
            Text = config.Game.Name;
            formBuilder.BuildAddons(config, comboBoxAddon);

            if (steamAddonId != null)
            {
                var match = config.Addons.FirstOrDefault(a => a.SteamId == steamAddonId);
                if (match != null)
                {
                    int index = comboBoxAddon.Items.IndexOf(match.Title);
                    if (index >= 0)
                        comboBoxAddon.SelectedIndex = index;
                }
            }

            formBuilder.BuildForm(config, panelOptions, UpdateButtons);
            saveManager.LoadChoices();
            UpdateButtons();
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
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplyConfig();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!formValidator.IsValid())
            {
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saveManager.SaveChoices();
            MessageBox.Show($"Configuration saved for {formStateManager.GetAddon()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyConfig()
        {
            saveManager.SaveChoices();

            var config = configProvider.GetConfig();
            var selected = formStateManager.GetChoices();
            selected["Addon"] = formStateManager.GetAddon();

            fileActionManager.ExecuteOperations(config.Actions.Operations, selected);

            if (config.Actions.Launch.Count > 0 && !exeWrapper.IsWrapped() && exeWrapper.CanWrap())
            {
                exeWrapper.Wrap();
            }

            MessageBox.Show("Applied.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateButtons()
        {
            var isValid = formValidator.IsValid();
            btnLaunch.Enabled = isValid;
            btnSave.Enabled = isValid;
        }
    }
}
