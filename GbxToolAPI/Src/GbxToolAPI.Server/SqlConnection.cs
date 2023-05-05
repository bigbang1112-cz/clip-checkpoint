using System.Data;

namespace GbxToolAPI.Server;

public interface ISqlConnection : IDisposable
{
    IDbConnection Connection { get; }
}

public interface ISqlConnection<T> : ISqlConnection where T : IServer
{
}

public class SqlConnection : ISqlConnection
{
    public IDbConnection Connection { get; }

    public SqlConnection(IDbConnection connection)
    {
        Connection = connection;
    }

    public void Dispose()
    {
        Connection.Dispose();
    }
}

public class SqlConnection<T> : SqlConnection, ISqlConnection<T> where T : IServer
{
    public SqlConnection(IDbConnection connection) : base(connection)
    {
    }
}