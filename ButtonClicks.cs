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

        List<DataPoint> dataPoints1 = [];
        List<DataPoint> dataPoints2 = [];
        StringBuilder logs1 = new();
        StringBuilder logs2 = new();

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

        _ = await Task.Run(() => (dataPoints1, logs1) = FirstStatement.CalculatePoints(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion)).ConfigureAwait(false);
        _ = await Task.Run(() => (dataPoints2, logs2) = SecondStatement.CalculatePoints_original(housePrice, maxReservedMoney, creditDuration, personalMoney, loanInterestRate, incomeDispersion)).ConfigureAwait(false);

        await Task.Run(() => DrawGraphic(dataPoints1, logs1, dataPoints2, logs2)).ConfigureAwait(false);
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

    internal static void DrawGraphic (List<DataPoint> dataPoints1, StringBuilder logs1, List<DataPoint> dataPoints2, StringBuilder logs2)
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
        form.Controls.Add(tabControl);

        // Первый график
        LineSeries lineSeries1 = new()
        {
            ItemsSource = dataPoints1,
            Color = OxyColors.Blue
        };

        TabPage logsTabPage1 = new()
        {
            Text = "Логи постановки 1",
            //Font = new Font("null", 14),
            BackColor = Color.Blue
        };

        TextBox logTextBox1 = new()
        {
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            Font = new Font("null", 14),
            Text = logs1.ToString()
        };

        plotModel.Series.Add(lineSeries1);
        logsTabPage1.Controls.Add(logTextBox1);
        tabControl.TabPages.Add(logsTabPage1);

        // Второй график
        LineSeries lineSeries2 = new()
        {
            ItemsSource = dataPoints2,
            Color = OxyColors.Red
        };

        TabPage logsTabPage2 = new()
        {
            Text = "Логи постановки 2",
            //Font = new Font("null", 14),
            BackColor = Color.Red
        };

        TextBox logTextBox2 = new()
        {
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            Font = new Font("null", 14),
            Text = logs2.ToString()
        };

        plotModel.Series.Add(lineSeries2);
        logsTabPage2.Controls.Add(logTextBox2);
        tabControl.TabPages.Add(logsTabPage2);

        _ = form.ShowDialog();
    }
}
