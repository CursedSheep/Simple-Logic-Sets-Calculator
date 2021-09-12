using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class Program {
  public static void Main() {
    string variables = "p=t,q=f,r=f"; //variable list
    string formula = "(p ∧ q) ∨ ¬r"; //given formula

    Console.WriteLine(EvaluateLogic.EvaluateExpression(formula, GetVariablesFromString(variables)));
  }
  private static Dictionary < string, bool > GetVariablesFromString(string str) {
    Dictionary < string, bool > datatest = new Dictionary < string, bool > ();
    var splitstr = str.Split(',');
    foreach(var item in splitstr) {
      var split2 = item.Split('=');
      datatest.Add(split2[0], char.ToLower(split2[1][0]) == 't');
    }
    return datatest;
  }
}
class EvaluateLogic {

  enum LogicalOperator {
    Negation,
    Conjunction,
    Disjunction,
    Conditional,
    Biconditional,
    ExclusiveOr,
    LdBool = 999
  }
  class LogicInstructions {
    public LogicalOperator Operator;
    public object Operand;
    public LogicInstructions(LogicalOperator _Operator, object _Operand) {
      Operator = _Operator;
      Operand = _Operand;
    }
	  public override string ToString()
	  {
		  if (Operator == LogicalOperator.LdBool)
			  return $"{Operator},{Operand ?? ""}";
		  else
			  return $"{stringOperators[Operator]},{Operand ?? ""}";
	  }
    static Dictionary < LogicalOperator, string > stringOperators = new Dictionary < LogicalOperator, string > () {

      {
        LogicalOperator.Negation, "~"
      }, {
        LogicalOperator.Conjunction,
        "∧"
      }, {
        LogicalOperator.Disjunction,
        "∨"
      }, {
        LogicalOperator.Conditional,
        "→"
      }, {
        LogicalOperator.Biconditional,
        "⟷"
      }, {
        LogicalOperator.ExclusiveOr,
        "⊕"
      }

    };
  }
  static Dictionary < string, LogicalOperator > LogicalOperators = new Dictionary < string, LogicalOperator > () {

    {
      "~",
      LogicalOperator.Negation
    }, {
      "∧",
      LogicalOperator.Conjunction
    }, {
      "∨",
      LogicalOperator.Disjunction
    }, {
      "→",
      LogicalOperator.Conditional
    }, {
      "⟷",
      LogicalOperator.Biconditional
    }, {
      "⊕",
      LogicalOperator.ExclusiveOr
    }

  };
  private static LogicInstructions[] ParseExpressions(string s, Dictionary < string, bool > data) {
    List < LogicInstructions > result = new List < LogicInstructions > ();
    Dictionary < int, Stack < LogicInstructions >> tmpOperator = new Dictionary < int, Stack < LogicInstructions >> ();
    StringBuilder sb = new StringBuilder();
    int parenthesisCount = 0;
    for (int i = 0; i < s.Length; i++) {
      sb.Append(s[i]);
      string currstr = sb.ToString();
      if (LogicalOperators.ContainsKey(currstr)) {
        var instruction = new LogicInstructions(LogicalOperators[currstr], null);

        if (!tmpOperator.ContainsKey(parenthesisCount))
          tmpOperator[parenthesisCount] = new Stack < LogicInstructions > ();
        tmpOperator[parenthesisCount].Push(instruction);
        sb.Clear();
      } else if (currstr.All(x => char.IsLetter(x)) && data.ContainsKey(currstr)) {
        var selectedData = data[currstr];
        result.Add(new LogicInstructions(LogicalOperator.LdBool, selectedData));
        sb.Clear();

        if (tmpOperator.ContainsKey(parenthesisCount) && tmpOperator[parenthesisCount].Count > 0) {
          //result.Add(tmpOperator[parenthesisCount].Pop());
          var item = tmpOperator[parenthesisCount];
          if (item.Count > 0) {
            for (int z = item.Count; z > 0; z--)
              result.Add(item.Pop());
          }
        }
      } else if (currstr == "(") {
        parenthesisCount++;
        sb.Clear();
      } else if (currstr == ")") {
        parenthesisCount--;
        sb.Clear();
      } else throw new Exception("Variable doesn't exist or invalid syntax!");
    }
    var reverse = tmpOperator.Reverse();
    foreach(var item in reverse) {
      if (item.Value.Count > 0) {
        for (int i = 0; i < item.Value.Count; i++)
          result.Add(item.Value.Pop());
      }
    }
    return result.ToArray();
  }
  private static bool GetBiConditional(bool o1, bool o2) {
    if (o1 && o2)
      return true;
    else if (!o1 && !o2)
      return true;
    else
      return false;
  }
  private static bool ExecuteInstructions(LogicInstructions[] instructions) {
    Stack < bool > MachineStack = new Stack < bool > ();
    foreach(var item in instructions) {
      switch (item.Operator) {
      case LogicalOperator.LdBool:
        MachineStack.Push((bool) item.Operand);
        break;
      case LogicalOperator.Negation:
        MachineStack.Push(!MachineStack.Pop());
        break;
      case LogicalOperator.Conjunction: {
        bool o2 = MachineStack.Pop();
        bool o1 = MachineStack.Pop();
        MachineStack.Push(o1 && o2);
      }
      break;
      case LogicalOperator.Disjunction: {
        bool o2 = MachineStack.Pop();
        bool o1 = MachineStack.Pop();
        MachineStack.Push(o1 || o2);
      }
      break;
      case LogicalOperator.Conditional: {
        bool o2 = MachineStack.Pop();
        bool o1 = MachineStack.Pop();
        if (o1 && o2)
          MachineStack.Push(true);
        else if (!o1 && o2)
          MachineStack.Push(true);
        else if (!o1 && !o2)
          MachineStack.Push(true);
        else
          MachineStack.Push(false);
      }
      break;
      case LogicalOperator.Biconditional: {
        bool o2 = MachineStack.Pop();
        bool o1 = MachineStack.Pop();
        MachineStack.Push(GetBiConditional(o1, o2));
      }
      break;
      case LogicalOperator.ExclusiveOr: {
        bool o2 = MachineStack.Pop();
        bool o1 = MachineStack.Pop();
        MachineStack.Push(!GetBiConditional(o1, o2));
      }
      break;
      default:
        throw new NotSupportedException();
      }
    }
    return MachineStack.Pop();
  }
  public static bool EvaluateExpression(string s, Dictionary < string, bool > data) {
    var test = ParseExpressions(s.Replace(" ", "").Replace("¬", "~").Replace("↔", "⟷").Replace("∼", "~"), data);
    return ExecuteInstructions(test);
  }
}
