namespace AutoStarter
{
    partial class About
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_Titel = new System.Windows.Forms.Label();
            this.txt_Build = new System.Windows.Forms.Label();
            this.txt_BuildDate = new System.Windows.Forms.Label();
            this.lbl_Version = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txt_Titel
            // 
            this.txt_Titel.AutoSize = true;
            this.txt_Titel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Titel.Location = new System.Drawing.Point(31, 33);
            this.txt_Titel.Name = "txt_Titel";
            this.txt_Titel.Size = new System.Drawing.Size(51, 16);
            this.txt_Titel.TabIndex = 0;
            this.txt_Titel.Text = "label1";
            // 
            // txt_Build
            // 
            this.txt_Build.AutoSize = true;
            this.txt_Build.Location = new System.Drawing.Point(31, 115);
            this.txt_Build.Name = "txt_Build";
            this.txt_Build.Size = new System.Drawing.Size(35, 13);
            this.txt_Build.TabIndex = 1;
            this.txt_Build.Text = "label1";
            // 
            // txt_BuildDate
            // 
            this.txt_BuildDate.AutoSize = true;
            this.txt_BuildDate.Location = new System.Drawing.Point(31, 149);
            this.txt_BuildDate.Name = "txt_BuildDate";
            this.txt_BuildDate.Size = new System.Drawing.Size(35, 13);
            this.txt_BuildDate.TabIndex = 2;
            this.txt_BuildDate.Text = "label1";
            // 
            // lbl_Version
            // 
            this.lbl_Version.AutoSize = true;
            this.lbl_Version.Location = new System.Drawing.Point(34, 79);
            this.lbl_Version.Name = "lbl_Version";
            this.lbl_Version.Size = new System.Drawing.Size(35, 13);
            this.lbl_Version.TabIndex = 3;
            this.lbl_Version.Text = "label1";
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 267);
            this.Controls.Add(this.lbl_Version);
            this.Controls.Add(this.txt_BuildDate);
            this.Controls.Add(this.txt_Build);
            this.Controls.Add(this.txt_Titel);
            this.Name = "About";
            this.Text = "Über";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label txt_Titel;
        private System.Windows.Forms.Label txt_Build;
        private System.Windows.Forms.Label txt_BuildDate;
        private System.Windows.Forms.Label lbl_Version;
    }
}