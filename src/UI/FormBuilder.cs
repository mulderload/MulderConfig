using MulderConfig.src.Configuration;

namespace MulderConfig.src.UI;

public class FormBuilder(FormValidator formValidator, FormSelectionProvider formSelectionProvider)
{
    public void BuildAddons(ConfigModel config, ComboBox comboBox)
    {
        comboBox.Items.Clear();

        foreach (var addon in config.Addons)
        {
            comboBox.Items.Add(addon.Title);
        }

        comboBox.SelectedIndex = 0;
        formSelectionProvider.SetAddon(comboBox.SelectedItem?.ToString());
    }

    public void BuildForm(ConfigModel config, Panel panelOptions, Action updateButtons)
    {
        panelOptions.Controls.Clear();

        int y = 10;

        foreach (var group in config.OptionGroups)
        {
            var groupBox = new GroupBox
            {
                Text = group.Name,
                Left = 10,
                Top = y,
                Width = panelOptions.ClientSize.Width - 40,
                Height = 10,
                AutoSize = true,
                ForeColor = Color.White,
            };

            panelOptions.Controls.Add(groupBox);

            int innerY = 20;

            if (group.Type == "radioGroup" && group.Radios != null)
            {
                foreach (var radioChoice in group.Radios)
                {
                    var radioButton = new RadioButton
                    {
                        Text = radioChoice.Value,
                        Left = 10,
                        Top = innerY,
                        AutoSize = true
                    };

                    radioButton.CheckedChanged += (s, e) =>
                    {
                        formValidator.ApplyWhenConstraints();
                        updateButtons();
                    };
                    groupBox.Controls.Add(radioButton);
                    formSelectionProvider.AddRadioButton(group.Name, radioButton, radioChoice.Value);
                    innerY += 25;
                }
            }
            else if (group.Type == "checkboxGroup" && group.Checkboxes != null)
            {
                foreach (var checkItem in group.Checkboxes)
                {
                    var checkBox = new CheckBox
                    {
                        Text = checkItem.Value,
                        Left = 10,
                        Top = innerY,
                        AutoSize = true,
                        Tag = checkItem.Value
                    };

                    checkBox.CheckedChanged += (s, e) =>
                    {
                        formValidator.ApplyWhenConstraints();
                        updateButtons();
                    };
                    groupBox.Controls.Add(checkBox);
                    formSelectionProvider.AddCheckBox(checkBox, checkItem.Value);
                    innerY += 25;
                }
            }

            groupBox.Height = innerY + 10;
            y += groupBox.Height + 8;
        }
    }
}
