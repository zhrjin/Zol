using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.Common.DBHelper
{
    public abstract class DataBase
    {
        private string _oracleConnectionString = "";

        protected string ConnString { get { return _oracleConnectionString; } }

        public DataBase()
        {
            _oracleConnectionString = Zol.Common.Config.AppConfigHelper.ConnectionString;
        }

        public DataBase(string strConn)
        {
            _oracleConnectionString = strConn;
        }

        /// <summary>
        /// 获取model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T QuerySingle<T>(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                return conn.QueryFirstOrDefault<T>(strSql, param);
            }
        }

        /// <summary>
        /// 获取model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                return conn.QueryFirstOrDefault<T>(strSql, param);
            }
        }

        /// <summary>
        /// 获取model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T QuerySingleProc<T>(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                return conn.QueryFirstOrDefault<T>(strSql, param, commandType: CommandType.StoredProcedure);
            }
        }


        /// <summary>
        /// 获取list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<T> Query<T>(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                return conn.Query<T>(strSql, param).ToList();
            }
        }


        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int Execute(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                var result = conn.Execute(strSql, param: param);
                return result;
            }
        }

        /// <summary>
        /// 执行过程
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ExecuteProc(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                var result = conn.Execute(strSql, param: param, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        /// <summary>
        /// 执行语句，返回第一列
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string strSql, object param = null)
        {
            using (IDbConnection conn = GetConnection())
            {
                var result = conn.ExecuteScalar<T>(strSql, param: param);
                return result;
            }
        }

        /// <summary>
        /// 批量操作功能
        /// </summary>
        public int ExecuteBatch(List<string> sqlList)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sql in sqlList)
                {
                    row += conn.Execute(sql, null, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能
        /// </summary>
        public int TryExecuteBatch(List<string> sqlList)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sql in sqlList)
                {
                    try
                    {
                        row += conn.Execute(sql, null, transaction, null, null);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("数据执行sql=" + sql, ex);
                    }
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能
        /// </summary>
        public int ExecuteBatch<T>(string strSql, List<T> modelList) where T : class
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                row += conn.Execute(strSql, modelList, transaction, null, null);
                //foreach (var param in modelList)
                //{
                //    row += conn.Execute(strSql, param, transaction, null, null);
                //}
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能
        /// </summary>
        public int ExecuteBatch<T>(string strHeaderSql, string strSql, List<T> modelList, string procName, object param = null) where T : class
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                row += conn.Execute(strHeaderSql, null, transaction, null, null);
                row += conn.Execute(strSql, modelList, transaction, null, null);
                if (!string.IsNullOrEmpty(procName))
                {
                    conn.Execute(procName, param: param, transaction: transaction, commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public int ExecuteBatch<T>(Dictionary<string, T> sqlList)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sql in sqlList)
                {
                    row += conn.Execute(sql.Key, sql.Value, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public int ExecuteBatch<T>(Dictionary<string, List<T>> sqlLists)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sqlList in sqlLists)
                {
                    var sqlstr = sqlList.Key;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                    //foreach (var param in sqlList.Value)
                    //{
                    //    row += conn.Execute(sqlstr, param, transaction, null, null);
                    //}
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="sqlList1s"></param>
        /// <param name="sqlList2s"></param>
        /// <returns></returns>
        public int ExecuteBatch<T1, T2>(Dictionary<string, List<T1>> sqlList1s, Dictionary<string, List<T2>> sqlList2s)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sqlList in sqlList1s)
                {
                    var sqlstr = sqlList.Key;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                    //foreach (var param in sqlList.Value)
                    //{
                    //    row += conn.Execute(sqlstr, param, transaction, null, null);
                    //}
                }
                foreach (var sqlList in sqlList2s)
                {
                    var sqlstr = sqlList.Key;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                    //foreach (var param in sqlList.Value)
                    //{
                    //    row += conn.Execute(sqlstr, param, transaction, null, null);
                    //}
                }
                transaction.Commit();
                return row;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="sqlList1s"></param>
        /// <param name="sqlList2s"></param>
        /// <param name="sqlList3s"></param>
        /// <returns></returns>
        public int ExecuteBatch<T1, T2, T3>(Dictionary<string, List<T1>> sqlList1s, Dictionary<string, List<T2>> sqlList2s, Dictionary<string, List<T3>> sqlList3s)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();
                int row = 0;
                foreach (var sqlList in sqlList1s)
                {
                    var sqlstr = sqlList.Key;
                    if (string.IsNullOrEmpty(sqlstr)) continue;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                }
                foreach (var sqlList in sqlList2s)
                {
                    var sqlstr = sqlList.Key;
                    if (string.IsNullOrEmpty(sqlstr)) continue;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                }
                foreach (var sqlList in sqlList3s)
                {
                    var sqlstr = sqlList.Key;
                    if (string.IsNullOrEmpty(sqlstr)) continue;
                    row += conn.Execute(sqlstr, sqlList.Value, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能(表头，明细）
        /// </summary>
        public int ExecuteBatch<TH, TD>(string headStrSql, TH head, List<string> detailSqlList, List<TD> detailList)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();

                int row = 0;
                row += conn.Execute(headStrSql, head, transaction, null, null);
                foreach (var item in detailSqlList)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    row += conn.Execute(item, detailList, transaction, null, null);

                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能(表头，明细, 明细的明细）
        /// </summary>
        public int ExecuteBatch<TH, TD, TDD>(string headStrSql, TH head, List<string> detailSqlList, List<TD> detailList, List<string> dDetailSqlList, List<TDD> dDetailList)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();

                int row = 0;
                row += conn.Execute(headStrSql, head, transaction, null, null);
                foreach (var item in dDetailSqlList)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    row += conn.Execute(item, dDetailList, transaction, null, null);
                }
                foreach (var item in detailSqlList)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    row += conn.Execute(item, detailList, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能(表头，明细1, 明细2, 明细3）
        /// </summary>
        public int ExecuteBatch<TH, TD1, TD2, TD3>(string headStrSql, TH head, List<string> detail1SqlList, List<TD1> detail1List,
            List<string> detail2SqlList, List<TD2> detail2List, List<string> detail3SqlList, List<TD3> detail3List)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();

                int row = 0;
                row += conn.Execute(headStrSql, head, transaction, null, null);
                foreach (var item in detail1SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail1List, transaction, null, null);
                }
                foreach (var item in detail2SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail2List, transaction, null, null);
                }
                foreach (var item in detail3SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail3List, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        /// <summary>
        /// 批量操作功能(表头，明细1, 明细2, 明细3, 明细4）
        /// </summary>
        public int ExecuteBatch<TH, TD1, TD2, TD3, TD4>(string headStrSql, TH head, List<string> detail1SqlList, List<TD1> detail1List,
            List<string> detail2SqlList, List<TD2> detail2List, List<string> detail3SqlList, List<TD3> detail3List, List<string> detail4SqlList, List<TD4> detail4List)
        {
            using (IDbConnection conn = GetConnection())
            {
                IDbTransaction transaction = conn.BeginTransaction();

                int row = 0;
                row += conn.Execute(headStrSql, head, transaction, null, null);
                foreach (var item in detail1SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail1List, transaction, null, null);
                }
                foreach (var item in detail2SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail2List, transaction, null, null);
                }
                foreach (var item in detail3SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail3List, transaction, null, null);
                }
                foreach (var item in detail4SqlList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    row += conn.Execute(item, detail4List, transaction, null, null);
                }
                transaction.Commit();
                return row;
            }
        }

        public int Execute(IDbConnection conn, string strSql, IDbTransaction transaction, object param = null)
        {
            return conn.Execute(strSql, param, transaction, null, null);
        }

        public abstract IDbConnection GetConnection();
    }
}
