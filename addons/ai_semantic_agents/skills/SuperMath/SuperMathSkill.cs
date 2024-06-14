using System.ComponentModel;
using Godot;
using Microsoft.SemanticKernel.SkillDefinition;

namespace GDSK;

public sealed class SuperMathSkill
{
    public SKAgent agent;

    public SuperMathSkill(SKAgent agent)
    {
        this.agent = agent;
    }

    [SKFunction, Description("Takes the square root of a value.")]
    public double Sqrt([Description("The value to take the square root of.")] double value)
    {
        GD.Print("Using native C# function, here's the proof!");
        return System.Math.Sqrt(value);
    }

    [SKFunction, Description("Adds two numbers together.")]
    public double Add(
        [Description("The first value to add.")] double a,
        [Description("The second value to add.")] double b
    )
    {
        return a + b;
    }

    [SKFunction, Description("Subtracts a number from another.")]
    public double Subtract(
        [Description("The value that will be subtracted from.")] double a,
        [Description("The value that will be used to subtract from the first.")] double b
    )
    {
        return a - b;
    }

    [SKFunction, Description("Multiplies two numbers.")]
    public double Multiply(
        [Description("The first value to multiply.")] double a,
        [Description("The second value to multiply.")] double b
    )
    {
        return a * b;
    }

    [SKFunction, Description("Divides two numbers.")]
    public double Divide(
        [Description("The numerator.")] double a,
        [Description("The denominator.")] double b
    )
    {
        return a / b;
    }
}