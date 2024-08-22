using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

#pragma warning disable CA2000
#pragma warning disable CA1303
#pragma warning disable CA1305

namespace RiskCalculator;
internal static class Utilities
{
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

    // Новый алгоритм, в 18 раз быстрее
    internal static List<DataPoint> CalculatePoints (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        for (int i = 0; i <= maxReservedMoney; i++)
        {
            int mortgageLoan = housePrice - personalMoney + i; // Ипотечный кредит в тысячах рублей, D0

            double yearlyPayment = mortgageLoan * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));

            Dictionary<double, double> currentLevel = new() { { i, 1.0 } };
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
            _ = logs.AppendLine($"Риск заёмщика при резерве {i,-3}: {Math.Round(badPointsProbabilitySum, 5),-15}");
            _ = logs.AppendLine($"Ипотечный кредит в тысячах рублей, D0: {mortgageLoan}");
            _ = logs.AppendLine($"Годовой платеж в тысячах рублей, R: {yearlyPayment}");
            _ = logs.AppendLine();

            dataPoints.Add(new DataPoint(i, badPointsProbabilitySum));
        }

        return dataPoints;
    }

    internal static void DrawGraphic (List<DataPoint> dataPoints, StringBuilder logs)
    {
        // Создание модели графика
        PlotModel plotModel = new();
        LineSeries lineSeries = new()
        {
            ItemsSource = dataPoints
        };
        plotModel.Series.Add(lineSeries);

        // Создание вкладок
        TabControl tabControl = new()
        {
            Dock = DockStyle.Fill
        };

        // Вкладка с графиком
        TabPage graphTabPage = new()
        {
            Text = "График"
        };
        PlotView plotView = new()
        {
            Dock = DockStyle.Fill,
            Model = plotModel
        };
        graphTabPage.Controls.Add(plotView);
        tabControl.TabPages.Add(graphTabPage);

        // Вкладка с логами
        TabPage logsTabPage = new()
        {
            Text = "Логи"
        };
        TextBox logTextBox = new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Text = logs.ToString()
        };
        logsTabPage.Controls.Add(logTextBox);
        tabControl.TabPages.Add(logsTabPage);

        // Создание и отображение формы
        Form form = new()
        {
            Text = "График",
            AutoScaleMode = AutoScaleMode.Dpi,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 800,
            Height = 600
        };
        form.Controls.Add(tabControl);
        _ = form.ShowDialog();
    }

    // Старый алгоритм

    /*internal sealed class TreeNode (double money, double probability)
    {
        public double Money { get; set; } = money;
        public double Probability { get; set; } = probability;
        public List<TreeNode> Children { get; set; } = [];
    }

    internal static List<DataPoint> CalculatePointsOld (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        for (int i = 0; i <= maxReservedMoney; i++)
        {
            int mortgageLoan = housePrice - personalMoney + i; // Ипотечный кредит в тысячах рублей, D0

            // Годовой платеж в тысячах рублей, R
            double yearlyPayment = mortgageLoan * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));

            // надо делать перебор по резервам, от 0 до maxReservedMoney
            TreeNode root = new(i, 1.0);
            List<TreeNode> currentLevel = [root];
            List<TreeNode> badPoints = [];

            for (int j = 0; j < creditDuration; j++)
            {
                List<TreeNode> nextLevel = [];

                foreach (TreeNode node in currentLevel)
                {
                    foreach (KeyValuePair<int, double> income in incomeDispersion)
                    {
                        double newMoney = node.Money + income.Key - yearlyPayment;
                        if (newMoney < 0)
                        {
                            badPoints.Add(new TreeNode(newMoney, node.Probability * income.Value));
                            //Console.WriteLine($"Банкротство: {newMoney,-20} : {Math.Round(node.Probability * income.Value, 5),-15}");

                        }
                        else
                        {
                            TreeNode? existingNode = nextLevel.Find(n => n.Money == newMoney);
                            if (existingNode != null)
                            {
                                existingNode.Probability += node.Probability * income.Value;
                            }
                            else
                            {
                                nextLevel.Add(new TreeNode(newMoney, node.Probability * income.Value));
                            }
                        }
                    }
                }

                currentLevel = nextLevel;
            }

            double badPointsProbabilitySum = 0.0;
            foreach (TreeNode node in badPoints)
            {
                badPointsProbabilitySum += node.Probability;
            }

            _ = logs.AppendLine();
            _ = logs.AppendLine($"Риск заёмщика при резерве {i,-3}: {Math.Round(badPointsProbabilitySum, 5),-15}");
            _ = logs.AppendLine($"Ипотечный кредит в тысячах рублей, D0: {mortgageLoan}");
            _ = logs.AppendLine($"Годовой платеж в тысячах рублей, R: {yearlyPayment}");
            _ = logs.AppendLine();

            dataPoints.Add(new DataPoint(i, badPointsProbabilitySum));
        }

        return dataPoints;
    }*/
}
