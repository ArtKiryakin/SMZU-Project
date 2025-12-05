using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class ExpressionParser
    {
        private readonly string _text;
        private readonly Dictionary<string, ParameterExpression> _params;
        private int _pos;

        public ExpressionParser(string text, Dictionary<string, ParameterExpression> parameters)
        {
            _text = text.Replace(" ", "");
            _params = parameters;
            _pos = 0;
        }

        private char Current => _pos < _text.Length ? _text[_pos] : '\0';

        private void Next() => _pos++;

        private bool Eat(char c)
        {
            if (Current == c)
            {
                Next();
                return true;
            }
            return false;
        }

        // ----------------- Grammar -----------------
        // Expr     = Term ((+|-) Term)*
        // Term     = Power ((*|/) Power)*
        // Power    = Factor (^ Power)?
        // Factor   = number | variable | func | '(' Expr ')'
        // -------------------------------------------

        public Expression ParseExpression()
        {
            Expression left = ParseTerm();

            while (true)
            {
                if (Eat('+')) left = Expression.Add(left, ParseTerm());
                else if (Eat('-')) left = Expression.Subtract(left, ParseTerm());
                else break;
            }

            return left;
        }

        private Expression ParseTerm()
        {
            Expression left = ParsePower();

            while (true)
            {
                if (Eat('*')) left = Expression.Multiply(left, ParsePower());
                else if (Eat('/')) left = Expression.Divide(left, ParsePower());
                else break;
            }

            return left;
        }

        private Expression ParsePower()
        {
            Expression left = ParseFactor();

            while (true)
            {
                if (Eat('^'))
                {
                    var right = ParsePower();
                    left = Expression.Call(typeof(Math).GetMethod(nameof(Math.Pow))!, left, right);
                }
                else break;
            }

            return left;
        }

        private Expression ParseFactor()
        {
            if (Eat('-'))
                return Expression.Negate(ParseFactor());

            if (Eat('('))
            {
                var inner = ParseExpression();
                if (!Eat(')')) throw new Exception("Пропущена закрывающая скобка");
                return inner;
            }

            if (char.IsDigit(Current) || Current == '.')
                return ParseNumber();

            if (char.IsLetter(Current) || Current == '_')
                return ParseIdentifier();

            throw new Exception($"Неожиданный символ: {Current}");
        }

        private Expression ParseNumber()
        {
            int start = _pos;
            while (char.IsDigit(Current) || Current == '.') Next();

            double val = double.Parse(_text[start.._pos], CultureInfo.InvariantCulture);
            return Expression.Constant(val);
        }

        private Expression ParseIdentifier()
        {
            int start = _pos;

            while (char.IsLetterOrDigit(Current) || Current == '_')
                Next();

            string name = _text[start.._pos];

            // Функция?
            if (Eat('('))
            {
                var args = new List<Expression> { ParseExpression() };
                while (Eat(',')) args.Add(ParseExpression());
                if (!Eat(')')) throw new Exception("Ожидалась ) в вызове функции");

                return BuildFunction(name.ToLower(), args);
            }

            // Переменная?
            if (_params.TryGetValue(name, out var p))
                return p;

            throw new Exception($"Неизвестная переменная: {name}");
        }

        private Expression BuildFunction(string name, List<Expression> args)
        {
            return name switch
            {
                "sin" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Sin))!, args[0]),
                "cos" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Cos))!, args[0]),
                "tan" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Tan))!, args[0]),
                "abs" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) })!, args[0]),
                "sqrt" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Sqrt))!, args[0]),
                "log" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Log), new[] { typeof(double) })!, args[0]),
                "exp" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Exp))!, args[0]),
                "min" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!, args[0], args[1]),
                "max" => Expression.Call(typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!, args[0], args[1]),
                _ => throw new Exception($"Неизвестная функция: {name}")
            };
        }
    }
}
