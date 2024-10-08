﻿using System.Text;
using OxyPlot;

#pragma warning disable CA1305
#pragma warning disable CA1814

namespace RiskCalculator;

internal static class SecondStatement
{
    // Вторая постановка
    // Постановка та же самая, но есть возможность управлять резервом.
    internal static (List<DataPoint>, StringBuilder) CalculatePoints (int housePrice, int maxReservedMoney, int creditDuration, int personalMoney, double loanInterestRate, List<KeyValuePair<int, double>> incomeDispersion)
    {
        List<DataPoint> dataPoints = [];
        StringBuilder logs = new();

        _ = logs.AppendLine($"Вторая постановка");
        _ = logs.AppendLine();

        double [,] nextRisk = new double [housePrice + 1, housePrice + 1];
        double [,] currentRisk = new double [housePrice + 1, housePrice + 1];

        // Считаем начальные условия для конечного момента времени
        for (int housePriceToPay = 0; housePriceToPay <= housePrice; housePriceToPay++)
        {
            for (int moneyToPay = 0; moneyToPay <= housePrice; moneyToPay++)
            {
                foreach (KeyValuePair<int, double> income in incomeDispersion)
                {
                    // Проверка вероятности для последнего платежа
                    if (income.Key < (housePriceToPay - moneyToPay) * (1 + loanInterestRate))
                    {
                        nextRisk [housePriceToPay, moneyToPay] += income.Value;
                    }
                }
            }
        }

        // Основная часть вычислений для всех шагов времени от конца к началу
        for (int currentTime = creditDuration - 2; currentTime > 1; currentTime--)
        {
            for (int currentHousePrice = 0; currentHousePrice <= housePrice; currentHousePrice++)
            {
                for (int currentMoney = 0; currentMoney <= housePrice; currentMoney++)
                {
                    // Если не хватает денег, то риск = 0
                    if (currentHousePrice < currentMoney)
                    {
                        currentRisk [currentHousePrice, currentMoney] = 0;
                    }
                    else
                    {
                        currentRisk [currentHousePrice, currentMoney] = 1.0;

                        // Перебор возможных резервов
                        for (int currentReserve = 0; currentReserve < currentMoney; currentReserve++)
                        {
                            double nextPayment = (currentHousePrice - currentMoney + currentReserve) * (loanInterestRate * Math.Pow(1 + loanInterestRate, creditDuration - currentTime) / (Math.Pow(1 + loanInterestRate, creditDuration - currentTime) - 1));

                            double tmpRisk = 0;
                            foreach (KeyValuePair<int, double> income in incomeDispersion)
                            {
                                // Расчёт риска для текущего состояния резерва
                                if (currentReserve + income.Key < nextPayment)
                                {
                                    tmpRisk += income.Value;
                                }
                                else
                                {
                                    int indexS = Convert.ToInt32(Math.Ceiling(((currentHousePrice - currentMoney + currentReserve) * (1 + loanInterestRate)) - nextPayment));
                                    int indexM = Convert.ToInt32(Math.Floor(currentReserve + income.Key - nextPayment));
                                    tmpRisk += income.Value * nextRisk [indexS, indexM];
                                }
                            }

                            if (tmpRisk < currentRisk [currentHousePrice, currentMoney])
                            {
                                currentRisk [currentHousePrice, currentMoney] = tmpRisk;
                            }
                        }
                    }
                }
            }

            // Обновление риска для следующего момента времени
            nextRisk = currentRisk;
        }

        // Рассчитываем риск для начального момента времени
        for (int startReserve = 0; startReserve <= maxReservedMoney; startReserve++)
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

            _ = logs.AppendLine($"Резерв: {startReserve,+20}");
            _ = logs.AppendLine($"Риск: {Math.Round(tmp, 5),+30}");
            _ = logs.AppendLine($"Годовой платеж: {Math.Round(yearlyPayment, 5),+10}");
            _ = logs.AppendLine();

            dataPoints.Add(new DataPoint(startReserve, tmp));
        }

        return (dataPoints, logs);
    }
}
