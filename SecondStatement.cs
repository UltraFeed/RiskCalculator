using System.Diagnostics;
using System.Text;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

#pragma warning disable CA1305
#pragma warning disable CA1814

namespace RiskCalculator;

internal static class SecondStatement
{
    // Вторая постановка
    // Постановка та же самая, но есть возможность управлять резервом.
    internal static List<DataPoint> CalculatePoints_original (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        // тут точно нельзя использовать двумерные массивы
        // подходящий тип данных - Dictionary<Tuple<int, int>, double>
        // словарь - список неповторяющихся элементов, где ключом будет два инта, как индексы массива
        // а значением - вероятность
        double [,] nextRisk = new double [housePrice + 1, housePrice + 1]; // заполняется нулями по умолчанию
        double [,] currentRisk = new double [housePrice + 1, housePrice + 1]; // заполняется нулями по умолчанию

        // Считаем начальные условия для конечного момента времени
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
                    if (currentHousePrice < currentMoney)
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

        // Начальный момент времени
        for (int startReserve = 0; startReserve < maxReservedMoney; startReserve++)
        {
            double yearlyPayment = (housePrice - personalMoney + startReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));
            double tmp = 0;
            foreach (KeyValuePair<int, double> income in incomeDispersion)
            {
                if (startReserve + income.Key < yearlyPayment)
                {
                    tmp += income.Value;
                }
                else
                {
                    int indexS = Convert.ToInt32(Math.Ceiling(((housePrice - personalMoney + startReserve) * (1 + loanInterestRate)) - yearlyPayment));
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

    // моя попытка оптимизировать код
    internal static List<DataPoint> CalculatePoints_new (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion, out StringBuilder logs)
    {
        List<DataPoint> dataPoints = [];
        logs = new StringBuilder();

        Dictionary<Tuple<int, int>, double> nextRisk = [];
        Dictionary<Tuple<int, int>, double> currentRisk = [];
#if DEBUG
        Debug.WriteLine($"[{DateTime.Now:dd.MM.yyyy hh:mm:ss}] {nameof(creditDuration)}{-1} = {creditDuration - 1}");
#endif
        for (int currentTime = creditDuration - 1; currentTime >= 0; currentTime--)
        {
            for (int currentHousePrice = 0; currentHousePrice < housePrice; currentHousePrice++)
            {
#if DEBUG
                if (currentHousePrice % 100 == 0)
                {
                    Debug.WriteLine($"[{DateTime.Now:dd.MM.yyyy hh:mm:ss}] {nameof(currentTime)} = {currentTime}, {nameof(currentHousePrice)} = {currentHousePrice}");
                }
#endif
                for (int currentMoney = 0; currentMoney < housePrice; currentMoney++)
                {
                    Tuple<int, int> key = Tuple.Create(currentHousePrice, currentMoney);

                    if (currentTime == 0) // Последняя итерация
                    {
                        for (int startReserve = 0; startReserve < maxReservedMoney; startReserve++)
                        {
                            double yearlyPayment = (housePrice - personalMoney + startReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration) / (Math.Pow(1 + loanInterestRate, creditDuration) - 1));
                            double tmp = 0;

                            foreach (KeyValuePair<int, double> income in incomeDispersion)
                            {
                                if (startReserve + income.Key < yearlyPayment)
                                {
                                    tmp += income.Value;
                                }
                                else
                                {
                                    int indexS = (int) Math.Ceiling(((housePrice - personalMoney + startReserve) * (1 + loanInterestRate)) - yearlyPayment);
                                    int indexM = (int) Math.Floor(startReserve + income.Key - yearlyPayment);
                                    Tuple<int, int> nextKey = Tuple.Create(indexS, indexM);
                                    tmp += income.Value * (nextRisk.TryGetValue(nextKey, out double value) ? value : 0);
                                }
                            }

                            _ = logs.AppendLine($"Начальный резерв: {startReserve}");
                            _ = logs.AppendLine($"Риск: {tmp}");
                            dataPoints.Add(new DataPoint(startReserve, tmp));
#if DEBUG
                            //Debug.WriteLine($"{nameof(startReserve)} = {startReserve}");
                            //Debug.WriteLine($"{nameof(tmp)} = {tmp}");
#endif
                        }
                    }
                    else
                    {
                        if (currentMoney > currentHousePrice)
                        {
                            currentRisk [key] = 0;
                        }
                        else
                        {
                            double minRisk = double.MaxValue;

                            for (int currentReserve = 0; currentReserve < currentMoney; currentReserve++)
                            {
                                double nextPayment = (currentHousePrice - currentMoney + currentReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration - currentTime) / (Math.Pow(1 + loanInterestRate, creditDuration - currentTime) - 1));

                                double tmp = 0;
                                foreach (KeyValuePair<int, double> income in incomeDispersion)
                                {
                                    if (income.Key < ((currentHousePrice - currentMoney) * (1 + loanInterestRate)) && currentTime == creditDuration - 1)
                                    {
                                        tmp += income.Value;
                                    }
                                    else
                                    {
                                        if (currentReserve + income.Key < nextPayment)
                                        {
                                            tmp += income.Value;
                                        }
                                        else
                                        {
                                            int indexS = (int) Math.Ceiling(((currentHousePrice - currentMoney + currentReserve) * (1 + loanInterestRate)) - nextPayment);
                                            int indexM = (int) Math.Floor(currentReserve + income.Key - nextPayment);
                                            Tuple<int, int> nextKey = Tuple.Create(indexS, indexM);

                                            if (nextRisk.TryGetValue(nextKey, out double value))
                                            {
                                                tmp += income.Value * value;
                                            }
                                        }
                                    }
                                }

                                if (tmp < minRisk)
                                {
                                    minRisk = tmp;
                                }
                            }

                            currentRisk [key] = minRisk;
                        }
                    }
                }
            }

            if (currentTime > 0)
            {
                nextRisk = new Dictionary<Tuple<int, int>, double>(currentRisk);
            }
        }
#if DEBUG
        Debug.WriteLine($"{nameof(dataPoints)}.Count = {dataPoints.Count}");
        Debug.WriteLine($"{nameof(dataPoints)}.Distinct().ToList().Count = {dataPoints.Distinct().ToList().Count}");
#endif
        return dataPoints.Distinct().ToList();
    }
}