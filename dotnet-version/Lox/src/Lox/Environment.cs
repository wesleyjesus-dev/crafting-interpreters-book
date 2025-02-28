namespace Lox;

public class Environment
{
    public Environment()
    {
        Enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }
    public Environment Enclosing { get; set; }
    private Dictionary<string, object> _values = new();
    public void Define(string name, object value) => _values[name] = value;

    public object Get(Token name)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            return _values[name.Lexeme];
        }
        if (Enclosing != null)
            return Enclosing.Get(name);
        
        throw new Interpreter.RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Assign(Token exprName, object value)
    {
        if (_values.ContainsKey(exprName.Lexeme))
        {
            _values[exprName.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(exprName, value);
            return;
        }
        
        throw new Interpreter.RuntimeError(exprName, "Undefined variable '" + exprName.Lexeme + "'.");
    }
}