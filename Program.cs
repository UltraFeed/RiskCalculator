using System.Text;
using OxyPlot;

#pragma warning disable CA1303
#pragma warning disable CA1305
#pragma warning disable CA2000
#pragma warning disable CS8618

namespace RiskCalculator;

internal sealed class Program : Form
{
    private static NumericUpDown housePriceNumericUpDown;
    private static NumericUpDown maxReservedMoneyNumericUpDown;
    private static NumericUpDown creditDurationNumericUpDown;
    private static NumericUpDown personalMoneyNumericUpDown;
    private static NumericUpDown loanInterestRateNumericUpDown;
    private static TableLayoutPanel incomeDispersionPanel;

    private Program ()
    {
        InitializeComponent();
        AutoSize = false;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.Sizable;
        Width = 500;
        Height = Size.Height * 2;
        //Size = new Size(Size.Width, Size.Height * 2);
    }

    [STAThread]
    private static void Main ()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Program program = new();
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
            Padding = new Padding(10)
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
            Value = 2000,
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
            Value = 1000,
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
            Value = 500,
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
        };

        Label incomeDispersionLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Распределение годового дохода, ξt:",
            Margin = new Padding(0, 20, 0, 0)
        };

        Label EmptyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "",
        };

        incomeDispersionPanel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 0, // Количество строк будет динамически изменяться
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
        };

        Button addIncomeButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Добавить строку дохода"
        };

        Button removeIncomeButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Удалить строку дохода"
        };

        Button calculateButton = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Рассчитать"
        };

        addIncomeButton.Click += (sender, e) => AddIncomeField(0, 0.0);
        removeIncomeButton.Click += RemoveLastIncomeField;
        calculateButton.Click += CalculateButton_Click;

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
        panel.Controls.Add(EmptyLabel);
        panel.Controls.Add(addIncomeButton);
        panel.Controls.Add(removeIncomeButton);
        panel.Controls.Add(incomeDispersionPanel);
        panel.Controls.Add(calculateButton);

        Controls.Add(panel);

        // Инициализация значений по умолчанию для распределения дохода
        AddIncomeField(200, 0.1);
        AddIncomeField(250, 0.2);
        AddIncomeField(300, 0.4);
        AddIncomeField(350, 0.2);
        AddIncomeField(400, 0.1);
    }
    private static void AddIncomeField (int income, double probability)
    {
        // Установка новой строки
        incomeDispersionPanel.RowCount++;
        _ = incomeDispersionPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label moneyLabel = new() { Text = "Деньги:", AutoSize = true };
        TextBox incomeTextBox = new() { Text = income.ToString(), Width = 80 };
        Label probabilityLabel = new() { Text = "Вероятность:", AutoSize = true };
        TextBox probabilityTextBox = new() { Text = probability.ToString("F2"), Width = 80 };

        incomeDispersionPanel.Controls.Add(moneyLabel, 0, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(incomeTextBox, 1, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(probabilityLabel, 2, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(probabilityTextBox, 3, incomeDispersionPanel.RowCount - 1);
    }

    private void RemoveLastIncomeField (object? sender, EventArgs e)
    {
        if (incomeDispersionPanel.RowCount > 0)
        {
            incomeDispersionPanel.RowCount--;
            incomeDispersionPanel.RowStyles.RemoveAt(incomeDispersionPanel.RowCount);
            for (int i = 0; i < 4; i++)
            {
                Control? control = incomeDispersionPanel.GetControlFromPosition(i, incomeDispersionPanel.RowCount);
                if (control != null)
                {
                    incomeDispersionPanel.Controls.Remove(control);
                }
            }
        }
    }

    internal static List<KeyValuePair<int, double>> GetDispersionList ()
    {
        // Сборка распределения дохода
        List<KeyValuePair<int, double>> incomeDispersion = [];
        // Проходим по всем строкам в TableLayoutPanel
        for (int row = 0; row < incomeDispersionPanel.RowCount; row++)
        {
            if (incomeDispersionPanel.GetControlFromPosition(1, row) is TextBox incomeTextBox && incomeDispersionPanel.GetControlFromPosition(3, row) is TextBox probabilityTextBox)
            {
                if (int.TryParse(incomeTextBox.Text, out int income) &&
                    double.TryParse(probabilityTextBox.Text, out double probability))
                {
                    incomeDispersion.Add(new KeyValuePair<int, double>(income, probability));
                }
            }
        }

        return incomeDispersion;
    }

    private static async void CalculateButton_Click (object? sender, EventArgs e)
    {
        int housePrice = (int) housePriceNumericUpDown.Value;
        int maxReservedMoney = (int) maxReservedMoneyNumericUpDown.Value;
        int creditDuration = (int) creditDurationNumericUpDown.Value;
        int personalMoney = (int) personalMoneyNumericUpDown.Value;
        double loanInterestRate = (double) loanInterestRateNumericUpDown.Value;

        List<KeyValuePair<int, double>> incomeDispersion = GetDispersionList();

        if (!Validator.IsProbabilitySumValid(incomeDispersion))
        {
            _ = MessageBox.Show($"Сумма вероятностей не равна {1.0}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await Task.Run(() =>
        {
            List<DataPoint> dataPoints = Utilities.CalculatePoints(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);
            Utilities.DrawGraphic(dataPoints, logs);
        }).ConfigureAwait(false);
    }
}
