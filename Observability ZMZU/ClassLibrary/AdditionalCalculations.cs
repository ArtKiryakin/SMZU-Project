using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace ClassLibrary
{
    public class AdditionalCalculations
    {
        private static readonly HashSet<string> AllowedFunctions = new()
        {
            "sin","cos","tan","abs","sqrt",
            "log","exp","min","max"
        };

        public static double Evaluate(string formula, Dictionary<string, double> variables)
        {
            // правка пробелов
            formula = NormalizeSpaces(formula);

            ValidateFormula(formula, variables);

            // создаём параметры
            var parameters = variables.ToDictionary(
                v => v.Key,
                v => Expression.Parameter(typeof(double), v.Key)
            );

            // парсим
            var parser = new ExpressionParser(formula, parameters);
            Expression expr = parser.ParseExpression();

            // ЗАМЕНА ПАРАМЕТРОВ НА КОНСТАНТЫ ← исправление ошибки
            expr = new ParameterToConstantReplacer(variables).Visit(expr);

            // создаём лямбду без параметров
            var lambda = Expression.Lambda<Func<double>>(expr);
            return lambda.Compile().Invoke();
        }

        private static string NormalizeSpaces(string s)
        {
            return s.Replace('\u00A0', ' ')
                    .Replace('\u202F', ' ')
                    .Trim();
        }

        // ---------------- VALIDATION ----------------
        private static void ValidateFormula(string formula, Dictionary<string, double> vars)
        {
            if (string.IsNullOrWhiteSpace(formula))
                throw new ArgumentException("Формула пустая.");

            if (!Regex.IsMatch(formula, @"^[0-9A-Za-z_\+\-*/^().,\s]*$"))
                throw new ArgumentException("Формула содержит недопустимые символы.");

            // Баланс скобок
            int balance = 0;
            foreach (char c in formula)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0)
                    throw new ArgumentException("Ошибка порядка закрытия скобок.");
            }
            if (balance != 0)
                throw new ArgumentException("Скобки не сбалансированы.");

            // Проверка имён
            var tokens = Regex.Matches(formula, @"[A-Za-z_][A-Za-z0-9_]*")
                              .Select(m => m.Value)
                              .Distinct();

            foreach (var t in tokens)
            {
                if (!vars.ContainsKey(t) && !AllowedFunctions.Contains(t.ToLower()))
                    throw new ArgumentException($"Неизвестная переменная или функция: '{t}'");
            }
        }
    }
}
