﻿using LiteDB;

namespace LiteDB.Benchmark;


[RPlotExporter]
[MemoryDiagnoser]
public class BsonExpressionTests
{
    private BsonDocument _doc = new() { ["a"] = 34m };

    [Benchmark]
    public void ExprExec() => BsonExpression.Create("(45 + 12 * a) > 99").Execute(_doc);

    [Benchmark]
    public void ExprParseCompileExec() => BsonExpressionParser.ParseFullExpression(new Tokenizer("(45 + 12 * a) > 99"), true).Execute(_doc);

    //[Benchmark]
    //public void v5ExprExec() => v5.BsonExpression.Create("45 + 12 * a").Execute(new v5.BsonDocument { ["a"] = 34m });
}
