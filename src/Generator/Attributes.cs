using System;

namespace RazorBladeGenerator
{
    /// <summary>
    /// Attribute to mark a class for code generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateCodeAttribute : Attribute
    {
    }
}

