﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System.Text;

namespace DBDocs_Editor
{
    public class ProgSettings
    {   // Common Functions will be added in here
        private static string dbName = "";
        private static string serverName = "";
        private static string userName = "";
        private static string password = "";
        public static string sqldBconn = "";
        public static Form mainForm;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public static string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                if (dbName != "*")
                {
                    sqldBconn = "SERVER=" + serverName + ";DATABASE=" + dbName + ";UID=" + userName + ";PWD=" + password + ";";
                }
                else
                {
                    sqldBconn = "SERVER=" + serverName + ";UID=" + userName + ";PWD=" + password + ";";
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public static string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                if (dbName != "*")
                {
                    sqldBconn = "SERVER=" + serverName + ";DATABASE=" + dbName + ";UID=" + userName + ";PWD=" + password + ";";
                }
                else
                {
                    sqldBconn = "SERVER=" + serverName + ";UID=" + userName + ";PWD=" + password + ";";
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        /// <value>The name of the server.</value>
        public static string ServerName
        {
            get
            {
                return serverName;
            }
            set
            {
                serverName = value;
                if (dbName != "*")
                {
                    sqldBconn = "SERVER=" + serverName + ";DATABASE=" + dbName + ";UID=" + userName + ";PWD=" + password + ";";
                }
                else
                {
                    sqldBconn = "SERVER=" + serverName + ";UID=" + userName + ";PWD=" + password + ";";
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the db.
        /// </summary>
        /// <value>The name of the db.</value>
        public static string DbName
        {
            get
            {
                return dbName;
            }
            set
            {
                dbName = value;
                if (dbName != "*")
                {
                    sqldBconn = "SERVER=" + serverName + ";DATABASE=" + dbName + ";UID=" + userName + ";PWD=" + password + ";";
                }
                else
                {
                    sqldBconn = "SERVER=" + serverName + ";UID=" + userName + ";PWD=" + password + ";";
                }
            }
        }

        /// <summary>
        /// Returns a dataset contain the results of the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DataSet SelectRows(string query)
        {
            var conn = new MySqlConnection(sqldBconn);
            var adapter = new MySqlDataAdapter();
            var myDataset = new DataSet();
            adapter.SelectCommand = new MySqlCommand(query, conn);
            try
            {
                adapter.Fill(myDataset);
            }
            catch (Exception ex)
            {
                myDataset = null;
                MessageBox.Show("There was an error connecting to the database: \n" + ex.Message);
            }
            adapter.Dispose();
            conn.Close();
            return myDataset;
        }

        #region "dbdocsTable Functions"
        /// <summary>
        /// Inserts a record into the dbdocstable table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="languageId"></param>
        /// <param name="tableNotes"></param>
        public static void TableInsert(string tableName, int languageId, string tableNotes)
        {
            
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();

            cmd.CommandText = "insert  into `dbdocstable`(`languageId`,`tableName`,`tableNotes`) "
                              + "VALUES (@languageId, @tablename, @tablenotes)";

            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@tablename", tableName);
            cmd.Parameters.AddWithValue("@tablenotes", tableNotes);

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        /// <summary>
        /// Update a record in the dbdocstable or dbdocstable_localised table
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="languageId"></param>
        /// <param name="tableNotes"></param>
        public static void TableUpdate(int tableId, int languageId, string tableNotes)
        {
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();

            // Does this entry exist as a main table entry ?
            if (LookupTableEntry(languageId, tableId) == true)
            {
                // Update dbdocs
                cmd.CommandText = "update `dbdocstable` set `tablenotes`=@tableNotes where tableId=@tableId and languageId=@languageId";
            }
            else
            {   // Does this entry exist as a localised entry ?
                if (LookupTableEntryLocalised(languageId, tableId) == true)
                {
                    // update dbdocs_localised
                    cmd.CommandText = "update `dbdocstable_localised` set `tablenotes`=@tableNotes where tableId=@tableId and languageId=@languageId";
                }
                else
                { 
                    // insert into dbdocs_localised
                    cmd.CommandText = "insert  into `dbdocstable_localised`(`tableId`,`languageId`,`tableNotes`) "
                                    + "VALUES (@tableId, @languageId, @tablenotes)";
                }
            }

            cmd.Parameters.AddWithValue("@tableId", tableId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@tablenotes", tableNotes);

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        /// <summary>
        /// Check whether the data exists in the localised or main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupTableEntryLocalised(int languageId, int tableId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT tableNotes FROM dbdocstable_localised where tableId=" + tableId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Returns the TableId for a given TableName
        /// </summary>
        /// <param name="subTableName"></param>
        /// <returns></returns>
        public static int LookupTableId(string tableName)
        {
            System.Data.DataSet dbViewSubtable = new System.Data.DataSet();
            dbViewSubtable = ProgSettings.SelectRows("SELECT tableid FROM dbdocstable where tableName='" + tableName + "';");
            if (dbViewSubtable != null)
            {
                if (dbViewSubtable.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToInt32(dbViewSubtable.Tables[0].Rows[0]["tableid"]);
                }
            }
            return 0;
        }

        /// <summary>
        /// Check whether the data exists in the main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupTableEntry(int languageId, int tableId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT tableNotes FROM dbdocstable where tableId=" + tableId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        #endregion "dbdocsTable Functions"

        #region "dbdocsfields Functions"

        /// <summary>
        /// Inserts a record into the dbdocsfields table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldNotes">The field notes.</param>
        public static void FieldInsert(int languageId, string tableName, string fieldName, string FieldComment, string fieldNotes)
        {
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();
            cmd.CommandText = "insert  into `dbdocsfields`(`languageId`,`tableName`,`fieldName`,`FieldComment`,`fieldNotes`) "
                              + "VALUES (@languageId,@tablename, @fieldname, @FieldComment, @fieldnotes)";

            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@tablename", tableName);
            cmd.Parameters.AddWithValue("@fieldName", fieldName);
            cmd.Parameters.AddWithValue("@FieldComment", FieldComment);
            cmd.Parameters.AddWithValue("@fieldNotes", fieldNotes);

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        /// <summary>
        /// Updates a record in the dbdocsfields table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldNotes">The field notes.</param>
        public static void FieldUpdate(int fieldId, int languageId, string FieldComment, string fieldNotes)
        {
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();

            // Does this entry exist as a main table entry ?
            if (LookupFieldEntry(languageId, fieldId) == true)
            {
                // Update dbdocs
                cmd.CommandText = "update `dbdocsfields` set `FieldComment`=@FieldComment, `fieldnotes`=@fieldNotes where `fieldId`=@fieldId and `languageId`=@languageId;";
            }
            else
            {   // Does this entry exist as a localised entry ?
                if (LookupFieldEntryLocalised(languageId, fieldId) == true)
                {
                    // update dbdocs_localised
                    cmd.CommandText = "update `dbdocsfields_localised` set `FieldComment`=@FieldComment, `fieldnotes`=@fieldNotes where `fieldId`=@fieldId and `languageId`=@languageId;";
                }
                else
                {
                    // insert into dbdocs_localised
                    cmd.CommandText = "insert into `dbdocsfields_localised` (`fieldId`,`languageId`,`FieldComment`,`fieldNotes`) "
                                    + "VALUES (@fieldId, @languageId, @FieldComment, @fieldNotes)";
                }
            }

            

            cmd.Parameters.AddWithValue("@fieldId", fieldId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@FieldComment", FieldComment);
            cmd.Parameters.AddWithValue("@fieldNotes", fieldNotes);

            try
            { 
                cmd.ExecuteNonQuery(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Saving: " + ex.Message);
            }
            cmd.Connection.Close();
        }

        /// <summary>
        /// Check whether the data exists in the localised or main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupFieldEntryLocalised(int languageId, int fieldId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT fieldNotes FROM dbdocsfields_localised where fieldId=" + fieldId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Check whether the data exists in the main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupFieldEntry(int languageId, int fieldId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT fieldNotes FROM dbdocsfields where fieldId=" + fieldId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the FieldId for a given FieldName
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static int LookupFieldId(string tableName, string fieldName)
        {
            System.Data.DataSet dbViewSubtable = new System.Data.DataSet();
            dbViewSubtable = ProgSettings.SelectRows("SELECT fieldid FROM dbdocsfields where tableName='" + tableName + "' and fieldname='" + fieldName + "';");
            if (dbViewSubtable != null)
            {
                if (dbViewSubtable.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToInt32(dbViewSubtable.Tables[0].Rows[0]["fieldId"]);
                }
            }
            return 0;
        }


        #endregion "dbdocsfields Functions"
        
        #region "dbdocssubTable Functions"
        /// <summary>
        /// Inserts a record into the dbdocsfields table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldNotes">The field notes.</param>
        public static void SubTableInsert(int subTableId, int languageId, string subTableName, string subTableContent, string subTableTemplate)
        {
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();
            cmd.CommandText = "insert  into `dbdocssubtables`(`subtableid`,`languageId`,`subtableName`,`subtablecontent`,`subtabletemplate`) "
                              + "VALUES (@subtableid, @languageId, @subtableName, @subtablecontent, @subtabletemplate)";

            cmd.Parameters.AddWithValue("@subtableid", subTableId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@subtableName", subTableName);
            cmd.Parameters.AddWithValue("@subtablecontent", subTableContent);
            cmd.Parameters.AddWithValue("@subtabletemplate", subTableTemplate);

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

        /// <summary>
        /// Updates a record in the dbdocsfields table
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldNotes">The field notes.</param>
        public static void SubTableUpdate(int subTableId, int languageId, string subTableName, string subTableContent, string subTableTemplate)
        {
            var conn = new MySqlConnection(sqldBconn);
            var cmd = new MySqlCommand("", conn);
            cmd.Connection.Open();

            // Does this entry exist as a main table entry ?
            if (LookupSubTableEntry(languageId, subTableId) == true)
            {
                // Update dbdocs
                cmd.CommandText = "update `dbdocssubtables` set `subTableName`=@subTableName,`subTableContent`=@subTableContent,`subTableTemplate`=@subTableTemplate where subTableId=@subTableId and languageId=@languageId;";
            }
            else
            {   // Does this entry exist as a localised entry ?
                if (LookupSubTableEntryLocalised(languageId, subTableId) == true)
                {
                    // update dbdocs_localised
                    cmd.CommandText = "update `dbdocssubtables_localised` set `subTableContent`=@subTableContent,`subTableTemplate`=@subTableTemplate where subTableId=@subTableId and languageId=@languageId;";
                }
                else
                {
                    // insert into dbdocs_localised
                    cmd.CommandText = "insert  into `dbdocssubtables_localised`(`subtableid`,`languageId`,`subtablecontent`,`subtabletemplate`) "
                                      + "VALUES (@subtableid, @languageId, @subtablecontent, @subtabletemplate)";
                }
            }
            
            cmd.Parameters.AddWithValue("@subtableid", subTableId);
            cmd.Parameters.AddWithValue("@languageId", languageId);
            cmd.Parameters.AddWithValue("@subtableName", subTableName);
            cmd.Parameters.AddWithValue("@subTableContent", subTableContent);
            cmd.Parameters.AddWithValue("@subTableTemplate", subTableTemplate);

            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }


        /// <summary>
        /// Check whether the data exists in the localised or main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupSubTableEntryLocalised(int languageId, int subtableId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT subtabletemplate FROM dbdocssubtables_localised where subtableId=" + subtableId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Check whether the data exists in the main table
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public static bool LookupSubTableEntry(int languageId, int subtableId)
        {
            System.Data.DataSet dbView = new System.Data.DataSet();
            dbView = ProgSettings.SelectRows("SELECT subtablename FROM dbdocssubtables where subtableId=" + subtableId + " and languageId=" + languageId + ";");
            if (dbView != null)
            {
                if (dbView.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
                #endregion "dbdocsTable Functions"

        /// <summary>
        /// Redisplays the form 'formName' passed
        /// </summary>
        /// <param name="formName"></param>
        public static void ShowThisForm(Form formName)
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm == formName)
                {
                    frm.Show();
                    return;
                }
            }
        }

        /// <summary>
        /// Writes the connection setting into the registry.
        /// </summary>
        public static void WriteRegistry()
        { // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\MaNGOS\\DBDocsEditor\\Connections";
            const string keyName = userRoot + "\\" + subkey;

            // Concat all the login requirements into a single field
            string[] connectionString = { serverName, userName, password, dbName };

            // Write in into the registry as a key call "<servername>-<dbname>"
            Registry.SetValue(keyName, serverName + "-" + DbName, connectionString);

            // Rebuild the internal list of connections
            PopulateConnections();
        }

        /// <summary>
        /// Deletes the connection setting from the registry. Has Two uses: a) The delete the selected connect entry, b) To remove the old entry when a server connnection is renamed
        /// </summary>
        /// <param name="keyname">The keyname.</param>
        public static void DeleteConnection(string keyname)
        {
            const string subkey = "Software\\MaNGOS\\DBDocsEditor\\Connections";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subkey, true))
            {
                if (key == null)
                {
                    // Key doesn't exist. Do whatever you want to handle this case
                }
                else
                {
                    key.DeleteValue(keyname);
                }
            }
        }

        /// <summary>
        /// Populates the connection information from the registry and returns a list of the entries.
        /// </summary>
        /// <returns></returns>
        public static List<ConnectionInfo> PopulateConnections()
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\MaNGOS\\DBDocsEditor\\Connections";

            RegistryKey rk = Registry.CurrentUser;

            // Open a subKey as read-only
            RegistryKey dbDocsKey = rk.OpenSubKey(subkey);
            if (dbDocsKey != null)
            {
                var names = dbDocsKey.GetValueNames();

                if (names.GetLength(0) > 0)
                {
                    var allconnections = new List<ConnectionInfo>();
                    var connections = new ConnectionInfo();

                    // For every Connection entry in the registry
                    foreach (String connectionstring in names)
                    {
                        var theseValues = (string[])Registry.GetValue(userRoot + "\\" + subkey, connectionstring, "");

                        // Populate a connectionInfo structure with the values from the registry
                        connections.ServerNameorIp = theseValues[0];
                        connections.DatabaseUserName = theseValues[1];
                        connections.DatabasePassword = theseValues[2];
                        connections.DatabaseName = theseValues[3];

                        // Add the structure to a list
                        allconnections.Add(connections);
                    }
                    return allconnections;
                }
            }
            return null;
        }

        /// <summary>
        /// Extracts the subtableId's from subtable markup and returns them comma delimited
        /// </summary>
        /// <param name="subTableText"></param>
        /// <returns></returns>
        private static string GetSubtables(string subTableText)
        {
            var startPos = 1;
            var retString = "";

            while (startPos > 0)
            {        //¬subtable:xxx¬
                startPos = subTableText.IndexOf("¬subtable:", startPos + 1, System.StringComparison.Ordinal);
                if (startPos > 0)
                {
                    startPos = startPos + 10;
                }
                if (startPos <= 0) continue;

                var endPos = subTableText.IndexOf("¬", startPos + 1, System.StringComparison.Ordinal);
                retString = retString + subTableText.Substring(startPos, endPos - startPos) + ",";
            }
            return retString;
        }

        /// <summary>
        /// Populates a listbox with the name of each subtable entry found in text
        /// </summary>
        /// <param name="SourceText"></param>
        /// <param name="lstSubTables"></param>
        public static void ExtractSubTables(string SourceText, ListBox lstSubTables)
        {
            lstSubTables.Items.Clear();
            if (SourceText.Contains("¬subtable:"))
            {
                string subtables = GetSubtables(SourceText);
                string[] stringSeparators = new string[] { "," };
                string[] subarray = subtables.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string tableid in subarray)
                {
                    if (!string.IsNullOrEmpty(tableid))
                    {
                        System.Data.DataSet dbViewSubtable = new System.Data.DataSet();
                        dbViewSubtable = ProgSettings.SelectRows("SELECT subtablename FROM dbdocssubtables where subtableid=" + tableid);
                        if (dbViewSubtable != null)
                        {
                            if (dbViewSubtable.Tables[0].Rows.Count > 0)
                            {
                                lstSubTables.Items.Add(subtables.Replace(",", "") + ":" + dbViewSubtable.Tables[0].Rows[0]["subtablename"].ToString());
                            }
                        }
                    }
                }
//                MessageBox.Show("Found subtables: " + subtables);
            }
        }

        /// <summary>
        /// Populate the Language Listboxes
        /// </summary>
        /// <param name="lstLangs"></param>
        public static void LoadLangs(ListBox lstLangs)
        {
            // The following command reads all the columns for the selected table
            var dbViewList = ProgSettings.SelectRows("SELECT * FROM dbdocslanguage");

            // Did we return anything
            if (dbViewList != null)
            {
                // Do we have rows
                if (dbViewList.Tables[0].Rows.Count > 0)
                {
                    lstLangs.Items.Clear();

                    // for each Field returned, populate the listbox with the table name
                    for (var thisRow = 0; thisRow <= dbViewList.Tables[0].Rows.Count - 1; thisRow++)
                    {
                        var tableName = dbViewList.Tables[0].Rows[thisRow]["LanguageName"].ToString();
                        lstLangs.Items.Add(tableName);
                    }
                }
            }
            
            // Should something horrible go wrong, this will attempt to at least provide an option for english
            if (lstLangs.Items.Count > 0)
            { 
                if (lstLangs.SelectedIndex < 0) lstLangs.SelectedIndex = 0; 
            }
            else
            {
                lstLangs.Items.Add("English");
            }
        }

        /// <summary>
        /// Populate the Language Listboxes
        /// </summary>
        /// <param name="lstLangs"></param>
        public static void LoadLangs(ToolStripComboBox lstLangs)
        {
            // The following command reads all the columns for the selected table
            var dbViewList = ProgSettings.SelectRows("SELECT * FROM dbdocslanguage");

            // Did we return anything
            if (dbViewList != null)
            {
                // Do we have rows
                if (dbViewList.Tables[0].Rows.Count > 0)
                {
                    lstLangs.Items.Clear();

                    // for each Field returned, populate the listbox with the table name
                    for (var thisRow = 0; thisRow <= dbViewList.Tables[0].Rows.Count - 1; thisRow++)
                    {
                        var tableName = dbViewList.Tables[0].Rows[thisRow]["LanguageName"].ToString();
                        lstLangs.Items.Add(tableName);
                    }
                }
            }

            // Should something horrible go wrong, this will attempt to at least provide an option for english
            if (lstLangs.Items.Count > 0)
            {
                if (lstLangs.SelectedIndex < 0) lstLangs.SelectedIndex = 0;
            }
            else
            {
                lstLangs.Items.Add("English");
            }
        }

        /// <summary>
        /// Looks up the next available subtableId
        /// </summary>
        /// <returns></returns>
        public static string GetNewSubTableId()
        {
            System.Data.DataSet dbViewSubtable = new System.Data.DataSet();
            dbViewSubtable = ProgSettings.SelectRows("SELECT max(subtableid) as MaxId FROM dbdocssubtables");
            if (dbViewSubtable != null)
            {
                if (dbViewSubtable.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToString(Convert.ToInt32(dbViewSubtable.Tables[0].Rows[0]["MaxId"].ToString()) + 1);
                }
            }
            return "0";
        }

        /// <summary>
        /// Returns the subTableId for a given subTableName
        /// </summary>
        /// <param name="subTableName"></param>
        /// <returns></returns>
        public static string LookupSubTableId(string subTableName)
        {
            System.Data.DataSet dbViewSubtable = new System.Data.DataSet();
            dbViewSubtable = ProgSettings.SelectRows("SELECT subtableid FROM dbdocssubtables where subtableName='" + subTableName + "';");
            if (dbViewSubtable != null)
            {
                if (dbViewSubtable.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToString(dbViewSubtable.Tables[0].Rows[0]["SubTableId"].ToString());
                }
            }
            return "0";
        }

        /// <summary>
        /// Attempts to Create a subtable template from a HTML table. This is messy and horrible. !!
        /// - It is only used to attempt to convert the HTML table into the Template format, some user cleanup may be required.
        /// </summary>
        /// <param name="inText"></param>
        /// <returns></returns>
        public static string ConvertHtmlToTemplate(string inText)
        {
            inText = inText.Replace("valign=\"middle\"", "");
            inText = inText.Replace("valign='middle'", "");
            inText = inText.Replace("<tr bgcolor='#f0f0ff'>", "<tr>");
            inText = inText.Replace("<tr bgcolor=\"#f0f0ff\">", "<tr>");
            inText = inText.Replace("<tr bgcolor='#FFFFEE'>", "<tr>");
            inText = inText.Replace("<tr bgcolor=\"#FFFFEE\">", "<tr>");
            inText = inText.Replace("<tr bgcolor='#FEFEFF'>", "<tr>");
            inText = inText.Replace("<tr bgcolor=\"#FEFEFF\">", "<tr>");

            inText = inText.Replace("</table>", "");

            inText = inText.Replace("<th align=left>", "<th align=left><");
            inText = inText.Replace("<th align='left'>", "<th align='left'><");
            inText = inText.Replace("<th align=\"left\">", "<th align=\"left\"><");

            inText = inText.Replace("<th align=right>", "<th align=right>>");
            inText = inText.Replace("<th align='right'>", "<th align='right'>>");
            inText = inText.Replace("<th align=\"right\">", "<th align=\"right\">>");


            // Clean up the alignment stuff
            inText = inText.Replace("align='left'", "");
            inText = inText.Replace("align=\"left\"", "");

            inText = inText.Replace("align='centre'", "");
            inText = inText.Replace("align=\"centre\"", "");
            inText = inText.Replace("align='center'", "");
            inText = inText.Replace("align=\"center\"", "");

            inText = inText.Replace("align='right'", "");
            inText = inText.Replace("align=\"right\"", "");

            // Clean up iffy td entry
            while (inText.Contains(" >"))
            {
                inText = inText.Replace(" >", ">");
            }
            inText = inText.Replace("</td><td>", "|");
            inText = inText.Replace("</td></tr>", "");
            inText = inText.Replace("<tr><td>", "");

            // Clean up Table heading stuff
            inText = inText.Replace("<th><b>", "<th>");
            inText = inText.Replace("<<b>", "<");
            inText = inText.Replace("</b></th>", "</th>");
            inText = inText.Replace("<tr>" + "\r\n" + "<th>", "<th>");
            inText = inText.Replace("<tr>" + "\r" + "<th>", "<th>");
            inText = inText.Replace("<tr>" + "\n" + "<th>", "<th>");

            inText = inText.Replace("</th>" + "\r\n" + "<th>", "|");
            inText = inText.Replace("</th>" + "\r" + "<th>", "|");
            inText = inText.Replace("</th>" + "\n" + "<th>", "|");

            inText = inText.Replace("</th><th>", "|");

            inText = inText.Replace("<tr><th>", "");
            inText = inText.Replace("</th></tr>", "|");

            inText = inText.Replace("<th>", "");
            inText = inText.Replace("</th>", "");
            inText = inText.Replace("<tr></tr>", "");
            inText = inText.Replace("\r\n" + "\r\n", "\r\n");
            inText = inText.Replace("\r" + "\r", "\r");
            inText = inText.Replace("\n" + "\n", "\n");


            if (inText.Contains("<table"))
            {
                int startPos = inText.IndexOf("<table");
                int endPos = inText.IndexOf(">", startPos + 1) + 1;
                string sourceText = inText.Substring(startPos, endPos - startPos);
                inText = inText.Replace(sourceText, "").Trim();

            }
            inText = inText.Replace("><b>", ">");
            return inText;
        }

        /// <summary>
        /// Converts the Template markup to HTML
        /// </summary>
        /// <param name="templateText"></param>
        /// <returns></returns>
        public static string ConvertTemplateToHtml(string templateHeader, string templateText)
        {
            bool blnBuild = false;
            StringBuilder sbHtml = new StringBuilder();
            string[] strHeaders = null;
            if (templateHeader.EndsWith("|"))
                templateHeader = templateHeader.Substring(0, templateHeader.Length - 1);

            string[] stringSeparators = new string[] { "|" };
            strHeaders = templateHeader.Trim().Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            
            string[] strHeaderCols = null;

            if (templateText.Contains("|"))
                blnBuild = true;

            if (blnBuild == true)
            {
                sbHtml.AppendLine("<table border='1' cellspacing='1' cellpadding='3' bgcolor='#f0f0f0'>");
                sbHtml.AppendLine("<tr bgcolor='#f0f0ff'>");
                //"<thead>")

                //Process the headers
                strHeaderCols = new string[strHeaders.GetUpperBound(0) + 1];
                for (int intHead = 0; intHead <= strHeaders.GetUpperBound(0); intHead++)
                {
                    if (strHeaders[intHead].StartsWith("<"))
                    {
                        sbHtml.AppendLine("<th align='left'><b>" + strHeaders[intHead].Substring(1) + "</b></th>");
                        strHeaderCols[intHead] = "1";
                    }
                    else if (strHeaders[intHead].StartsWith(">"))
                    {
                        sbHtml.AppendLine("<th align='right'><b>" + strHeaders[intHead].Substring(1) + "</b></th>");
                        strHeaderCols[intHead] = "2";
                    }
                    else
                    {
                        sbHtml.AppendLine("<th><b>" + strHeaders[intHead] + "</b></th>");
                    }
                }
                //SbHtml.AppendLine("</thead>")
            }

            //Now need to process the body
            string[] strLines = null;
            string[] strBody = null;
            string[] stringSeparators2 = new string[] { "\r\n" };
            strLines = templateText.Split(stringSeparators2, StringSplitOptions.RemoveEmptyEntries);

            bool resyncError = false;
            for (int intLines = 0; intLines <= strLines.GetUpperBound(0); intLines++)
            {
                if (blnBuild == true)
                {
                    try
                    {
                        strBody = strLines[intLines].Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            
                        if (intLines % 2 == 0)
                        {
                            //               sbHtml.Append("<tr bgcolor='#FAF8FA'>")
                            sbHtml.Append("<tr bgcolor='#FFFFEE'>");
                        }
                        else
                        {
                            //                sbHtml.Append("<tr bgcolor='#F8FAFA'>")
                            sbHtml.Append("<tr bgcolor='#FEFEFF'>");
                        }

                        for (int intFields = 0; intFields <= strBody.GetUpperBound(0); intFields++)
                        {
                            try
                            {
                                strBody[intFields] = strBody[intFields].Trim();
                                if (strBody[intFields].Length > 0)
                                {
                                    if (strHeaderCols[intFields] == "1")
                                    {
                                        sbHtml.Append("<td align='left' valign='middle'>" + strBody[intFields] + "</td>");
                                    }
                                    else if (strHeaderCols[intFields] == "2")
                                    {
                                        sbHtml.Append("<td align='right' valign='middle'>" + strBody[intFields] + "</td>");
                                    }
                                    else
                                    {
                                        sbHtml.Append("<td align='center' valign='middle'>" + strBody[intFields] + "</td>");
                                    }
                                }
                            }
                            catch 
                            {
                                resyncError = true;
                            }    
                        }
                        sbHtml.AppendLine("</tr>");
                
                    }
                    catch 
                    {
                        resyncError = true;
                    }

                }
                else
                {
                    sbHtml.AppendLine(strLines[intLines]);
                }
            }
            if (blnBuild == true)
                sbHtml.AppendLine("</table>");

            if (resyncError == true) 
            { 
                MessageBox.Show("Failures during Resync"); 
            }

            return sbHtml.ToString();
        }


        /// <summary>
        /// Replace ' with an escaped \' so SQL is happy
        /// </summary>
        /// <param name="inSQL"></param>
        /// <returns></returns>
        public static string PrepareSqlString(string inSQL)
        {
            inSQL = inSQL.Replace("\'", "\\'");
            return inSQL;
        }

        /// <summary>
        /// Creates a simple input box
        /// </summary>
        /// <param name="input"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static DialogResult ShowInputDialog(ref string input, string title)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = title;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            inputBox.StartPosition = FormStartPosition.CenterParent; 

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

       
        /// <summary>
        /// Replace <br /> with standard \r\n so that text displays correctly in textboxes
        /// </summary>
        /// <param name="inText"></param>
        /// <returns></returns>
        public static string ConvertBrToCrlf(string inText)
        {
            inText = inText.Replace("<br />", "\r\n");
            inText = inText.Replace("<br/>", "\r\n");
            inText = inText.Replace("<br>", "\r\n");
            return inText;
        }

        /// <summary>
        /// Replace \r\n with <br /> so that text is stored correctly in db
        /// </summary>
        /// <param name="inText"></param>
        /// <returns></returns>
        public static string ConvertCrlfToBr(string inText)
        {
            inText = inText.Replace("\r\n","<br />");
            return inText;
        }


        
        /// <summary>
        /// Basic Connectin Information Structure
        /// </summary>
        public struct ConnectionInfo
        {
            public string ServerNameorIp;
            public string DatabaseName;
            public string DatabaseUserName;
            public string DatabasePassword;
        }
    }
}