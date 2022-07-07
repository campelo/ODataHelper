﻿namespace StringToExpression.Test
{
    public class ParserTests
    {
        [Fact]
        public void Should_parse_basic_expression()
        {
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(decimal.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<decimal>("1 + 2 + 3 + 5");
            Assert.Equal(11, expression.Compile()());
        }

        [Fact]
        public void When_too_many_operaters_should_throw()
        {
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(decimal.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var exception = Assert.Throws<OperandExpectedException>(() => language.Parse<decimal>("1 + + 5"));
            Assert.Equal("1 + [+] 5", exception.OperatorStringSegment.Highlight());
            Assert.Equal("1 + []+ 5", exception.ExpectedOperandStringSegment.Highlight());
        }

        [Fact]
        public void When_too_many_operand_should_throw()
        {
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(decimal.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var exception = Assert.Throws<OperandUnexpectedException>(() => language.Parse<decimal>("1 + 5 5"));
            Assert.Equal("1 [+] 5 5", exception.OperatorStringSegment.Highlight());
            Assert.Equal("1 + 5 [5]", exception.UnexpectedOperandStringSegment.Highlight());
        }


        [Fact]
        public void Should_obey_operator_precedence()
        {
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    orderOfPrecedence: 2,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                  new OperatorDefinition(
                    name: "MULTIPLY",
                    regex: @"\*",
                    orderOfPrecedence: 1,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Multiply(args[0], args[1])),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(decimal.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<decimal>("1 + 2 * 3 + 5");
            Assert.Equal(12, expression.Compile()());
        }

        [Fact]
        public void Should_apply_brackets()
        {
            BracketOpenDefinition openBracket;
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    orderOfPrecedence: 1,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                  new OperatorDefinition(
                    name: "MULTIPLY",
                    regex: @"\*",
                    orderOfPrecedence: 2,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Multiply(args[0], args[1])),
                openBracket = new BracketOpenDefinition(
                    name: "OPENBRACKET",
                    regex: @"\("),
                new BracketCloseDefinition(
                    name: "CLOSEBRACKET",
                    regex: @"\)",
                    bracketOpenDefinition: openBracket),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(decimal.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<decimal>("(1 + 2) * (3 + 5)");
            Assert.Equal(24, expression.Compile()());
        }

        [Fact]
        public void Should_run_single_param_functions()
        {
            BracketOpenDefinition openBracket, sinFn;
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    orderOfPrecedence: 10,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                 sinFn = new FunctionCallDefinition(
                    name: "SIN",
                    regex: @"sin\(",
                    expressionBuilder: args => Expression.Call(typeof(Math).GetMethod("Sin"), args[0])),
                openBracket = new BracketOpenDefinition(
                    name: "OPENBRACKET",
                    regex: @"\("),
                new BracketCloseDefinition(
                    name: "CLOSEBRACKET",
                    regex: @"\)",
                    bracketOpenDefinitions: new[] { openBracket, sinFn }),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(double.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<double>("sin(1+2)+3");
            Assert.Equal(3.14, expression.Compile()(), 2);
        }

        [Fact]
        public void Should_run_two_param_functions()
        {
            BracketOpenDefinition openBracket, logFn;
            GrammerDefinition listDelimeter;
            var language = new Language(
                new OperatorDefinition(
                    name: "PLUS",
                    regex: @"\+",
                    orderOfPrecedence: 1,
                    paramaterPositions: new[] { RelativePosition.Left, RelativePosition.Right },
                    expressionBuilder: args => Expression.Add(args[0], args[1])),
                 logFn = new FunctionCallDefinition(
                    name: "LOG",
                    regex: @"[Ll]og\(",
                    expressionBuilder: args => Expression.Call(Type<int>.Method(x => Math.Log(0, 0)), args)),
                openBracket = new BracketOpenDefinition(
                    name: "OPENBRACKET",
                    regex: @"\("),
                listDelimeter = new ListDelimiterDefinition(
                    name: "COMMA",
                    regex: @"\,"),
                new BracketCloseDefinition(
                    name: "CLOSEBRACKET",
                    regex: @"\)",
                    bracketOpenDefinitions: new[] { openBracket, logFn },
                    listDelimeterDefinition: listDelimeter),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d*\.?\d+?",
                    expressionBuilder: x => Expression.Constant(double.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<double>("Log(1024,2) + 5");
            Assert.Equal(15, expression.Compile()());
        }

        [Fact]
        public void Should_cast_function_params()
        {
            BracketOpenDefinition openBracket, logFn;
            GrammerDefinition listDelimeter;
            var language = new Language(

                 logFn = new FunctionCallDefinition(
                    name: "LOG",
                    regex: @"[Ll]og\(",
                    argumentTypes: new[] { typeof(double), typeof(double) },
                    expressionBuilder: args => Expression.Call(Type<int>.Method(x => Math.Log(0, 0)), args)),
                openBracket = new BracketOpenDefinition(
                    name: "OPENBRACKET",
                    regex: @"\("),
                listDelimeter = new ListDelimiterDefinition(
                    name: "COMMA",
                    regex: @"\,"),
                new BracketCloseDefinition(
                    name: "CLOSEBRACKET",
                    regex: @"\)",
                    bracketOpenDefinitions: new[] { openBracket, logFn },
                    listDelimeterDefinition: listDelimeter),
                new OperandDefinition(
                    name: "NUMBER",
                    regex: @"\d+",
                    expressionBuilder: x => Expression.Constant(int.Parse(x))),
                new GrammerDefinition(name: "WHITESPACE", regex: @"\s+", ignore: true)
                );

            var expression = language.Parse<double>("Log(1024,2)");
            Assert.Equal(10, expression.Compile()());
        }


    }
}
