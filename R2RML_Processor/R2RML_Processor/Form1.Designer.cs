namespace R2RML_Processor
{
    partial class Form1
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
            this.rchbx = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rchbx
            // 
            this.rchbx.BackColor = System.Drawing.SystemColors.Window;
            this.rchbx.Location = new System.Drawing.Point(12, 12);
            this.rchbx.Name = "rchbx";
            this.rchbx.Size = new System.Drawing.Size(887, 347);
            this.rchbx.TabIndex = 0;
            this.rchbx.Text = "";
            this.rchbx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rchbx_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 371);
            this.Controls.Add(this.rchbx);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox rchbx;

    }
}

