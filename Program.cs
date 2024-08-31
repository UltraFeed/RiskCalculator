#pragma warning disable CA1303
#pragma warning disable CS8618

namespace RiskCalculator;

internal sealed class Program : Form
{
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

        Label housePriceLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Стоимость квартиры (тыс. руб.), S0:"
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

        Label maxReservedMoneyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Макс. начальный резерв денег (тыс. руб.), Z0max:"
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

        Label creditDurationLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Длительность кредита в годах, T:"
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

        Label personalMoneyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Собственные деньги в нулевой момент (тыс. руб.), M0:"
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

        Label loanInterestRateLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Годовая ставка по кредиту в долях, r:"
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

        Label incomeDispersionLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Распределение годового дохода, ξt:",
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

        // Создание панелей для размещения элементов
        panel.Controls.Add(housePriceLabel);
        panel.Controls.Add(housePriceNumericUpDown);
        panel.Controls.Add(maxReservedMoneyLabel);
        panel.Controls.Add(maxReservedMoneyNumericUpDown);
        panel.Controls.Add(creditDurationLabel);
        panel.Controls.Add(creditDurationNumericUpDown);
        panel.Controls.Add(personalMoneyLabel);
        panel.Controls.Add(personalMoneyNumericUpDown);
        panel.Controls.Add(loanInterestRateLabel);
        panel.Controls.Add(loanInterestRateNumericUpDown);
        panel.Controls.Add(incomeDispersionLabel);
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
