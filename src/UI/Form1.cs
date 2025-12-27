using MulderConfig.src.Save;
using MulderConfig.src.Configuration;
using MulderConfig.src.Apply;

namespace MulderConfig.src.UI
{
    public partial class Form1 : Form
    {
        private readonly int? _steamAddonId;
        private readonly ConfigModel _config;
        private readonly ApplyManager _applyManager;
        private readonly FormBuilder _formBuilder;
        private readonly FormValidator _formValidator;
        private readonly FormSelectionProvider _formSelectionProvider;
        private readonly SaveLoader _saveLoader;
        private readonly SaveSaver _saveSaver;

        public Form1(
            int? steamAddonId,
            ConfigModel config,
            ApplyManager applyManager,
            FormBuilder formBuilder,
            FormValidator formValidator,
            FormSelectionProvider formSelectionProvider,
            SaveLoader saveLoader,
            SaveSaver saveSaver)
        {
            _steamAddonId = steamAddonId;
            _config = config;
            _applyManager = applyManager;
            _formBuilder = formBuilder;
            _formValidator = formValidator;
            _formSelectionProvider = formSelectionProvider;
            _saveLoader = saveLoader;
            _saveSaver = saveSaver;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = _config.Game.Name;
            _formBuilder.BuildAddons(_config, comboBoxAddon);

            if (_steamAddonId != null)
            {
                var match = _config.Addons.FirstOrDefault(a => a.SteamId == _steamAddonId);
                if (match != null)
                {
                    int index = comboBoxAddon.Items.IndexOf(match.Title);
                    if (index >= 0)
                        comboBoxAddon.SelectedIndex = index;
                }
            }

            _formBuilder.BuildForm(_config, panelOptions, UpdateButtons);
            LoadSavedChoices();
            UpdateButtons();
        }

        private void comboBoxAddon_SelectedIndexChanged(object sender, EventArgs e)
        {
            _formSelectionProvider.SetAddon(comboBoxAddon.SelectedItem?.ToString());
            LoadSavedChoices();
            _formValidator.ApplyWhenConstraints();
            UpdateButtons();
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (!_formValidator.IsValid())
            {
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplyConfig();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!_formValidator.IsValid())
            {
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _saveSaver.SaveChoices(_formSelectionProvider.GetAddon(), _formSelectionProvider.GetChoices());
            MessageBox.Show($"Configuration saved for {_formSelectionProvider.GetAddon()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyConfig()
        {
            _applyManager.Apply(_formSelectionProvider);

            MessageBox.Show("Applied.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSavedChoices()
        {
            var saved = _saveLoader.Load(_formSelectionProvider.GetAddon());
            _formSelectionProvider.ResetChoices();
            _formSelectionProvider.ApplyChoices(saved);
        }

        private void UpdateButtons()
        {
            var isValid = _formValidator.IsValid();
            btnLaunch.Enabled = isValid;
            btnSave.Enabled = isValid;
        }
    }
}
