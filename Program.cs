using System.Text;
using OxyPlot;

#pragma warning disable CA1303
#pragma warning disable CA1305
#pragma warning disable CA2000
#pragma warning disable CS8602
#pragma warning disable CS8618

namespace RiskCalculator;

internal sealed class Program : Form
{
    private static TextBox housePriceTextBox;
    private static TextBox maxReservedMoneyTextBox;
    private static TextBox creditDurationTextBox;
    private static TextBox personalMoneyTextBox;
    private static TextBox loanInterestRateTextBox;
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

        housePriceTextBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "2000",
        };

        Label maxReservedMoneyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Макс. начальный резерв денег (тыс. руб.), Z0max:"
        };

        maxReservedMoneyTextBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "2000",
        };

        Label creditDurationLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Длительность кредита в годах, T:"
        };
        creditDurationTextBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "15",
        };

        Label personalMoneyLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Собственные деньги в нулевой момент (тыс. руб.), M0:"
        };
        personalMoneyTextBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "100",
        };

        Label loanInterestRateLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Годовая ставка по кредиту в долях, r:"
        };
        loanInterestRateTextBox = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "0,22",
        };

        Label incomeDispersionLabel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Распределение годового дохода, ξt:",
            Margin = new Padding(0, 20, 0, 0)
        };

        Label incomeDispersionLabelEmpty = new()
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
        removeIncomeButton.Click += RemoveLastIncomeRow;
        calculateButton.Click += CalculateButton_Click;

        // Создание панелей для размещения элементов
        panel.Controls.Add(housePriceLabel);
        panel.Controls.Add(housePriceTextBox);
        panel.Controls.Add(maxReservedMoneyLabel);
        panel.Controls.Add(maxReservedMoneyTextBox);
        panel.Controls.Add(creditDurationLabel);
        panel.Controls.Add(creditDurationTextBox);
        panel.Controls.Add(personalMoneyLabel);
        panel.Controls.Add(personalMoneyTextBox);
        panel.Controls.Add(loanInterestRateLabel);
        panel.Controls.Add(loanInterestRateTextBox);
        panel.Controls.Add(incomeDispersionLabel);
        panel.Controls.Add(incomeDispersionLabelEmpty);
        panel.Controls.Add(addIncomeButton);
        panel.Controls.Add(removeIncomeButton);
        panel.Controls.Add(incomeDispersionPanel);
        panel.Controls.Add(calculateButton);

        Controls.Add(panel);

        // Инициализация значений по умолчанию для распределения дохода
        AddIncomeField(0, 0.1);
        AddIncomeField(720, 0.1);
        AddIncomeField(840, 0.65);
        AddIncomeField(960, 0.1);
        AddIncomeField(1200, 0.05);
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

    private void RemoveLastIncomeRow (object? sender, EventArgs e)
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
        // Создание StringBuilder для сбора сообщений об ошибках
        StringBuilder sb = new();

        // Валидация входных значений
        if (!Validator.IsPositiveInteger(housePriceTextBox.Text, out int housePrice))
        {
            _ = sb.AppendLine($"Неверное значение для {housePriceTextBox.Parent.Controls [0].Text}");
        }

        if (!Validator.IsPositiveInteger(maxReservedMoneyTextBox.Text, out int maxReservedMoney))
        {
            _ = sb.AppendLine($"Неверное значение для {maxReservedMoneyTextBox.Parent.Controls [0].Text}");
        }

        if (!Validator.IsPositiveInteger(creditDurationTextBox.Text, out int creditDuration))
        {
            _ = sb.AppendLine($"Неверное значение для {creditDurationTextBox.Parent.Controls [0].Text}");
        }

        if (!Validator.IsPositiveInteger(personalMoneyTextBox.Text, out int personalMoney))
        {
            _ = sb.AppendLine($"Неверное значение для {personalMoneyTextBox.Parent.Controls [0].Text}");
        }

        if (!Validator.IsPositiveDouble(loanInterestRateTextBox.Text, out double loanInterestRate))
        {
            _ = sb.AppendLine($"Неверное значение для {loanInterestRateTextBox.Parent.Controls [0].Text}");
        }

        if (sb.Length > 0)
        {
            _ = MessageBox.Show(sb.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        List<KeyValuePair<int, double>> incomeDispersion = GetDispersionList();

        // Проверка суммы вероятностей
        if (!Validator.IsProbabilitySumValid(incomeDispersion))
        {
            _ = MessageBox.Show("Сумма вероятностей не равна 1.0 с учетом погрешности.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Вычисление точек для графика
        //List<DataPoint> dataPoints = Tree.CalculatePoints(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion);
        //Tree.DrawGraphic(dataPoints);

        // Запуск длительной операции в фоновом потоке
        await Task.Run(() =>
        {
            List<DataPoint> dataPoints = Tree.CalculatePoints(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);

            // Отрисовка графика также в фоновом потоке
            Tree.DrawGraphic(dataPoints, logs);
        }).ConfigureAwait(false);

    }
}
