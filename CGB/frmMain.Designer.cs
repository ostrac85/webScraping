namespace CGB
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblRequestedUKey = new System.Windows.Forms.Label();
            this.lblQueueUkey = new System.Windows.Forms.Label();
            this.tsMainFunctions = new System.Windows.Forms.ToolStrip();
            this.tsRelogin = new System.Windows.Forms.ToolStripButton();
            this.tsTimer = new System.Windows.Forms.ToolStripLabel();
            this.tsLogo = new System.Windows.Forms.ToolStripLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.wbMain = new System.Windows.Forms.WebBrowser();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tlblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1.SuspendLayout();
            this.tsMainFunctions.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnContinue);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.lblRequestedUKey);
            this.panel1.Controls.Add(this.lblQueueUkey);
            this.panel1.Controls.Add(this.tsMainFunctions);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1008, 83);
            this.panel1.TabIndex = 0;
            // 
            // btnContinue
            // 
            this.btnContinue.Enabled = false;
            this.btnContinue.Location = new System.Drawing.Point(193, 24);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(91, 30);
            this.btnContinue.TabIndex = 8;
            this.btnContinue.TabStop = false;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.Transparent;
            this.btnStop.Location = new System.Drawing.Point(96, 24);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(91, 30);
            this.btnStop.TabIndex = 7;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblRequestedUKey
            // 
            this.lblRequestedUKey.AutoSize = true;
            this.lblRequestedUKey.Location = new System.Drawing.Point(878, 21);
            this.lblRequestedUKey.Name = "lblRequestedUKey";
            this.lblRequestedUKey.Size = new System.Drawing.Size(118, 13);
            this.lblRequestedUKey.TabIndex = 6;
            this.lblRequestedUKey.Text = "Requested UKey Input:";
            this.lblRequestedUKey.Visible = false;
            // 
            // lblQueueUkey
            // 
            this.lblQueueUkey.AutoSize = true;
            this.lblQueueUkey.Location = new System.Drawing.Point(878, 46);
            this.lblQueueUkey.Name = "lblQueueUkey";
            this.lblQueueUkey.Size = new System.Drawing.Size(51, 13);
            this.lblQueueUkey.TabIndex = 5;
            this.lblQueueUkey.Text = "Priority #:";
            this.lblQueueUkey.Visible = false;
            // 
            // tsMainFunctions
            // 
            this.tsMainFunctions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(240)))));
            this.tsMainFunctions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsMainFunctions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsRelogin,
            this.tsTimer,
            this.tsLogo});
            this.tsMainFunctions.Location = new System.Drawing.Point(0, 0);
            this.tsMainFunctions.Name = "tsMainFunctions";
            this.tsMainFunctions.Size = new System.Drawing.Size(1008, 82);
            this.tsMainFunctions.TabIndex = 2;
            this.tsMainFunctions.Text = "toolStrip1";
            // 
            // tsRelogin
            // 
            this.tsRelogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(173)))), ((int)(((byte)(240)))));
            this.tsRelogin.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsRelogin.ForeColor = System.Drawing.Color.White;
            this.tsRelogin.Image = ((System.Drawing.Image)(resources.GetObject("tsRelogin.Image")));
            this.tsRelogin.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsRelogin.ImageTransparentColor = System.Drawing.Color.AliceBlue;
            this.tsRelogin.Name = "tsRelogin";
            this.tsRelogin.Padding = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.tsRelogin.Size = new System.Drawing.Size(71, 79);
            this.tsRelogin.Text = "Re-Login";
            this.tsRelogin.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsRelogin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsRelogin.Click += new System.EventHandler(this.tsRelogin_Click);
            // 
            // tsTimer
            // 
            this.tsTimer.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold);
            this.tsTimer.ForeColor = System.Drawing.Color.White;
            this.tsTimer.Margin = new System.Windows.Forms.Padding(30, 1, 1, 2);
            this.tsTimer.Name = "tsTimer";
            this.tsTimer.Size = new System.Drawing.Size(99, 79);
            this.tsTimer.Text = "00:00";
            this.tsTimer.Visible = false;
            // 
            // tsLogo
            // 
            this.tsLogo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsLogo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsLogo.Name = "tsLogo";
            this.tsLogo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tsLogo.Size = new System.Drawing.Size(0, 79);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.statusStrip1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 83);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1008, 468);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.wbMain);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1008, 446);
            this.panel3.TabIndex = 2;
            // 
            // wbMain
            // 
            this.wbMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbMain.Location = new System.Drawing.Point(0, 0);
            this.wbMain.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbMain.Name = "wbMain";
            this.wbMain.ScriptErrorsSuppressed = true;
            this.wbMain.Size = new System.Drawing.Size(1008, 446);
            this.wbMain.TabIndex = 0;
            this.wbMain.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.wbMain_DocumentCompleted);
            this.wbMain.NewWindow += new System.ComponentModel.CancelEventHandler(this.wbMain_NewWindow);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 446);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tlblStatus
            // 
            this.tlblStatus.Name = "tlblStatus";
            this.tlblStatus.Size = new System.Drawing.Size(39, 17);
            this.tlblStatus.Text = "Status";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 551);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "CGB - Main Page";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tsMainFunctions.ResumeLayout(false);
            this.tsMainFunctions.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.WebBrowser wbMain;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tlblStatus;
        private System.Windows.Forms.ToolStrip tsMainFunctions;
        private System.Windows.Forms.ToolStripButton tsRelogin;
        private System.Windows.Forms.ToolStripLabel tsTimer;
        private System.Windows.Forms.ToolStripLabel tsLogo;
        public System.Windows.Forms.Label lblRequestedUKey;
        public System.Windows.Forms.Label lblQueueUkey;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnStop;
    }
}

