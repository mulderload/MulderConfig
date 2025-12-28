using MulderConfig.Save;
using MulderConfig.Configuration;
using MulderConfig.Apply;

namespace MulderConfig.UI
{
    public partial class Form1 : Form
    {
        private readonly string _title;
        private readonly ConfigModel _config;
        private readonly ApplyManager _applyManager;
        private readonly FormBuilder _formBuilder;
        private readonly FormValidator _formValidator;
        private readonly FormSelectionProvider _formSelectionProvider;
        private readonly FormController _formController;
        private readonly SaveLoader _saveLoader;
        private readonly SaveSaver _saveSaver;
        private bool _isInitializing;

        public Form1(
            string title,
            ConfigModel config,
            ApplyManager applyManager,
            FormBuilder formBuilder,
            FormValidator formValidator,
            FormSelectionProvider formSelectionProvider,
            SaveLoader saveLoader,
            SaveSaver saveSaver)
        {
            _title = title;
            _config = config;
            _applyManager = applyManager;
            _formBuilder = formBuilder;
            _formValidator = formValidator;
            _formSelectionProvider = formSelectionProvider;
            _saveLoader = saveLoader;
            _saveSaver = saveSaver;

            InitializeComponent();

            _formController = new FormController(_formSelectionProvider, _formValidator, btnApply, btnSave);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _isInitializing = true;

            Text = _config.Game.Title;
            _formBuilder.BuildComboBox(_config, comboBoxTitle);

            var initialIndex = comboBoxTitle.Items.IndexOf(_title);
            if (initialIndex >= 0)
            {
                comboBoxTitle.SelectedIndex = initialIndex;
            }

            _formSelectionProvider.SetTitle(comboBoxTitle.SelectedItem?.ToString() ?? "default");

            _formBuilder.BuildForm(_config, panelOptions, _formController.UpdateButtons);
            _formController.LoadSavedChoices(_saveLoader);

            _isInitializing = false;
        }

        private void comboBoxTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
                return;

            _formSelectionProvider.SetTitle(comboBoxTitle.SelectedItem?.ToString() ?? "default");
            _formController.LoadSavedChoices(_saveLoader);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (!_formValidator.IsValid())
            {
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _applyManager.Apply(_formSelectionProvider);
            MessageBox.Show("Done.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!_formValidator.IsValid())
            {
                MessageBox.Show("Form is invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _saveSaver.SaveChoices(_formSelectionProvider.GetTitle(), _formSelectionProvider.GetChoices());
            MessageBox.Show($"Configuration saved for {_formSelectionProvider.GetTitle()}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
