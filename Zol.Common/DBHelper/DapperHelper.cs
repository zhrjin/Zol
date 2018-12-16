using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    public class DapperHelper : DataBase
    {
        public DapperHelper() : base()
        {

        }

        public DapperHelper(string strConn) : base(strConn)
        {
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetConnection()
        {
            OracleConnection conn = new OracleConnection(ConnString);
            conn.Open();
            return conn;
        }

        /// <summary>
        ///  执行带参数存储过程，并返回结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<T> QueryProc<T>(string procName, OracleDynamicParameters param)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                var result = conn.Query<T>(procName, param: param, commandType: CommandType.StoredProcedure).ToList();
                transaction.Commit();
                return result;
            }
        }

        /// <summary>
        /// 根据条件莸取数据
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="where"></param>
        /// <param name="sidx"></param>
        /// <param name="sord"></param>
        /// <returns></returns>
        public List<T> QueryProc<T>(string procName, string where, string sidx = "", int sord = 0)
        {
            var p = new OracleDynamicParameters();
            p.Add("wherecase", where);
            p.Add("orderField", sidx);
            p.Add("orderFlag", sord);
            p.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
            return QueryProc<T>(procName, p);
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sidx">排序列 空为不排序</param>
        /// <param name="sord">排序方式  0：正序 1：倒序 </param>
        /// <param name="where">条件</param>
        /// <param name="totalRecored">记录数</param>
        /// <returns></returns>
        public List<T> QueryProc<T>(string procName, int pageIndex, int pageSize, string where, string sidx = "", int sord = 0)
        {
            var p = new OracleDynamicParameters();
            p.Add("wherecase", where);
            p.Add("pageSize", pageSize);
            p.Add("pageNow", pageIndex);
            p.Add("orderField", sidx);
            p.Add("orderFlag", sord);
            p.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            return QueryProc<T>(procName, p);
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sidx">排序列 空为不排序</param>
        /// <param name="sord">排序方式  0：正序 1：倒序 </param>
        /// <param name="where">条件</param>
        /// <param name="totalRecored">记录数</param>
        /// <returns></returns>
        public List<T> QueryProc<T>(string procName, int pageIndex, int pageSize, string where, string where2, string sidx = "", int sord = 0)
        {
            var p = new OracleDynamicParameters();
            p.Add("wherecase", where);
            p.Add("wherecase2", where2);
            p.Add("pageSize", pageSize);
            p.Add("pageNow", pageIndex);
            p.Add("orderField", sidx);
            p.Add("orderFlag", sord);
            p.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

            return QueryProc<T>(procName, p);
        }

        /// <summary>
        ///  执行带参数存储过程，并返回结果 单笔
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T QuerySingleProc<T>(string procName, OracleDynamicParameters param)
        {
            using (IDbConnection conn = GetConnection())
            {
                var result = conn.QuerySingleOrDefault<T>(procName, param: param, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        /// <summary>
        ///  同一个事务，先执行带参数存储过程，再执行查询语句并返回结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="param"></param>
        /// <param name="nextSql"></param>
        /// <returns></returns>
        public List<T> QueryProcTransaction<T>(string procName, OracleDynamicParameters param, string nextSql)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                conn.Execute(procName, param: param, commandType: CommandType.StoredProcedure);
                var result = conn.Query<T>(nextSql).ToList();
                transaction.Commit();
                return result;
            }
        }

        /// <summary>
        /// 增加Sql
        /// </summary>
        /// <param name="type"></param>
        /// <param name="squeName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAddSql(Type type, string squeName = "", string key = "", string[] noAddFileds = null)
        {
            var propArr = type.GetProperties().Select(p => p.Name).ToArray();
            if (noAddFileds != null)
            {
                propArr = propArr.Except(noAddFileds).ToArray();
            }
            var typeName = type.Name;
            var valueString = (":" + string.Join(",:", propArr));
            var insertDetailSql = new StringBuilder();
            insertDetailSql.AppendFormat("insert into {0}(", typeName);
            insertDetailSql.Append(string.Join(",", propArr));
            insertDetailSql.Append(") select ");
            insertDetailSql.Append(valueString);
            insertDetailSql.Append(" from dual");
            var sql = insertDetailSql.ToString();
            if (!string.IsNullOrEmpty(squeName))
                sql = sql.Replace(":" + key, squeName + ".nextVal");
            return sql;
        }
    }

    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        private readonly DynamicParameters _dynamicParameters = new DynamicParameters();

        private readonly List<OracleParameter> _oracleParameters = new List<OracleParameter>();

        public void Add(string name, object value = null, DbType dbType = DbType.AnsiString, ParameterDirection? direction = null, int? size = null)
        {
            _dynamicParameters.Add(name, value, dbType, direction, size);
        }

        public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction)
        {
            var oracleParameter = new OracleParameter(name, oracleDbType) { Direction = direction };
            _oracleParameters.Add(oracleParameter);
        }

        public void Add(string name, OracleDbType oracleDbType, int size, ParameterDirection direction)
        {
            var oracleParameter = new OracleParameter(name, oracleDbType, size) { Direction = direction };
            _oracleParameters.Add(oracleParameter);
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            ((SqlMapper.IDynamicParameters)_dynamicParameters).AddParameters(command, identity);

            var oracleCommand = command as OracleCommand;

            if (oracleCommand != null)
            {
                oracleCommand.Parameters.AddRange(_oracleParameters.ToArray());
            }
        }

        public T Get<T>(string parameterName)
        {
            var parameter = _oracleParameters.SingleOrDefault(t => t.ParameterName == parameterName);
            if (parameter != null)
                return (T)Convert.ChangeType(parameter.Value, typeof(T));
            return default(T);
        }

        public T Get<T>(int index)
        {
            var parameter = _oracleParameters[index];
            if (parameter != null)
                return (T)Convert.ChangeType(parameter.Value, typeof(T));
            return default(T);
        }
    }

}
