using EntityFramework.Extension.Procedure.Entity;
using EntityFramework.Extension.Procedure.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Extension.Procedure
{
    //This is an extension class for getting multiple result set from a single sp.
    public static class StoredProcUtility
    {
        /// <summary>
        /// Set up the DBCommand object, adding in options and parameters
        /// </summary>
        /// <param name="procName">Name of the stored proc</param>
        /// <param name="parms">SQLParameters to pass. Override: no parm created for TVP that is DBNull</param>
        /// <param name="commandTimeout">Set Command Timeout Override</param>
        /// <param name="transaction">Transaction in which to enroll this proc call</param>
        /// <param name="cmd">DBCommand object representing the command to the database</param>
        private static void SetupStoredProcCall(string procName, IEnumerable<SqlParameter> parms, int? commandTimeout, DbTransaction transaction, DbCommand cmd)
        {
            cmd.Transaction = transaction;
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;

            // Assign command timeout value, if one was provided
            cmd.CommandTimeout = commandTimeout ?? cmd.CommandTimeout;

            if (null != parms)
                foreach (SqlParameter p in parms)
                {
                    // Don't send any parm for null table-valued parameters
                    if (!(SqlDbType.Structured == p.SqlDbType && DBNull.Value == p.Value))
                    {
                        cmd.Parameters.Add(p);
                    }
                }
        }

        /// <summary>
        /// public
        ///
        /// Call a stored procedure and get results back. - async version
        /// </summary>
        /// <param name="context">Code First database context object</param>
        /// <param name="procName">Qualified name of proc to call</param>
        /// <param name="token">Cancellation token for asyc process cancellation</param>
        /// <param name="parms">List of ParameterHolder objects - input and output parameters</param>
        /// <param name="commandTimeout">Timeout for stored procedure call</param>
        /// <param name="commandBehavior"></param>
        /// <param name="transaction">Sql transaction in which to enroll the stored procedure call</param>
        /// <param name="outputTypes">List of types to expect in return. Each type *must* have a default constructor.</param>
        /// <returns></returns>
        public static async Task<ResultsSet> ReadFromStoredProcAsync(this DbContext context, string procName, CancellationToken token, 
            IEnumerable<SqlParameter> parms = null, int? commandTimeout = null, CommandBehavior commandBehavior = CommandBehavior.Default, 
            DbTransaction transaction = null, params Type[] outputTypes)
        {
            var results = new ResultsSet();
            var currentType = (null == outputTypes) ? Type.EmptyTypes.GetEnumerator() : outputTypes.GetEnumerator();
            var connection = context.Database.GetDbConnection();
            var closeConnection = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(token);
                    closeConnection = true;
                }

                using (var cmd = connection.CreateCommand())
                {
                    SetupStoredProcCall(procName, parms, commandTimeout, transaction, cmd);
                    var reader = await cmd.ExecuteReaderAsync(commandBehavior, token);

                    // get the type we're expecting for the first result. If no types specified,
                    // ignore all results
                    if (currentType.MoveNext())
                    {
                        // process results - repeat this loop for each result set returned by the stored proc
                        // for which we have a result type specified
                        do
                        {
                            PropertyInfo[] propertyList = null;

                            var current = new List<object>();
                            while (await reader.ReadAsync(token))
                            {
                                var item = ((Type)currentType.Current)?.GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]);
                                propertyList = propertyList ?? reader.MatchRecordProperties(item, currentType);
                                await reader.ReadRecordAsync(item, propertyList, token);
                                current.Add(item);
                            }

                            results.Add(current);
                        } while (await reader.NextResultAsync(token) && currentType.MoveNext());
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading from stored proc " + procName + ": " + ex.Message, ex);
            }
            finally
            {
                if (closeConnection)
                    connection.Close();
            }

            return results;
        }
    }
}
