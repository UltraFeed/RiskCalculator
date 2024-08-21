﻿namespace RiskCalculator;
internal static class Validator
{
    // Метод для проверки, что сумма вероятностей равна 1
    internal static bool IsProbabilitySumValid (List<KeyValuePair<int, double>> incomeDispersion)
    {
        // Подсчет суммы вероятностей
        double sum = incomeDispersion.Sum(pair => pair.Value);

        // Допустимая погрешность, 10^(-9)
        const double epsilon = 1e-9;

        return Math.Abs(sum - 1.0) < epsilon;
    }

    internal static bool IsPositiveInteger (string input, out int value)
    {
        return int.TryParse(input, out value) && value > 0;
    }

    internal static bool IsPositiveDouble (string input, out double value)
    {
        return double.TryParse(input, out value) && value > 0;
    }
}
