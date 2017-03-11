namespace IR
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
            this.crawl = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.column = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.URL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.URLContent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.visitedCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // crawl
            // 
            this.crawl.Location = new System.Drawing.Point(12, 12);
            this.crawl.Name = "crawl";
            this.crawl.Size = new System.Drawing.Size(75, 23);
            this.crawl.TabIndex = 0;
            this.crawl.Text = "Crawl";
            this.crawl.UseVisualStyleBackColor = true;
            this.crawl.Click += new System.EventHandler(this.crawl_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.column,
            this.URL,
            this.URLContent,
            this.columnHeader1});
            this.listView1.Location = new System.Drawing.Point(12, 41);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1219, 482);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // column
            // 
            this.column.Text = "column";
            this.column.Width = 0;
            // 
            // URL
            // 
            this.URL.Text = "URL";
            this.URL.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.URL.Width = 340;
            // 
            // URLContent
            // 
            this.URLContent.Text = "Html Content";
            this.URLContent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.URLContent.Width = 300;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Content";
            this.columnHeader1.Width = 400;
            // 
            // visitedCount
            // 
            this.visitedCount.AutoSize = true;
            this.visitedCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.visitedCount.Location = new System.Drawing.Point(114, 15);
            this.visitedCount.Name = "visitedCount";
            this.visitedCount.Size = new System.Drawing.Size(0, 16);
            this.visitedCount.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1243, 535);
            this.Controls.Add(this.visitedCount);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.crawl);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button crawl;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label visitedCount;
        private System.Windows.Forms.ColumnHeader column;
        private System.Windows.Forms.ColumnHeader URL;
        private System.Windows.Forms.ColumnHeader URLContent;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}

