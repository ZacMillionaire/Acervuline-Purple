using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Acervuline {
	class ParseToDB {

		private MySqlConnection connection;
		string myConnectionString = "server=127.0.0.1;uid=root; pwd=;database=acervuline;";

		public bool Insert(Dictionary<string, dynamic> unitDict) {

			try {

				connection = new MySqlConnection();
				connection.ConnectionString = myConnectionString;
				connection.Open();
				
				MySqlCommand sql = new MySqlCommand(
					string.Format(
						"INSERT INTO `acervuline`.`unitdata`(`unitCode`,`unitName`,`CSP`,`DOM`,`INT`,`synopsis`,`creditPoints`)VALUES('{0}','{1}',{2},{3},{4},'{5}',{6});",
						unitDict["unitCode"],
						unitDict["title"],
						int.Parse(unitDict["CSP"]),
						int.Parse(unitDict["DOM"]),
						int.Parse(unitDict["INT"]),
						unitDict["synopsis"],
						int.Parse(unitDict["CP"])
					)
				);

				sql.Connection = connection;
				sql.ExecuteNonQuery();
				long ID = sql.LastInsertedId;
				
				connection.Close();

				return true;
			} catch(MySqlException) {
				return false;
			}

		}
	}
}
