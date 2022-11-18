
using System.Collections.Generic;

namespace EntityFramework.Extension.Procedure.Entity
{
    public class ResultsSet
    {
        readonly List<List<object>> _resultList = new List<List<object>>();

        /// <summary>
        /// Add a results list to the results set
        /// </summary>
        /// <param name="list"></param>
        public void Add(List<object> list)
        {
            _resultList.Add(list);
        }

        /// <summary>
        /// Get the nth results list item
        /// </summary>
        /// <param name="index"></param>
        /// <returns>List of objects that make up the result set</returns>
        public List<object> this[int index]
        {
            get { return _resultList[index]; }
        }
    }
}
