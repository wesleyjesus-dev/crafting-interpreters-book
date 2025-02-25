namespace Lox;

public class Environment
{
    private Dictionary<string, object> _values = new();
    public void Define(string name, object value) => _values[name] = value;

    public object Get(Token name)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            return _values[name.Lexeme];
        }
        
        throw new Interpreter.RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }
    
}