using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

#pragma warning disable CA1303

namespace RiskCalculator;
internal static class ButtonClicks
{
    internal static List<KeyValuePair<int, double>> GetDispersionList ()
    {
        List<KeyValuePair<int, double>> incomeDispersion = [];
        for (int row = 0; row < Program.incomeDispersionPanel.RowCount; row++)
        {
            if (Program.incomeDispersionPanel.GetControlFromPosition(1, row) is NumericUpDown moneyNumericUpDown && Program.incomeDispersionPanel.GetControlFromPosition(3, row) is NumericUpDown probabilityNumericUpDown)
            {
                incomeDispersion.Add(new KeyValuePair<int, double>((int) moneyNumericUpDown.Value, (double) probabilityNumericUpDown.Value));
            }
        }

        return incomeDispersion;
    }

    internal static async void CalculateButton_Click (object? sender, EventArgs e)
    {
        int housePrice = (int) Program.housePriceNumericUpDown.Value;
        int maxReservedMoney = (int) Program.maxReservedMoneyNumericUpDown.Value;
        int creditDuration = (int) Program.creditDurationNumericUpDown.Value;
        int personalMoney = (int) Program.personalMoneyNumericUpDown.Value;
        double loanInterestRate = (double) Program.loanInterestRateNumericUpDown.Value;

        List<KeyValuePair<int, double>> incomeDispersion = GetDispersionList();

        if (Math.Abs(incomeDispersion.Sum(pair => pair.Value) - 1.0) > 1e-9)
        {
            _ = MessageBox.Show($"Сумма вероятностей не равна {1.0}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (maxReservedMoney > personalMoney)
        {
            _ = MessageBox.Show($"{nameof(maxReservedMoney)} не может быть больше, чем {nameof(personalMoney)}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (Program.taskTypeCheckBox.Checked)
        {
            await Task.Run(() =>
            {
                List<DataPoint> dataPoints = SecondStatement.CalculatePoints_original(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);
                DrawGraphic(dataPoints, logs);
            }).ConfigureAwait(false);
        }
        else
        {
            await Task.Run(() =>
            {
                List<DataPoint> dataPoints = FirstStatement.CalculatePoints(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion, out StringBuilder logs);
                DrawGraphic(dataPoints, logs);
            }).ConfigureAwait(false);
        }
    }

    internal static void TaskType_Click (object? sender, EventArgs e)
    {
        if (Program.taskTypeCheckBox.Checked)
        {
            Program.taskTypeCheckBox.Text = $"Выбрана постановка 2";
        }
        else
        {
            Program.taskTypeCheckBox.Text = $"Выбрана постановка 1";
        }
    }

    internal static void RemoveLastIncomeField (object? sender, EventArgs e)
    {
        if (Program.incomeDispersionPanel.RowCount > 0)
        {
            Program.incomeDispersionPanel.RowCount--;
            Program.incomeDispersionPanel.RowStyles.RemoveAt(Program.incomeDispersionPanel.RowCount);
            for (int i = 0; i < 4; i++)
            {
                Control? control = Program.incomeDispersionPanel.GetControlFromPosition(i, Program.incomeDispersionPanel.RowCount);
                if (control != null)
                {
                    Program.incomeDispersionPanel.Controls.Remove(control);
                }
            }
        }
    }

    internal static void AddIncomeField (int moneyValue, double probabilityValue)
    {
        Program.incomeDispersionPanel.RowCount++;
        _ = Program.incomeDispersionPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

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

        Program.incomeDispersionPanel.Controls.Add(moneyLabel, 0, Program.incomeDispersionPanel.RowCount - 1);
        Program.incomeDispersionPanel.Controls.Add(moneyNumericUpDown, 1, Program.incomeDispersionPanel.RowCount - 1);
        Program.incomeDispersionPanel.Controls.Add(probabilityLabel, 2, Program.incomeDispersionPanel.RowCount - 1);
        Program.incomeDispersionPanel.Controls.Add(probabilityNumericUpDown, 3, Program.incomeDispersionPanel.RowCount - 1);
    }

    internal static void DrawGraphic (List<DataPoint> dataPoints, StringBuilder logs)
    {
        using Form form = new()
        {
            Text = "График",
            AutoScaleMode = AutoScaleMode.Dpi,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 800,
            Height = 600
        };

        PlotModel plotModel = new();
        LineSeries lineSeries = new()
        {
            ItemsSource = dataPoints
        };
        plotModel.Series.Add(lineSeries);

        PlotView plotView = new()
        {
            Dock = DockStyle.Fill,
            Model = plotModel
        };

        TabControl tabControl = new()
        {
            Dock = DockStyle.Fill
        };

        TabPage graphTabPage = new()
        {
            Text = "График",
            Font = new Font("null", 14),
        };
        graphTabPage.Controls.Add(plotView);
        tabControl.TabPages.Add(graphTabPage);

        TabPage logsTabPage = new()
        {
            Text = "Логи",
            Font = new Font("null", 14),
        };

        TextBox logTextBox = new()
        {
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            Font = new Font("null", 14),
            Text = logs.ToString()
        };

        logsTabPage.Controls.Add(logTextBox);
        tabControl.TabPages.Add(logsTabPage);

        form.Controls.Add(tabControl);
        _ = form.ShowDialog();
    }
}
