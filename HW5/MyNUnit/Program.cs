// <copyright file="Program.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using MyNUnit;

if (args.Length == 0)
{
    Console.WriteLine("Usage: MyNUnit <directory_path>");
    return;
}

var directoryPath = args[0];

var testRunner = new TestRunner();
var results = testRunner.RunTests(directoryPath);

var reportPrinter = new ReportPrinter();
reportPrinter.PrintReport(results);
