using System;

#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IFormattedConfigOption<T> : IConfigOption<T>
{
    public IFormattedConfigOption<T> SetDisplayFormatter(Func<IFormattedConfigOption<T>, T, string> formatter);
}
