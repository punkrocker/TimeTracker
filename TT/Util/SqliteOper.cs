using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using TimeTracker.Util;

namespace Tstring.DataBase
{
    #region DataBaseOperatorII
    /// <summary>
    /// Sqlite数据库操作类
    /// </summary>
    public class SqliteOper
    {
        private string connectionString;

        public SqliteOper()
        {
            if (File.Exists(@"localdb.db"))
                connectionString = @"Data Source=" + System.Windows.Forms.Application.StartupPath + @"\localdb.db;Pooling=true;FailIfMissing=false";
            else
                throw new Exception(ConfigFile.Languege.ReadValue("FileNotFound"));
        }

        #region ExecuteNonQuery

        public int ExecuteNonQuery( string cmdText )
        {
            return SqliteHelper.ExecuteNonQuery( connectionString, CommandType.Text, cmdText, null );
        }

        public int ExecuteNonQuery( CommandType cmdType, string cmdText )
        {
            return SqliteHelper.ExecuteNonQuery( connectionString, cmdType, cmdText, null );
        }

        public int ExecuteNonQuery( CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            return SqliteHelper.ExecuteNonQuery( connectionString, cmdType, cmdText, commandParameters );
        }

        #endregion

        #region ExecuteReader

        public SQLiteDataReader ExecuteReader( string cmdText )
        {
            return SqliteHelper.ExecuteReader( connectionString, CommandType.Text, cmdText, null );
        }

        public SQLiteDataReader ExecuteReader( CommandType cmdType, string cmdText )
        {
            return SqliteHelper.ExecuteReader( connectionString, cmdType, cmdText, null );
        }

        public SQLiteDataReader ExecuteReader( CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            return SqliteHelper.ExecuteReader( connectionString, cmdType, cmdText, commandParameters );
        }


        #endregion

        #region ExecuteDataSet

        public DataSet ExecuteDataSet( string cmdText )
        {
            return SqliteHelper.ExecuteDataSet( connectionString, CommandType.Text, cmdText, null );
        }

        public DataSet ExecuteDataSet( CommandType cmdType, string cmdText )
        {
            return SqliteHelper.ExecuteDataSet( connectionString, cmdType, cmdText, null );
        }

        public DataSet ExecuteDataSet( CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            return SqliteHelper.ExecuteDataSet( connectionString, cmdType, cmdText, commandParameters );
        }

        #endregion

        #region ExecuteScalar

        public object ExecuteScalar( string cmdText )
        {
            return SqliteHelper.ExecuteScalar( connectionString, CommandType.Text, cmdText, null );
        }

        public object ExecuteScalar( CommandType cmdType, string cmdText )
        {
            return SqliteHelper.ExecuteScalar( connectionString, cmdType, cmdText, null );
        }

        public object ExecuteScalar( CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            return SqliteHelper.ExecuteScalar( connectionString, cmdType, cmdText, commandParameters );
        }
        #endregion
    }
    #endregion

    #region SqliteHelper
    /// <summary>
    /// The SqliteHelper class is intended to encapsulate high performance, 
    /// scalable best practices for common uses of SqlClient.
    /// </summary>
    public abstract class SqliteHelper
    {

        /// <summary>
        /// Database connection strings
        /// </summary>
        //public static readonly string ConnectionStringLocalTransaction = ConfigurationManager.ConnectionStrings["SQLConnString2"].ConnectionString;
        //public static readonly string ConnectionStringInventoryDistributedTransaction = ConfigurationManager.ConnectionStrings["SQLConnString2"].ConnectionString;
        //public static readonly string ConnectionStringOrderDistributedTransaction = ConfigurationManager.ConnectionStrings["SQLConnString3"].ConnectionString;
        //public static readonly string ConnectionStringProfile = ConfigurationManager.ConnectionStrings["SQLProfileConnString"].ConnectionString;        

        // Hashtable to store cached parameters
        private static Hashtable parmCache = Hashtable.Synchronized( new Hashtable() );

        /// <summary>
        /// Execute a SQLiteCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SQLiteConnection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery( string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {

            SQLiteCommand cmd = new SQLiteCommand();

            using( SQLiteConnection conn = new SQLiteConnection( connectionString ) )
            {
                PrepareCommand( cmd, conn, null, cmdType, cmdText, commandParameters );
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// Execute a SQLiteCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">an existing database connection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery( SQLiteConnection connection, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {

            SQLiteCommand cmd = new SQLiteCommand();

            PrepareCommand( cmd, connection, null, cmdType, cmdText, commandParameters );
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SQLiteCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery( SQLiteTransaction trans, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            SQLiteCommand cmd = new SQLiteCommand();
            PrepareCommand( cmd, trans.Connection, trans, cmdType, cmdText, commandParameters );
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// Execute a SQLiteCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SQLiteDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SQLiteConnection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SQLiteDataReader containing the results</returns>
        public static SQLiteDataReader ExecuteReader( string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection( connectionString );

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand( cmd, conn, null, cmdType, cmdText, commandParameters );
                SQLiteDataReader rdr = cmd.ExecuteReader( CommandBehavior.CloseConnection );
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 生成DataSet
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet( string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            SQLiteCommand cmd = new SQLiteCommand();
            SQLiteConnection conn = new SQLiteConnection( connectionString );

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand( cmd, conn, null, cmdType, cmdText, commandParameters );
                DataSet ds = new DataSet();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill( ds );
                cmd.Parameters.Clear();
                return ds;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a SQLiteCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SQLiteConnection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar( string connectionString, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {
            SQLiteCommand cmd = new SQLiteCommand();

            using( SQLiteConnection connection = new SQLiteConnection( connectionString ) )
            {
                PrepareCommand( cmd, connection, null, cmdType, cmdText, commandParameters );
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// Execute a SQLiteCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SQLiteParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">an existing database connection</param>
        /// <param name="cmdType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="cmdText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar( SQLiteConnection connection, CommandType cmdType, string cmdText, params SQLiteParameter[] commandParameters )
        {

            SQLiteCommand cmd = new SQLiteCommand();

            PrepareCommand( cmd, connection, null, cmdType, cmdText, commandParameters );
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="commandParameters">an array of SqlParamters to be cached</param>
        public static void CacheParameters( string cacheKey, params SQLiteParameter[] commandParameters )
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">key used to lookup parameters</param>
        /// <returns>Cached SqlParamters array</returns>
        public static SQLiteParameter[] GetCachedParameters( string cacheKey )
        {
            SQLiteParameter[] cachedParms = (SQLiteParameter[])parmCache[cacheKey];

            if( cachedParms == null )
                return null;

            SQLiteParameter[] clonedParms = new SQLiteParameter[cachedParms.Length];

            for( int i = 0, j = cachedParms.Length; i < j; i++ )
                clonedParms[i] = (SQLiteParameter)( (ICloneable)cachedParms[i] ).Clone();

            return clonedParms;
        }

        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SQLiteCommand object</param>
        /// <param name="conn">SQLiteConnection object</param>
        /// <param name="trans">SQLiteTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SQLiteParameters to use in the command</param>
        private static void PrepareCommand( SQLiteCommand cmd, SQLiteConnection conn, SQLiteTransaction trans, CommandType cmdType, string cmdText, SQLiteParameter[] cmdParms )
        {

            if( conn.State != ConnectionState.Open )
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if( trans != null )
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if( cmdParms != null )
            {
                foreach( SQLiteParameter parm in cmdParms )
                    cmd.Parameters.Add( parm );
            }
        }
    }
    #endregion
}