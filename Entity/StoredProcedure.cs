using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace EntityFramework.Extension.Procedure.Entity
{
    public class StoredProc
    {

        /// <summary>
        /// Database owner of this object
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Name of the stored procedure
        /// </summary>
        public string ProcName { get; set; }

        /// <summary>
        /// Fluent API - assign owner (schema)
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public StoredProc HasOwner(string owner)
        {
            Schema = owner;
            return this;
        }

        /// <summary>
        /// Fluent API - assign procedure name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StoredProc HasName(string name)
        {
            ProcName = name;
            return this;
        }

        /// <summary>
        /// Fluent API - set the data types of resultsets returned by the stored procedure.
        /// Order is important! Note that the return type objects must have a default constructor!
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public StoredProc ReturnsTypes(params Type[] types)
        {
            OutputTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Command Behavior for
        /// </summary>
        public CommandBehavior CommandBehavior { get; set; }

        /// <summary>
        /// Get the fully (schema plus owner) name of the stored procedure
        /// </summary>
        public string FullName
        {
            get { return Schema + "." + ProcName; }
        }

        /// <summary>
        /// Tranasaction to enroll the sqlcommand in; required if using
        /// connection.BeginTransaction or database.BeginTransaction instead of TransactionScope.
        /// </summary>
        public DbTransaction Transaction { get; set; }
        public StoredProc()
        {
            Schema = "dbo";
        }

        /// <summary>
        /// List of data types that this stored procedure returns as result sets.
        /// Order is important!
        /// </summary>
        public List<Type> OutputTypes = new List<Type>();

        /// <summary>
        /// Get an array of types returned
        /// </summary>
        public Type[] ReturnTypes
        {
            get { return OutputTypes.ToArray(); }
        }
    }
}
