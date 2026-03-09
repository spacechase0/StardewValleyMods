#nullable enable
namespace GenericModConfigMenu.Api.Elements;

public interface IConfigOption<T> : IElement
{
    public T PendingValue { get; set; }

    public delegate bool Validator(IConfigOption<T> option, T value, out string error);
    public IConfigOption<T> SetSaveValidator(Validator validator);
}
