using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [TestCase(-1, 2, Description = "when precision is negative")]
        [TestCase(3, -2, Description = "when scale is negative")]
        [TestCase(3, 4, Description = "when precision less or equal than scale")]
        public void Constructor_ThrowArgumentException(int precision, int scale)
        {
            Action act = () => new NumberValidator(precision, scale, true);
            act.ShouldThrow<ArgumentException>();
        }

        [TestCase(2, 1, Description = "when precision is positive and precision > scale")]
        public void Constructor_DoesNotThrow(int precision, int scale)
        {
            Action act = () => new NumberValidator(precision, scale, true);
            act.ShouldNotThrow();
        }

        [TestCase(17, 2, true, "1.23", Description = "when parts separated by point")]
        [TestCase(17, 2, true, "1,23", Description = "when parts separated by comma")]
        [TestCase(17, 2, true, "+1.23", Description = "when sign is first")]
        [TestCase(17, 2, true, "12", Description = "when integer without fraction")]
        [TestCase(17, 2, false, "-1.23", Description = "when negative and not onlyPositive")]
        public void Check_ValidateNumber(int precision, int scale, bool onlyPositive, string value)
        {
            new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value)
                .Should().BeTrue();
        }
        
        [TestCase(3, 2, true, null, Description = "when value is null")]
        [TestCase(3, 2, true, "", Description = "when value is empty")]
        [TestCase(3, 2, true, "asd", Description = "when value is just string")]
        [TestCase(3, 2, true, "+asd", Description = "when value is string with plus")]
        [TestCase(3, 2, false, "-asd", Description = "when value is string with minus")]
        [TestCase(3, 2, true, "a.sd", Description = "when value is string with point")]
        [TestCase(3, 2, true, ".", Description = "when value is point")]
        [TestCase(3, 2, true, "a,sd", Description = "when value is string with comma")]
        [TestCase(3, 2, true, ",", Description = "when value is comma")]
        [TestCase(3, 2, true, "++12", Description = "when number contains two signs")]
        [TestCase(3, 2, true, ".2", Description = "when there is no integer part")]
        [TestCase(3, 2, true, "1.234", Description = "when integer and fraction parts greater than precision")]
        [TestCase(3, 2, true, "+1.23", Description = "sign before number is considered as integer part")]
        [TestCase(17, 2, true, "0.000", Description = "when fraction part greater than scale")]
        [TestCase(3, 2, true, "-1.2", Description = "when only positive but value is negative")]
        public void Check_NotValidateNumber(int precision, int scale, bool onlyPositive, string value)
        {
            new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value)
                .Should().BeFalse();
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than scale");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}