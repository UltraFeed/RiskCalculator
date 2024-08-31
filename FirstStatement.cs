using System.Text;
using OxyPlot;

#pragma warning disable CA1305

namespace RiskCalculator;
internal static class FirstStatement
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

    internal static (List<DataPoint>, StringBuilder) CalculatePoints (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion)
    {
        List<DataPoint> dataPoints = [];
        StringBuilder logs = new();

        for (int startReserve = 0; startReserve <= maxReservedMoney; startReserve++)
        {
            double yearlyPayment = (housePrice - personalMoney + startReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));

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

            _ = logs.AppendLine($"Риск заёмщика при резерве {startReserve,-3}: {Math.Round(badPointsProbabilitySum, 5),-15}");
            _ = logs.AppendLine($"Ипотечный кредит в тысячах рублей, D0: {housePrice - personalMoney + startReserve}");
            _ = logs.AppendLine($"Годовой платеж в тысячах рублей, R: {yearlyPayment}");
            _ = logs.AppendLine();

            dataPoints.Add(new DataPoint(startReserve, badPointsProbabilitySum));
        }

        return (dataPoints, logs);
    }
}
