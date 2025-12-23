using MulderLauncher.Pipeline;
using MulderLauncher.Config;
using MulderLauncher.Save;

namespace MulderLauncher.UI
{
    public partial class Form1 : Form
    {
        private readonly int? steamAddonId;
        private readonly ConfigProvider configProvider;
        private readonly ApplyManager applyManager;
        private readonly FormBuilder formBuilder;
        private readonly FormValidator formValidator;
        private readonly FormSelectionProvider formSelectionProvider;
        private readonly SaveManager saveManager;

        public Form1(
            int? steamAddonId,
            ConfigProvider configProvider,
            ApplyManager applyManager,
            FormBuilder formBuilder,
            FormValidator formValidator,
            FormSelectionProvider formSelectionProvider,
            SaveManager saveManager)
        {
            this.steamAddonId = steamAddonId;
            this.configProvider = configProvider;
            this.applyManager = applyManager;
            this.formBuilder = formBuilder;
            this.formValidator = formValidator;
            this.formSelectionProvider = formSelectionProvider;
            this.saveManager = saveManager;

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
            LoadSavedChoices();
            UpdateButtons();
        }

        private void comboBoxAddon_SelectedIndexChanged(object sender, EventArgs e)
        {
            formSelectionProvider.SetAddon(comboBoxAddon.SelectedItem?.ToString());
            LoadSavedChoices();
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

            saveManager.SaveChoices(formSelectionProvider.GetAddon(), formSelectionProvider.GetChoices());
            MessageBox.Show($"Configuration saved for {formSelectionProvider.GetAddon()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyConfig()
        {
            applyManager.Apply(formSelectionProvider, persistSelections: true);

            MessageBox.Show("Applied.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSavedChoices()
        {
            var saved = saveManager.LoadChoices(formSelectionProvider.GetAddon());
            formSelectionProvider.ResetChoices();
            formSelectionProvider.ApplyChoices(saved);
        }

        private void UpdateButtons()
        {
            var isValid = formValidator.IsValid();
            btnLaunch.Enabled = isValid;
            btnSave.Enabled = isValid;
        }
    }
}
