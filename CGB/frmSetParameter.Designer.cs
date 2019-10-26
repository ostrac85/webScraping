namespace CGB
{
    partial class frmSetParameter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetParameter));
            this.txtCardCode = new System.Windows.Forms.TextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.dgvCards = new System.Windows.Forms.DataGridView();
            this.dcAcctName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dcBankAcctNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dcIDNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dcUKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dcCardCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblCardCode = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCards)).BeginInit();
            this.SuspendLayout();
            // 
            // txtCardCode
            // 
            this.txtCardCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCardCode.Location = new System.Drawing.Point(328, 30);
            this.txtCardCode.Name = "txtCardCode";
            this.txtCardCode.Size = new System.Drawing.Size(100, 20);
            this.txtCardCode.TabIndex = 26;
            this.txtCardCode.TabIndexChanged += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(434, 28);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 27;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.TabIndexChanged += new System.EventHandler(this.btnConfirm_Click);
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // dgvCards
            // 
            this.dgvCards.AllowUserToAddRows = false;
            this.dgvCards.AllowUserToDeleteRows = false;
            this.dgvCards.AllowUserToResizeColumns = false;
            this.dgvCards.AllowUserToResizeRows = false;
            this.dgvCards.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCards.BackgroundColor = System.Drawing.Color.White;
            this.dgvCards.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvCards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCards.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dcAcctName,
            this.dcBankAcctNum,
            this.dcIDNum,
            this.dcUKey,
            this.dcCardCode});
            this.dgvCards.Location = new System.Drawing.Point(63, 76);
            this.dgvCards.Name = "dgvCards";
            this.dgvCards.ReadOnly = true;
            this.dgvCards.RowHeadersVisible = false;
            this.dgvCards.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCards.ShowEditingIcon = false;
            this.dgvCards.ShowRowErrors = false;
            this.dgvCards.Size = new System.Drawing.Size(630, 97);
            this.dgvCards.TabIndex = 30;
            // 
            // dcAcctName
            // 
            this.dcAcctName.HeaderText = "Account Name";
            this.dcAcctName.Name = "dcAcctName";
            this.dcAcctName.ReadOnly = true;
            // 
            // dcBankAcctNum
            // 
            this.dcBankAcctNum.HeaderText = "Bank Account Number";
            this.dcBankAcctNum.Name = "dcBankAcctNum";
            this.dcBankAcctNum.ReadOnly = true;
            // 
            // dcIDNum
            // 
            this.dcIDNum.HeaderText = "ID Number";
            this.dcIDNum.Name = "dcIDNum";
            this.dcIDNum.ReadOnly = true;
            // 
            // dcUKey
            // 
            this.dcUKey.HeaderText = "U-Key";
            this.dcUKey.Name = "dcUKey";
            this.dcUKey.ReadOnly = true;
            this.dcUKey.Visible = false;
            // 
            // dcCardCode
            // 
            this.dcCardCode.HeaderText = "Card Code";
            this.dcCardCode.Name = "dcCardCode";
            this.dcCardCode.ReadOnly = true;
            // 
            // lblCardCode
            // 
            this.lblCardCode.AutoSize = true;
            this.lblCardCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.lblCardCode.Location = new System.Drawing.Point(262, 33);
            this.lblCardCode.Name = "lblCardCode";
            this.lblCardCode.Size = new System.Drawing.Size(60, 13);
            this.lblCardCode.TabIndex = 29;
            this.lblCardCode.Text = "Card Code:";
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.Location = new System.Drawing.Point(380, 189);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(157, 35);
            this.btnCancel.TabIndex = 28;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Enabled = false;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnConfirm.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm.ForeColor = System.Drawing.Color.Black;
            this.btnConfirm.Location = new System.Drawing.Point(217, 189);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(157, 35);
            this.btnConfirm.TabIndex = 28;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // frmSetParameter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 251);
            this.Controls.Add(this.txtCardCode);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.dgvCards);
            this.Controls.Add(this.lblCardCode);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmSetParameter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CGB Card Code";
            this.Load += new System.EventHandler(this.frmSetParameter_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCards)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCardCode;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.DataGridView dgvCards;
        private System.Windows.Forms.DataGridViewTextBoxColumn dcAcctName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dcBankAcctNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn dcIDNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn dcUKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn dcCardCode;
        private System.Windows.Forms.Label lblCardCode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;
    }
}