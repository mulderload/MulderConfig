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
            this.Text = config.Game.Name;
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
                // Launched via the wrapped original exe: apply operations then launch the real game.
                ApplyConfig(launchAfterApply: true);
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

            ApplyConfig(launchAfterApply: false);
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
        }

        private void ApplyConfig(bool launchAfterApply)
        {
            // Persist current selection (so future wrapped launches use it)
            saveManager.SaveChoices();

            var config = configProvider.GetConfig();
            var selected = formStateManager.GetChoices();
            selected["Addon"] = formStateManager.GetAddon();

            // Apply all operations first
            fileActionManager.ExecuteOperations(config.Actions.Operations, selected);

            // Then ensure wrapping is in place if launch actions exist
            if (config.Actions.Launch.Count > 0 && !exeWrapper.IsWrapped() && exeWrapper.CanWrap())
            {
                exeWrapper.Wrap();
            }

            if (launchAfterApply)
            {
                launchManager.Launch();
                return;
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
