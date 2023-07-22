using System;
using System.Globalization;
using static SkiaSharp.SKPath;
using System.Text;

namespace Kalkulator.Models;

public static class Calculator
{
    public static string Calculate(string str)
    {
        var calculus = new MyStringBuilder(str);

        var numberOfOpeningParentheses = calculus.Count('(');
        var numberOfClosingParentheses = calculus.Count(')');

        if (numberOfOpeningParentheses != numberOfClosingParentheses)
            return "Zamknij nawias";

        CalculateParentheses(ref calculus);
        CalculateNonParentheses(ref calculus);
        return calculus.ToString();
    }

    private static Operator? CharToOperator(char? character)
    {
        return character switch
        {
            OperatorChar.Add => Operator.Add,
            OperatorChar.Subtract => Operator.Subtract,
            OperatorChar.Multiply => Operator.Multiply,
            OperatorChar.Divide => Operator.Divide,
            _ => null
        };
    }

    private static void CalculateNonParentheses(ref MyStringBuilder calculus)
    {
        int indexOfOperator;

        while ((indexOfOperator = calculus.IndexOfAny(OperatorChar.PrecedentOperators, 1)) > 0 ||
               (indexOfOperator = calculus.IndexOfAny(OperatorChar.NonPrecedentOperators, 1)) > 0)
        {
            var indexOfPreviousOperator = SetIndexOfPreviousOperator(calculus, indexOfOperator);
            var stringOfFirstValue = calculus[(indexOfPreviousOperator + 1)..indexOfOperator];
            var startIndexOfCalculation = indexOfPreviousOperator + 1;

            if (stringOfFirstValue.Length == 1 &&
                OperatorChar.IsAnOperator(stringOfFirstValue[0]))
            {
                stringOfFirstValue = "0";
                indexOfOperator--;
            }

            var indexOfNextOperator = calculus.IndexOfAny(OperatorChar.Operators, indexOfOperator + 2);

            if (indexOfNextOperator == -1) 
                indexOfNextOperator = calculus.Length;

            var stringOfSecondValue = calculus[(indexOfOperator + 1)..indexOfNextOperator];
            var nextIndexAfterCalculation = indexOfNextOperator;

            var firstValue = Convert.ToDouble(stringOfFirstValue);
            var @operator = CharToOperator(calculus[indexOfOperator]);
            var secondValue = Convert.ToDouble(stringOfSecondValue);

            var calculation = new Calculation(firstValue, secondValue, @operator);

            calculus.Replace(startIndexOfCalculation, nextIndexAfterCalculation,
                Convert.ToString(calculation.Calculate(), CultureInfo.CurrentCulture));
        }
    }

    private static void CalculateParentheses(ref MyStringBuilder calculus)
    {
        int indexOfOpeningParenthesis;
        while ((indexOfOpeningParenthesis = calculus.LastIndexOf('(')) != -1)
        {
            var indexOfClosingParenthesis = calculus.IndexOf(')', indexOfOpeningParenthesis);
            calculus.Replace(indexOfOpeningParenthesis, indexOfClosingParenthesis + 1,
                Calculate(calculus[(indexOfOpeningParenthesis + 1)..indexOfClosingParenthesis]));
        }
    }

    private static int SetIndexOfPreviousOperator(MyStringBuilder calculus, int indexOfOperator)
    {
        var indexOfPreviousOperator = calculus.LastIndexOfAny(OperatorChar.Operators, indexOfOperator - 1);

        if (indexOfPreviousOperator == 0)
        {
            indexOfPreviousOperator = -1;
        }
        else if (indexOfPreviousOperator > 0 &&
                 calculus[indexOfPreviousOperator].Equals(OperatorChar.Subtract) && 
                 OperatorChar.IsAnOperator(calculus[indexOfPreviousOperator - 1])) 
        {
            indexOfPreviousOperator--;
        }

        return indexOfPreviousOperator;
    }
}