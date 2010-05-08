﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.Cassandra;
using FluentCassandra.Types;
using FluentCassandra.Operations.Helpers;

namespace FluentCassandra.Operations
{
	public class GetSuperColumnSlice<CompareWith, CompareSubcolumnWith> : ColumnFamilyOperation<FluentSuperColumn<CompareWith, CompareSubcolumnWith>>
		where CompareWith : CassandraType
		where CompareSubcolumnWith : CassandraType
	{
		/*
		 * list<ColumnOrSuperColumn> get_slice(keyspace, key, column_parent, predicate, consistency_level)
		 */

		public string Key { get; private set; }

		public CassandraType SuperColumnName { get; private set; }

		public CassandraSlicePredicate SlicePredicate { get; private set; }

		public override FluentSuperColumn<CompareWith, CompareSubcolumnWith> Execute(BaseCassandraColumnFamily columnFamily)
		{
			var result = new FluentSuperColumn<CompareWith, CompareSubcolumnWith>(GetColumns(columnFamily));
			columnFamily.Context.Attach(result);
			result.MutationTracker.Clear();

			return result;
		}

		private IEnumerable<IFluentColumn<CompareSubcolumnWith>> GetColumns(BaseCassandraColumnFamily columnFamily)
		{
			var parent = new ColumnParent {
				Column_family = columnFamily.FamilyName
			};

			if (SuperColumnName != null)
				parent.Super_column = SuperColumnName;

			var output = columnFamily.GetClient().get_slice(
				columnFamily.Keyspace.KeyspaceName,
				Key,
				parent,
				SlicePredicate.CreateSlicePredicate(),
				ConsistencyLevel
			);

			foreach (var result in output)
			{
				var r = ObjectHelper.ConvertColumnToFluentColumn<CompareSubcolumnWith>(result.Column);
				yield return r;
			}
		}

		public GetSuperColumnSlice(string key, CassandraType superColumnName, CassandraSlicePredicate columnSlicePredicate)
		{
			this.Key = key;
			this.SuperColumnName = superColumnName;
			this.SlicePredicate = columnSlicePredicate;
		}
	}
}