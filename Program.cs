using System.Text;
using OxyPlot;

#pragma warning disable CA1303
#pragma warning disable CA2000
#pragma warning disable CS8618

namespace RiskCalculator;

internal sealed class Program : Form
{
    private static CheckBox taskTypeCheckBox;
    private static NumericUpDown housePriceNumericUpDown;
    private static NumericUpDown maxReservedMoneyNumericUpDown;
    private static NumericUpDown creditDurationNumericUpDown;
    private static NumericUpDown personalMoneyNumericUpDown;
    private static NumericUpDown loanInterestRateNumericUpDown;
    private static TableLayoutPanel incomeDispersionPanel;

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
            //Padding = new Padding(10)
        };

        Label taskTypeLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Выберите постановку задачи"
        };

        taskTypeCheckBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "",
            AutoCheck = true,
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

        addIncomeButton.Click += (sender, e) => AddIncomeField(0, 0.0);
        removeIncomeButton.Click += RemoveLastIncomeField;
        calculateButton.Click += CalculateButton_Click;
        taskTypeCheckBox.CheckedChanged += TaskType_Click;
        taskTypeCheckBox.Checked = true;

        // Создание панелей для размещения элементов
        panel.Controls.Add(taskTypeLabel);
        panel.Controls.Add(taskTypeCheckBox);
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
             new(200, 0.1),
             new(250, 0.2),
             new(300, 0.4),
             new(350, 0.2),
             new(400, 0.1),
        ];

        foreach (KeyValuePair<int, double> value in defaultValues)
        {
            AddIncomeField(value.Key, value.Value);
        }
    }

    private static void AddIncomeField (int moneyValue, double probabilityValue)
    {
        incomeDispersionPanel.RowCount++;
        _ = incomeDispersionPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Label moneyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Деньги:"
        };

        NumericUpDown moneyNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = decimal.MaxValue,
            Value = moneyValue,
            DecimalPlaces = 0,
        };

        Label probabilityLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Вероятность:"
        };

        NumericUpDown probabilityNumericUpDown = new()
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 1,
            Value = (decimal) probabilityValue,
            DecimalPlaces = 2,
            Increment = 0.01M,
        };

        incomeDispersionPanel.Controls.Add(moneyLabel, 0, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(moneyNumericUpDown, 1, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(probabilityLabel, 2, incomeDispersionPanel.RowCount - 1);
        incomeDispersionPanel.Controls.Add(probabilityNumericUpDown, 3, incomeDispersionPanel.RowCount - 1);
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
        List<KeyValuePair<int, double>> incomeDispersion = [];
        for (int row = 0; row < incomeDispersionPanel.RowCount; row++)
        {
            if (incomeDispersionPanel.GetControlFromPosition(1, row) is NumericUpDown moneyNumericUpDown && incomeDispersionPanel.GetControlFromPosition(3, row) is NumericUpDown probabilityNumericUpDown)
            {
                incomeDispersion.Add(new KeyValuePair<int, double>((int) moneyNumericUpDown.Value, (double) probabilityNumericUpDown.Value));
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

        if (taskTypeCheckBox.Checked)
        {
            await Task.Run(() =>
            {
                List<DataPoint> dataPoints = Utilities.CalculatePoints2(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);
                Utilities.DrawGraphic(dataPoints, logs);
            }).ConfigureAwait(false);
        }
        else
        {
            await Task.Run(() =>
            {
                List<DataPoint> dataPoints = Utilities.CalculatePoints1(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);
                Utilities.DrawGraphic(dataPoints, logs);
            }).ConfigureAwait(false);
        }
    }

    private static void TaskType_Click (object? sender, EventArgs e)
    {
        if (taskTypeCheckBox.Checked)
        {
            taskTypeCheckBox.Text = "Выбрана постановка 2";
        }
        else
        {
            taskTypeCheckBox.Text = "Выбрана постановка 1";
        }
    }
}
