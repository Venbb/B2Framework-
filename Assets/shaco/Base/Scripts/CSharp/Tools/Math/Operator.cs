using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace shaco.Base
{
    /// <summary>
    ///Reverse Polish notation
	///support operator { + - * / % () ^ AC }, more information see 'CalculateWithOpeartor' method
    /// </summary>
    public static class Operator
    {
        private const string MAX_PRIORITY_BEGIN = "(";
        private const string MAX_PRIORITY_END = ")";
        private const string DEFAULT_OPERATOR = "#";
        static private readonly string[] IGNORE_SYMBOL = new string[] { " " };

        static private List<string> _operatorStack = new List<string>();
        static private List<string> _ReversePolishNotationStack = new List<string>();

        static public short CalculateInt(string formula) { return (short)CalculateDouble(formula); }
        static public int CalculateShort(string formula) { return (int)CalculateDouble(formula); }
        static public float CalculateFloat(string formula) { return (float)CalculateDouble(formula); }
        
        static public double CalculateDouble(string formula)
        {
            ConvertToReversePolishNotation(formula);
            return CalculateReversePolishNotation();
        }

        static private void ConvertToReversePolishNotation(string formula)
        {
            int parseCount = 0;
            ResetOperatorStack();

            for (int i = 0; i < formula.Length;)
            {
                char cValue = formula[i];
                if (IsIgnoreSymbol(cValue.ToString()))
                {
                    ++i;
                    continue;
                }

                //------get number or decimal------
                if (IsNumberOrDecimal(cValue))
                {
                    string parseNumberOrDecimal = ParseUntilConditionBreak(formula, i, ref parseCount, (char parseValue) =>
                    {
                        return IsNumberOrDecimal(parseValue) && !IsSpecialPriority(parseValue.ToString());
                    });
                    i += parseCount;

					if (parseNumberOrDecimal[parseNumberOrDecimal.Length - 1] == '.')
					{
						throw new System.Exception("Operator convert error: invalid number or decimal=" + parseNumberOrDecimal);
					}

                    _ReversePolishNotationStack.Add(parseNumberOrDecimal);
                }
                else
                {
                    switch (cValue.ToString())
                    {
                        case MAX_PRIORITY_BEGIN:
                            {
                                _operatorStack.Add(cValue.ToString());
                                i += MAX_PRIORITY_BEGIN.Length;
                                break;
                            }
                        case MAX_PRIORITY_END:
                            {
                                //popup all operators between '(' and ')'
                                for (int j = _operatorStack.Count - 1; j >= 1; --j)
                                {
                                    string topOperator = _operatorStack[j];
                                    _operatorStack.RemoveAt(j);
                                    if (topOperator == MAX_PRIORITY_BEGIN.ToString())
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        _ReversePolishNotationStack.Add(topOperator);
                                    }
                                }
                                i += MAX_PRIORITY_END.Length;
                                break;
                            }
                        default:
                            {
                                //convert nagetive symbol
                                if (cValue == '-' && i + 1 < formula.Length && IsNumberOrDecimal(formula[i + 1]))
                                {
                                    formula = formula.Remove(i, 1);
                                    formula = formula.Insert(i, "AC");
                                }

                                //------get operator------
                                string parseOperator = ParseUntilConditionBreak(formula, i, ref parseCount, (char parseValue) =>
                                {
                                    return !IsNumberOrDecimal(parseValue) && !IsSpecialPriority(parseValue.ToString());
                                });
                                i += parseCount;

                                int parseOperatorPriority = GetPriority(parseOperator);
                                for (int j = _operatorStack.Count - 1; j >= 0; --j)
                                {
                                    string topOperator = _operatorStack[j];
                                    int topOperatorPriority = GetPriority(topOperator);

                                    //push into new operator operator stack
                                    if (parseOperatorPriority > topOperatorPriority)
                                    {
                                        _operatorStack.Add(parseOperator);
                                        break;
                                    }
                                    //popup number and operator to RPN stack
                                    else
                                    {
                                        _ReversePolishNotationStack.Add(topOperator);
                                        _operatorStack.RemoveAt(j);
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            //------popup all operator and stop parse------
            //ingore operator "#"
            for (int j = _operatorStack.Count - 1; j >= 1; --j)
            {
                string topOperator = _operatorStack[j];
                _ReversePolishNotationStack.Add(topOperator);
                _operatorStack.RemoveAt(j);
            }
        }

        static private string ToRPNString()
        {
            string retValue = string.Empty;
            for (int i = 0; i < _ReversePolishNotationStack.Count; ++i)
            {
                retValue += _ReversePolishNotationStack[i];
            }
            return retValue;
        }

        static private double CalculateReversePolishNotation()
        {
            if (_ReversePolishNotationStack.Count < 2)
            {
                throw new System.Exception("Operator CalculateReversePolishNotation error: not enough number count to calculate");
            }

            string lastOpeartorValue = _ReversePolishNotationStack[_ReversePolishNotationStack.Count - 1];
            if (IsNumberOrDecimal(lastOpeartorValue))
            {
                throw new System.Exception("Operator CalculateReversePolishNotation error: last string not a operator !");
            }

            List<string> calculateStack = new List<string>();
            for (int i = 0; i < _ReversePolishNotationStack.Count; ++i)
            {
                var valueTmp = _ReversePolishNotationStack[i];
                if (IsNumberOrDecimal(valueTmp))
                {
                    calculateStack.Add(valueTmp);
                }
                else
                {
                    string rightValue = PopupStack(calculateStack, "0");
                    string leftValue = PopupStack(calculateStack, "0");
                    double calculateValue = CalculateWithOpeartor(double.Parse(leftValue), double.Parse(rightValue), valueTmp);
                    calculateStack.Add(calculateValue.ToString());
                }
            }

            if (calculateStack.Count > 1)
            {
                throw new System.Exception("Operator CalculateReversePolishNotation error: invalid formula=" + ToRPNString());
            }

            double retValue = double.Parse(calculateStack[0]);
            return retValue;
        }

        static private T PopupStack<T>(List<T> list, T defaultValue)
        {
            if (list.Count == 0)
            {
                return defaultValue;
            }

            T retValue = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return retValue;
        }

        static private int GetPriority(string operatorValue)
        {
            switch (operatorValue)
            {
                case MAX_PRIORITY_BEGIN: return -1;
                case MAX_PRIORITY_END: return -1;
                case DEFAULT_OPERATOR: return -1;
                case "+":
                case "-": return 0;
                case "*":
                case "/":
                case "%": return 1;
                case "^": return 2;
                case "AC": return 3;
                default: throw new System.Exception("Operator GetPriority error: unsupport operator=" + operatorValue);
            }
        }

        static private double CalculateWithOpeartor(double leftValue, double rightValue, string operatorValue)
        {
            switch (operatorValue)
            {
                case "+": return leftValue + rightValue;
                case "-": return leftValue - rightValue;
                case "*": return leftValue * rightValue;
                case "/": return leftValue / rightValue;
                case "%": return leftValue % rightValue;
                case "^": return System.Math.Pow(leftValue, rightValue);
                case "AC": return -(leftValue + rightValue);
                default: throw new System.Exception("Operator CalculateWithOpeartor error: unsupport operator=" + operatorValue);
            }
        }

        static private string ParseUntilConditionBreak(string value, int startIndex, ref int praseCount, System.Func<char, bool> condition)
        {
            praseCount = 0;

            StringBuilder retValue = new StringBuilder();
            for (int i = startIndex; i < value.Length; ++i)
            {
                char cValue = value[i];

                //ignore space
                if (IsIgnoreSymbol(cValue.ToString()))
                {
                    ++praseCount;
                    break;
                }

                if (condition(cValue))
                {
                    ++praseCount;
                    retValue.Append(cValue);
                }
                else
                {
                    break;
                }
            }

            return retValue.ToString();
        }

        static private void ResetOperatorStack()
        {
            _operatorStack.Clear();
            _ReversePolishNotationStack.Clear();

            _operatorStack.Add(DEFAULT_OPERATOR);
        }

		static private bool IsNumberOrDecimal(char c)
        {
            return (c >= '0' && c <= '9') || c == '.';
        }

        static private bool IsSpecialPriority(string str)
        {
            return str == MAX_PRIORITY_BEGIN || str == MAX_PRIORITY_END;
        }

        static private bool IsIgnoreSymbol(string str)
        {
            bool retValue = false;
            for (int i = IGNORE_SYMBOL.Length - 1; i >= 0; --i)
            {
                if (str == IGNORE_SYMBOL[i])
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        static private bool IsNumberOrDecimal(string str)
        {
            bool retValue = true;
            for (int i = str.Length - 1; i >= 0; --i)
            {
                if (!IsNumberOrDecimal(str[i]))
                {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }
    }
}