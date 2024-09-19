using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace StaticEvalFloat
{
    enum Op { Add, Sub, Mul, Div, Cos, Sin, Stp, Spk, Mod, None }
    class Node
    {
        public Node parent;
        public Op op;
        public Operand[] operands;

        float Add(Dictionary<string, float> variables)
        {
            float v = 0;
            foreach (Operand o in operands)
            {
                switch (o.type)
                {
                    case OperandType.Float: v += o.valuefloat; break;
                    case OperandType.Variable: v += variables[o.valueVariable]; break;
                    case OperandType.Node: v += o.valueNode.Evaluate(variables); break;
                }
            }
            return v;
        }

        float Sub(Dictionary<string, float> variables)
        {
            float v = operands[0].type switch
            {
                OperandType.Float => operands[0].valuefloat,
                OperandType.Variable => variables[operands[0].valueVariable],
                OperandType.Node => operands[0].valueNode.Evaluate(variables),
                _ => 0
            };

            for (int i = 1; i < operands.Length; i++)
            {
                var o = operands[i];
                switch (o.type)
                {
                    case OperandType.Float: v -= o.valuefloat; break;
                    case OperandType.Variable: v -= variables[o.valueVariable]; break;
                    case OperandType.Node: v -= o.valueNode.Evaluate(variables); break;
                }
            }
            return v;
        }

        float Mul(Dictionary<string, float> variables)
        {
            float v = 1;
            foreach (Operand o in operands)
            {
                switch (o.type)
                {
                    case OperandType.Float: v *= o.valuefloat; break;
                    case OperandType.Variable: v *= variables[o.valueVariable]; break;
                    case OperandType.Node: v *= o.valueNode.Evaluate(variables); break;
                }
            }
            return v;
        }

        float Div(Dictionary<string, float> variables)
        {
            float v = operands[0].type switch
            {
                OperandType.Float => operands[0].valuefloat,
                OperandType.Variable => variables[operands[0].valueVariable],
                OperandType.Node => operands[0].valueNode.Evaluate(variables),
                _ => 1
            };

            for (int i = 1; i < operands.Length; i++)
            {
                var o = operands[i];
                switch (o.type)
                {
                    case OperandType.Float: v /= o.valuefloat; break;
                    case OperandType.Variable: v /= variables[o.valueVariable]; break;
                    case OperandType.Node: v /= o.valueNode.Evaluate(variables); break;
                }
            }
            return v;
        }

        float GetOperand(Operand o, Dictionary<string, float> variables)
        {
            switch (o.type)
            {
                case OperandType.Float: return o.valuefloat;
                case OperandType.Variable: return variables[o.valueVariable];
                case OperandType.Node: return o.valueNode.Evaluate(variables);
                default: return 0;
            }
        }

        float Mod(Dictionary<string, float> variables)
        {
            float m = operands.Length > 1 ? GetOperand(operands[1], variables) : 1;
            float v = operands.Length > 0 ? GetOperand(operands[0], variables) : 0;

            return v % m;
        }

        float Cos(Dictionary<string, float> variables)
        {
            float r = operands.Length > 1 ? GetOperand(operands[1], variables) : 1;
            float ph = operands.Length > 2 ? GetOperand(operands[2], variables) : 0;
            float v = operands.Length > 0 ? GetOperand(operands[0], variables) : 0;

            return Mathf.Cos(r * v + ph);
        }

        float Sin(Dictionary<string, float> variables)
        {
            float r = operands.Length > 1 ? GetOperand(operands[1], variables) : 1;
            float ph = operands.Length > 2 ? GetOperand(operands[2], variables) : 0;
            float v = operands.Length > 0 ? GetOperand(operands[0], variables) : 0;

            return Mathf.Sin(r * v + ph);
        }


        float Stp(Dictionary<string, float> variables)
        {
            float x = operands.Length > 0 ? GetOperand(operands[0], variables) : 1;
            float p = operands.Length > 1 ? GetOperand(operands[1], variables) : 1; // period
            float w = operands.Length > 2 ? GetOperand(operands[2], variables) : p / 4; ; // width
            float hp = p * 0.5f; // half period

            if (w >= p)
                return 0;

            float s = 1 / (hp - w);// slope

            x = x % p;

            if (x < hp - w)
                return x * s;
            if (x < hp)
                return 1;
            if (x < p - w)
                return 1 - (x - hp) * s;

            return 0;
        }

        float Spk(Dictionary<string, float> variables)
        {
            float x = operands.Length > 0 ? GetOperand(operands[0], variables) : 1;
            float p = operands.Length > 1 ? GetOperand(operands[1], variables) : 1; // period
            float w = operands.Length > 2 ? GetOperand(operands[2], variables) : p / 4; ; // width
            float hp = p * 0.5f; // half period

            float s = 1 / (w * 0.5f);// slope

            x = x % p;

            if (x < w * 0.5f)
                return x * s;
            if (x < w)
                return 1 - (x - w * 0.5f) * s;

            return 0;
        }

        float None(Dictionary<string, float> variables)
        {
            return GetOperand(operands[0], variables);
        }

        public float Evaluate(Dictionary<string, float> variables)
        {
            return op switch
            {
                Op.Add => Add(variables),
                Op.Sub => Sub(variables),
                Op.Mul => Mul(variables),
                Op.Div => Div(variables),
                Op.Cos => Cos(variables),
                Op.Sin => Sin(variables),
                Op.Stp => Stp(variables),
                Op.Spk => Spk(variables),
                Op.Mod => Mod(variables),
                _ => None(variables)
            };
        }

        public override string ToString()
        {
            string str = $"{op} [";
            foreach (Operand o in operands) str += $"{o} | ";
            str += "]";
            return str;
        }
    }

    enum OperandType { Float, Variable, Node }
    class Operand
    {
        public OperandType type;
        public float valuefloat;
        public string valueVariable;
        public Node valueNode;
        public override string ToString()
        {
            return type switch
            {
                OperandType.Float => valuefloat.ToString(),
                OperandType.Variable => valueVariable.ToString(),
                OperandType.Node => valueNode.ToString(),
                _ => ""
            };
        }
    }


    public class EvaluatorFloat
    {
        Node root;
        Dictionary<string, float> variables = new Dictionary<string, float>();

        public Dictionary<string, float> Variables
        {
            get => variables;
        }

        public int Id { get; set; }

        static Dictionary<string, EvaluatorFloat> evaluators = new Dictionary<string, EvaluatorFloat>();
        static Dictionary<int, EvaluatorFloat> evaluatorsById = new Dictionary<int, EvaluatorFloat>();
        static int nextId = 0;

        public static EvaluatorFloat GetEvaluator(string op)
        {
            op.Replace(" ", "");
            if (evaluators.ContainsKey(op))
                return evaluators[op];
            Log.Info($"CREATE OP:{op}");
            EvaluatorFloat e = new EvaluatorFloat();
            e.Build(op);
            evaluators.Add(op, e);

            return e;
        }

        public static int GetEvaluatorId(string op)
        {
            if (op == null || op == "")
                return -1;
            op.Replace(" ", "");
            if (evaluators.ContainsKey(op))
                return evaluators[op].Id;

            EvaluatorFloat e = new EvaluatorFloat();
            e.Build(op);
            evaluators.Add(op, e);
            e.Id = nextId++;
            evaluatorsById.Add(e.Id, e);

            return e.Id;
        }

        public static EvaluatorFloat GetEvaluator(int id)
        {
            if (id == -1) return null;
            return evaluatorsById[id];
        }

        public void Build(string op)
        {
            if (op == null || op == "")
                op = "0";
            op.Replace(" ", "");
            op = Expand(op, '+', "add");
            // Log.Info(op);
            op = Expand(op, '-', "sub");
            //Log.Info(op);
            op = Expand(op, '*', "mul");
            // Log.Info(op);
            op = Expand(op, '/', "div");
            // Log.Info(op);
            op = Expand(op, '%', "mod");

            if (!op.Contains("("))
                op = $"none({op})";

            variables = new Dictionary<string, float>();
            root = GetNode(op, null);
        }

        public override string ToString()
        {
            return $"{root} variables:{variables.Count}";
        }

        string Expand(string str, char sep, string opcode)
        {
            // Log.Info($"***** checking {sep} {opcode}");
            int pos = str.IndexOf(sep);
            while (pos != -1)
            {
                int i = pos + 1;
                int pCnt = 0;
                bool cont = true;
                while (cont)
                {
                    if (i > str.Length - 1) { cont = false; }
                    else
                    {
                        if (str[i] == '(')
                        {
                            pCnt++;
                        }
                        else if (str[i] == ')')
                        {
                            if (pCnt == 0) { cont = false; }
                            else pCnt--;
                        }
                        else if ((str[i] == sep || str[i] == ',') && pCnt == 0) { cont = false; }
                    }

                    if (cont) i++;
                }

                int ri = i;

                i = pos - 1;
                pCnt = 0;
                cont = true;
                while (cont)
                {
                    if (i < 0) { cont = false; }
                    else
                    {
                        if (str[i] == ')')
                        {
                            if (pCnt == 0 && i < pos - 1) { i++; cont = false; }
                            else pCnt++;
                        }
                        else if (str[i] == '(')
                        {
                            if (pCnt == 0) { cont = false; }
                            else pCnt--;
                        }
                        else if ((str[i] == sep || str[i] == ',') && pCnt == 0) { cont = false; }
                    }

                    if (cont) i--;
                }

                int li = i;

                // Log.Info($"{li}/{pos}/{ri}");

                int prefixStart = 0;
                int prefixLength = li < 0 ? 0 : li + 1;
                int suffixStart = ri;
                int suffixLength = ri >= str.Length ? 0 : str.Length - ri;
                int op1Start = li < 0 ? 0 : li + 1;
                int op1Length = pos - op1Start;
                int op2Start = pos + 1;
                int op2Length = ri >= str.Length ? str.Length - op2Start : ri - op2Start;
                // Log.Info($"{prefixStart}-{prefixLength} {op1Start}-{op1Length} {op2Start}-{op2Length} {suffixStart}-{suffixLength}");
                str = $"{str.Substring(prefixStart, prefixLength)}" +
                    $"{opcode}({str.Substring(op1Start, op1Length)},{str.Substring(op2Start, op2Length)})" +
                    $"{str.Substring(suffixStart, suffixLength)}";
                // Log.Info($"EXPSTEP {str}");

                pos = str.IndexOf(sep);
            }
            return str;
        }

        string[] GetOperands(string str)
        {
            int pCnt = 0;
            StringBuilder strb = new StringBuilder(str);

            for (int i = 0; i < str.Length; i++)
            {
                if (strb[i] == '(') pCnt++;
                if (strb[i] == ')') pCnt--;
                if (strb[i] == ',' && pCnt == 0) strb[i] = '|';
            }

            Regex operandsRx = new Regex(@"([^|]+)");

            MatchCollection operationMatches = operandsRx.Matches(strb.ToString());

            if (operationMatches.Count == 0)
                throw new ArgumentException($"Evaluator invalid operand {str}");

            List<string> outOperands = new List<string>();
            foreach (Match match in operationMatches)
            {
                if (match.Groups.Count == 0)
                    throw new ArgumentException($"Evaluator invalid operand {str}");
                outOperands.Add(match.Groups[1].Value);
            }
            return outOperands.ToArray();
        }

        public bool SetVar(string key, float value)
        {
            if (!variables.ContainsKey(key)) return false;

            variables.Remove(key);
            variables[key] = value;

            return true;
        }
        public float Evaluate()
        {
            if (root == null)
                return 0;

            return root.Evaluate(variables);
        }

        Node GetNode(string op, Node parent)
        {
            Regex operationRx = new Regex(@"([\w]*)\((.*)\)");
            Log.Info($"OP:{op}");
            MatchCollection operationMatches = operationRx.Matches(op);

            if (operationMatches.Count == 0)
                throw new ArgumentException($"Evaluator invalid operation {op}");

            Node node = new Node();

            foreach (Match match in operationMatches)
            {
                if (match.Groups.Count != 3)
                    throw new ArgumentException($"Evaluator invalid operation {op}");

                Op operation = match.Groups[1].Value.ToLower() switch
                {
                    "add" => Op.Add,
                    "sub" => Op.Sub,
                    "mul" => Op.Mul,
                    "div" => Op.Div,
                    "cos" => Op.Cos,
                    "sin" => Op.Sin,
                    "stp" => Op.Stp,
                    "spk" => Op.Spk,
                    "mod" => Op.Mod,
                    _ => Op.None
                };

                node.op = operation;

                var operands = GetOperands(match.Groups[2].Value);
                List<Operand> listOperands = new List<Operand>();

                foreach (string o in operands)
                {
                    Operand operand = new Operand();

                    if (float.TryParse(o,
                        NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out float _))
                    {
                        operand.type = OperandType.Float;
                        operand.valuefloat = float.Parse(o);
                    }
                    else if (o.Contains("("))
                    {
                        operand.type = OperandType.Node;
                        operand.valueNode = GetNode(o, node);
                    }
                    else
                    {
                        operand.type = OperandType.Variable;
                        operand.valueVariable = o;
                        variables.Add(o, 0);
                    }

                    listOperands.Add(operand);
                }

                node.operands = listOperands.ToArray();
            }
            node.parent = parent;

            return node;
        }
    }
}