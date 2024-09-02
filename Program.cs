#pragma warning disable CA1303
#pragma warning disable CS8618

namespace RiskCalculator;

public enum CalculationType
{
    Both,
    FirstOption,
    SecondOption,
}

internal sealed class Program : Form
{
    internal static ComboBox statementTypeComboBox;

    internal static NumericUpDown housePriceNumericUpDown;
    internal static NumericUpDown maxReservedMoneyNumericUpDown;
    internal static NumericUpDown creditDurationNumericUpDown;
    internal static NumericUpDown personalMoneyNumericUpDown;
    internal static NumericUpDown loanInterestRateNumericUpDown;
    internal static TableLayoutPanel incomeDispersionPanel;

    private Program ()
    {
        InitializeComponent();
        AutoSize = true;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.Sizable;
        Dock = DockStyle.Fill;
    }

    [STAThread]
    private static void Main ()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using Program program = new();
        Application.Run(program);
    }

    private void InitializeComponent ()
    {
        Text = "RiskCalculator";

        TableLayoutPanel panel = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
        };

        statementTypeComboBox = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };

        housePriceNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = decimal.MaxValue,
            Value = 200,
            DecimalPlaces = 0,
            UpDownAlign = LeftRightAlignment.Right,
        };

        maxReservedMoneyNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = decimal.MaxValue,
            Value = 50,
            DecimalPlaces = 0,
            UpDownAlign = LeftRightAlignment.Right,
        };

        creditDurationNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = decimal.MaxValue,
            Value = 8,
            DecimalPlaces = 0,
            UpDownAlign = LeftRightAlignment.Right,
        };

        personalMoneyNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = decimal.MaxValue,
            Value = 50,
            DecimalPlaces = 0,
            UpDownAlign = LeftRightAlignment.Right,
        };

        loanInterestRateNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 1,
            Value = 0.08M,
            DecimalPlaces = 2,
            UpDownAlign = LeftRightAlignment.Right,
            Increment = 0.01M,
        };

        Button addIncomeButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Text = "Добавить строку дохода"
        };

        Button removeIncomeButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Text = "Удалить строку дохода"
        };

        incomeDispersionPanel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            RowCount = 0, // Количество строк будет динамически изменяться
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
        };

        Button calculateButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Text = "Рассчитать"
        };

        addIncomeButton.Click += (sender, e) => ButtonClicks.AddIncomeField(0, 0.0);
        removeIncomeButton.Click += ButtonClicks.RemoveLastIncomeField;
        calculateButton.Click += ButtonClicks.CalculateButton_Click;

        _ = statementTypeComboBox.Items.Add(new KeyValuePair<string, CalculationType>("Обе", CalculationType.Both));
        _ = statementTypeComboBox.Items.Add(new KeyValuePair<string, CalculationType>("Первая", CalculationType.FirstOption));
        _ = statementTypeComboBox.Items.Add(new KeyValuePair<string, CalculationType>("Вторая", CalculationType.SecondOption));
        statementTypeComboBox.DisplayMember = nameof(KeyValuePair<string, CalculationType>.Key);
        statementTypeComboBox.ValueMember = nameof(KeyValuePair<string, CalculationType>.Value);

        statementTypeComboBox.SelectedIndex = 0;
        panel.Controls.Add(new Label { Dock = DockStyle.Fill, AutoSize = true, Text = "Тип постановки" });
        panel.Controls.Add(statementTypeComboBox);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Стоимость квартиры (тыс. руб.), S0:" });
        panel.Controls.Add(housePriceNumericUpDown);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Макс. начальный резерв денег (тыс. руб.), Z0max:" });
        panel.Controls.Add(maxReservedMoneyNumericUpDown);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Длительность кредита в годах, T:" });
        panel.Controls.Add(creditDurationNumericUpDown);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Собственные деньги в нулевой момент (тыс. руб.), M0:" });
        panel.Controls.Add(personalMoneyNumericUpDown);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Годовая ставка по кредиту в долях, r:" });
        panel.Controls.Add(loanInterestRateNumericUpDown);

        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true, Text = "Распределение годового дохода, ξt:" });
        panel.Controls.Add(new Label() { Dock = DockStyle.Fill, AutoSize = true });

        panel.Controls.Add(addIncomeButton);
        panel.Controls.Add(removeIncomeButton);
        panel.Controls.Add(incomeDispersionPanel);
        panel.Controls.Add(calculateButton);

        Controls.Add(panel);

        // Инициализация значений по умолчанию для распределения дохода
        List<KeyValuePair<int, double>> defaultValues =
        [
             new(20, 0.1),
             new(25, 0.2),
             new(30, 0.4),
             new(35, 0.2),
             new(40, 0.1),
        ];

        foreach (KeyValuePair<int, double> value in defaultValues)
        {
            ButtonClicks.AddIncomeField(value.Key, value.Value);
        }
    }
}
