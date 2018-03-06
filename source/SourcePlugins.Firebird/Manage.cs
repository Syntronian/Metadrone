﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SourcePlugins.Firebird
{
    public partial class Manage : UserControl, Metadrone.PluginInterface.Sources.IManageSource
    {

        public event Metadrone.PluginInterface.Sources.IManageSource.ValueChangedEventHandler ValueChanged;
        public delegate void ValueChangedEventHandler(object value);
        public event Metadrone.PluginInterface.Sources.IManageSource.SaveEventHandler Save;
        public delegate void SaveEventHandler();

        public Manage()
        {
            InitializeComponent();

            this.Load += new System.EventHandler(this.Manage_Load);

            this.txtServer.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtFile.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtPort.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtUsername.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtPassword.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.cboCharset.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtRole.TextChanged += new System.EventHandler(this.txtConnParts_TextChanged);
            this.txtConnectionString.TextChanged += new System.EventHandler(this.txtConnectionString_TextChanged);
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);

            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);

            this.txtSchemaQuery.KeyDown += new Metadrone.UI.SQLEditor.KeyDownEventHandler(this.txtSchemaQuery_KeyDown);
            this.lnkPreviewSchema.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkPreviewSchema_LinkClicked);

            this.txtSchemaQuery.TextChanged += new Metadrone.UI.SQLEditor.TextChangedEventHandler(this.txtSchemaQuery_TextChanged);
            this.txtSchemaQuery.SavePress += new Metadrone.UI.SQLEditor.SavePressEventHandler(this.SavePress);
            this.txtTableSchemaQuery.TextChanged += new Metadrone.UI.SQLEditor.TextChangedEventHandler(this.txtTableSchemaQuery_TextChanged);
            this.txtTableSchemaQuery.SavePress += new Metadrone.UI.SQLEditor.SavePressEventHandler(this.SavePress);
            this.txtColumnSchemaQuery.TextChanged += new Metadrone.UI.SQLEditor.TextChangedEventHandler(this.txtColumnSchemaQuery_TextChanged);
            this.txtColumnSchemaQuery.SavePress += new Metadrone.UI.SQLEditor.SavePressEventHandler(this.SavePress);

            this.txtTableName.TextChanged += new System.EventHandler(this.txtTableName_TextChanged);

            this.rbApproachSingle.CheckedChanged += new System.EventHandler(this.rbMeta_CheckedChanged);
            this.rbApproachTableColumn.CheckedChanged += new System.EventHandler(this.rbMeta_CheckedChanged);
            this.rbTableDefault.CheckedChanged += new System.EventHandler(this.rbMeta_CheckedChanged);
            this.rbTableQuery.CheckedChanged += new System.EventHandler(this.rbMeta_CheckedChanged);

            this.txtTransformations.SavePress += new Metadrone.UI.TransformationsEditor.SavePressEventHandler(this.SavePress);
            this.txtTransformations.TextChanged += new Metadrone.UI.TransformationsEditor.TextChangedEventHandler(this.txtTransformations_TextChanged);
        }


        public void Setup()
        {
            if (this.SchemaQuery.Length == 0)
            {
                this.SchemaQuery = new Connection("", "").GetQuery(Connection.QueryEnum.SchemaQuery);
            }
            this.splitMain.Panel2Collapsed = true;
        }

        private void TestConn()
        {
            Connection conn = new Connection("", this.txtConnectionString.Text);
            conn.TestConnection();
        }


        #region "Properties"

        public string ConnectionString
        {
            get { return this.txtConnectionString.Text; }
            set { this.txtConnectionString.Text = value; }
        }

        public bool SingleResultApproach
        {
            get { return this.rbApproachSingle.Checked; }
            set
            {
                this.rbApproachSingle.Checked = value;
                this.rbApproachTableColumn.Checked = !value;
            }
        }

        public bool TableSchemaGeneric
        {
            get { return this.rbTableDefault.Checked; }
            set
            {
                this.rbTableDefault.Checked = value;
                this.rbTableQuery.Checked = !value;
            }
        }

        public bool ColumnSchemaGeneric
        {
            get { return false; }

            set { }
        }

        public string SchemaQuery
        {
            get { return this.txtSchemaQuery.Text; }
            set { this.txtSchemaQuery.Text = value; }
        }

        public string TableSchemaQuery
        {
            get { return this.txtTableSchemaQuery.Text; }
            set { this.txtTableSchemaQuery.Text = value; }
        }

        public string ColumnSchemaQuery
        {
            get { return this.txtColumnSchemaQuery.Text; }
            set { this.txtColumnSchemaQuery.Text = value; }
        }

        public string TableName
        {
            get { return this.txtTableName.Text; }
            set { this.txtTableName.Text = value; }
        }

        public string RoutineSchemaQuery
        {
            get { return null; }
            set { }
        }

        public string Transformations
        {
            get { return this.txtTransformations.Text; }
            set { this.txtTransformations.Text = value; }
        }

        #endregion


        #region "Events"

        private void Manage_Load(object sender, EventArgs e)
        {
            this.rbMeta_CheckedChanged(sender, e);
        }

        private void SavePress()
        {
            if (Save != null)
            {
                Save();
            }
        }

        private void txtSchemaQuery_TextChanged(object sender, System.EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this.txtSchemaQuery.Text);
            }
        }

        private void txtTableSchemaQuery_TextChanged(object sender, System.EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this.txtTableSchemaQuery.Text);
            }
        }


        private void txtColumnSchemaQuery_TextChanged(object sender, System.EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this.txtColumnSchemaQuery.Text);
            }
        }

        private void txtTableName_TextChanged(object sender, System.EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this.txtTableName.Text);
            }
        }

        private void txtTransformations_TextChanged(object sender, System.EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this.txtTransformations.Text);
            }
        }

        private void rbMeta_CheckedChanged(System.Object sender, System.EventArgs e)
        {
            this.splitQuery.Panel1Collapsed = !this.rbApproachSingle.Checked;
            this.splitQuery.Panel2Collapsed = this.rbApproachSingle.Checked;
            this.grpTableSchema.Visible = !this.rbApproachSingle.Checked;
            this.grpColumnSchema.Visible = !this.rbApproachSingle.Checked;

            this.splitTableColumn.Panel1Collapsed = !this.rbTableQuery.Checked;

            Connection conn = new Connection("", "");
            if (this.rbApproachSingle.Checked)
            {
                if (this.SchemaQuery.Length == 0)
                    this.SchemaQuery = conn.GetQuery(Connection.QueryEnum.SchemaQuery);
                this.txtTableSchemaQuery.Text = "";
                this.txtColumnSchemaQuery.Text = "";
                this.txtTableName.Text = "";
            }
            else
            {
                this.splitMain.Panel2Collapsed = true;

                if (this.rbTableQuery.Checked)
                {
                    if (this.txtTableSchemaQuery.Text.Length == 0)
                        this.txtTableSchemaQuery.Text = conn.GetQuery(Connection.QueryEnum.TableQuery);
                }
                else
                {
                    this.txtTableSchemaQuery.Text = "";
                }
                if (this.txtColumnSchemaQuery.Text.Length == 0)
                    this.txtColumnSchemaQuery.Text = conn.GetQuery(Connection.QueryEnum.ColumnQuery);
                if (this.txtTableName.Text.Length == 0)
                {
                    this.txtTableName.Text = Metadrone.Persistence.Source.Default_TableNamePlaceHolder;
                }
                this.SchemaQuery = "";
            }
        }


        #endregion


        #region "Connection guff"

        private void BuildConnectionString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("User=" + this.txtUsername.Text + ";");
            sb.Append("Password=" + this.txtPassword.Text + ";");
            sb.Append("Database=" + this.txtFile.Text + ";");
            sb.Append("Source=" + this.txtServer.Text + ";");
            sb.Append("Port=" + this.txtPort.Text + ";");
            sb.Append("Dialect=3;");
            sb.Append("Charset=" + this.cboCharset.Text + ";");
            sb.Append("Role=" + this.txtRole.Text + ";");
            sb.Append("Connection lifetime=15;Pooling=true; MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;");

            this.txtConnectionString.Text = sb.ToString();
        }

        private void txtConnParts_TextChanged(object sender, EventArgs e)
        {
            this.BuildConnectionString();
        }

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {
            if (ValueChanged != null)
            {
                ValueChanged(((TextBox)sender).Text);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                this.TestConn();

                this.Cursor = Cursors.Default;
                MessageBox.Show("Test connection successful.", "Metadrone", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Metadrone", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);

            }
        }

        private void btnFile_Click(System.Object sender, System.EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "Firebird Database Files (*.fdb)|*.fdb|All Files|*.*";
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFile.Text = dlg.FileName;
            }
        }

        #endregion


        #region "Query Testing"

        private void lnkPreviewSchema_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.TestQuery();
        }

        private void txtSchemaQuery_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                this.TestQuery();
                this.txtSchemaQuery.Focus();
            }
        }

        private void TestQuery()
        {
            this.splitMain.Panel2Collapsed = false;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                this.QueryResults.PrepareSourceLoad();
                Connection dt = new Connection("", this.txtConnectionString.Text);
                this.QueryResults.SetSource(dt.TestQuery(this.SchemaQuery));

            }
            catch (Exception ex)
            {
                this.QueryResults.Messages = ex.Message;

            }

            this.Cursor = Cursors.Default;
        }

        #endregion

    }
}
