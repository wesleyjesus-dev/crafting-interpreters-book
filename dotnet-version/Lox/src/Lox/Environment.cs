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

    public void Assign(Token exprName, object value)
    {
        if (_values.ContainsKey(exprName.Lexeme))
        {
            _values[exprName.Lexeme] = value;
            return;
        }
        
        throw new Interpreter.RuntimeError(exprName, "Undefined variable '" + exprName.Lexeme + "'.");
    }
}