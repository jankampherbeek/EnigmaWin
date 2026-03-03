using System;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public sealed class RadixInputViewModel
{
    private readonly RadixInputModel _model;

    public RadixInputViewModel(RadixInputModel model)
    {
        _model = model;
    }

    public FullChart CalculateAndPrint(RadixInputModel.InputData inputData)
    {
        var chart = _model.Calculate(inputData);
        Console.WriteLine(chart);
        return chart;
    }
}
