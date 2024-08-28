using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

#pragma warning disable CA2000
#pragma warning disable CA1303
#pragma warning disable CA1305
#pragma warning disable CA1814

namespace RiskCalculator;

internal static class Utilities
{
    // Первая постановка
    // Из каждой вершины будет выходить количество новых вершин, равных incomeDispersion.Count.
    // У кажой вершины есть характеристика - Текущее количество денег и вероятность, с которой мы попадаем в эту вершину.
    // На каждой итерации из каждой новой вершины будет выходить количество новых вершин, равное incomeDispersion.Count.
    // Количество денег для каждой вершины считается по формуле: количество денег в родительской вершине + количество денег, которые мы получаем (пары incomeDispersion содержат эту информацию) - ежемесячный платеж;
    // Если количество денег в вершине < 0, то создание вытекающих из нее вершин прерывается, а характеристики этой вершины сохраняются в отдельный List badPoints.
    // Если в процессе создания новых вершин создаются вершины, в которых количество денег совпадает, то все такие вершины кроме одной удаляются, а вероятности попадания в эти вершины добавляются к вероятности попадания в единственную оставшуюся.
    // Количество итераций создания новых вершин равно creditDuration.
    // Требуется вычислить сумму вероятностей всех вершин, находящихся в badPoints.
    // Затем нужно пройти по значениям от 0 до maxReservedMoney и сохранить пары: зарезервированные деньги - риск
    // Потом строим график. По оси y риск, по оси x зарезервированные деньги

    internal static List<DataPoint> CalculatePoints1 (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        for (int startReserve = 0; startReserve < maxReservedMoney; startReserve++)
        {
            int mortgageLoan = housePrice - personalMoney + startReserve; // Ипотечный кредит в тысячах рублей, D0

            double yearlyPayment = mortgageLoan * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));

            Dictionary<double, double> currentLevel = new() { { startReserve, 1.0 } };
            double badPointsProbabilitySum = 0.0;

            for (int j = 0; j < creditDuration; j++)
            {
                Dictionary<double, double> nextLevel = [];

                foreach (KeyValuePair<double, double> kvp in currentLevel)
                {
                    double currentMoney = kvp.Key;
                    double currentProbability = kvp.Value;

                    foreach (KeyValuePair<int, double> income in incomeDispersion)
                    {
                        double newMoney = currentMoney + income.Key - yearlyPayment;
                        double newProbability = currentProbability * income.Value;

                        if (newMoney < 0)
                        {
                            badPointsProbabilitySum += newProbability;
                        }
                        else
                        {
                            if (nextLevel.ContainsKey(newMoney))
                            {
                                nextLevel [newMoney] += newProbability;
                            }
                            else
                            {
                                nextLevel [newMoney] = newProbability;
                            }
                        }
                    }
                }

                currentLevel = nextLevel;
            }

            _ = logs.AppendLine();
            _ = logs.AppendLine($"Риск заёмщика при резерве {startReserve,-3}: {Math.Round(badPointsProbabilitySum, 5),-15}");
            _ = logs.AppendLine($"Ипотечный кредит в тысячах рублей, D0: {mortgageLoan}");
            _ = logs.AppendLine($"Годовой платеж в тысячах рублей, R: {yearlyPayment}");
            _ = logs.AppendLine();

            dataPoints.Add(new DataPoint(startReserve, badPointsProbabilitySum));
        }

        return dataPoints;
    }

    // Вторая постановка
    // Постановка та же самая, но есть возможность управлять резервом.
    internal static List<DataPoint> CalculatePoints2 (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        double [,] nextRisk = new double [housePrice, housePrice]; // заполняется нулями по умолчанию
        double [,] currentRisk = new double [housePrice, housePrice]; // заполняется нулями по умолчанию

        // Считаем начальные условия для времени creditDuration - 1
        // Цикл по ST-1
        for (int housePriceToPay = 0; housePriceToPay < housePrice; housePriceToPay++)
        {
            // Цикл по MT-1
            for (int moneyToPay = 0; moneyToPay < housePrice; moneyToPay++)
            {
                // Цикл по количеству пар в распределении
                for (int k = 0; k < incomeDispersion.Count; k++)
                {
                    if (incomeDispersion [k].Key < ((housePriceToPay - moneyToPay) * (1 + loanInterestRate)))
                    {
                        nextRisk [housePriceToPay, moneyToPay] += incomeDispersion [k].Value;
                    }
                }
            }
        }

        // Основная часть
        for (int currentTime = creditDuration - 2; currentTime > 1; currentTime--)
        {
            for (int currentHousePrice = 0; currentHousePrice < housePrice; currentHousePrice++)
            {
                for (int currentMoney = 0; currentMoney < housePrice; currentMoney++)
                {
                    if (currentMoney > currentHousePrice)
                    {
                        currentRisk [currentHousePrice, currentMoney] = 0;
                    }
                    else
                    {
                        currentRisk [currentHousePrice, currentMoney] = double.MaxValue;

                        for (int currentReserve = 0; currentReserve < currentMoney; currentReserve++)
                        {
                            double nextPayment = (currentHousePrice - currentMoney + currentReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration - currentTime) / (Math.Pow(1 + loanInterestRate, creditDuration - currentTime) - 1));

                            double tmp = 0;
                            foreach (KeyValuePair<int, double> income in incomeDispersion)
                            {
                                if (currentReserve + income.Key < nextPayment)
                                {
                                    tmp += income.Value;
                                }
                                else
                                {
                                    int indexS = Convert.ToInt32(Math.Ceiling(((currentHousePrice - currentMoney + currentReserve) * (1 + loanInterestRate)) - nextPayment));
                                    int indexM = Convert.ToInt32(Math.Floor(currentReserve + income.Key - nextPayment));
                                    tmp += income.Value * nextRisk [indexS, indexM];
                                }
                            }

                            if (tmp < currentRisk [currentHousePrice, currentMoney])
                            {
                                currentRisk [currentHousePrice, currentMoney] = tmp;
                            }
                        }
                    }
                }
            }

            nextRisk = currentRisk;
            // возможно нужно обнулять currentRisk
        }

        for (int startReserve = 0; startReserve < maxReservedMoney; startReserve++)
        {
            int mortgageLoan = housePrice - personalMoney + startReserve; // Ипотечный кредит в тысячах рублей, D0
            double yearlyPayment = mortgageLoan * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));
            double tmp = 0;
            foreach (KeyValuePair<int, double> income in incomeDispersion)
            {
                if (startReserve + income.Key < yearlyPayment)
                {
                    tmp += income.Value;
                }
                else
                {
                    int indexS = Convert.ToInt32(Math.Ceiling((mortgageLoan * (1 + loanInterestRate)) - yearlyPayment));
                    int indexM = Convert.ToInt32(Math.Floor(startReserve + income.Key - yearlyPayment));
                    tmp += income.Value * nextRisk [indexS, indexM];
                }
            }

            _ = logs.AppendLine($"Начальный резерв: {startReserve}");
            _ = logs.AppendLine($"Риск: {tmp}");
            dataPoints.Add(new DataPoint(startReserve, tmp));
        }

        return dataPoints;
    }

    internal static void DrawGraphic (List<DataPoint> dataPoints, StringBuilder logs)
    {
        Form form = new()
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