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
            this.visitedCount = new System.Windows.Forms.Label();
            this.Filter_Documents_button = new System.Windows.Forms.Button();
            this.Tokenize_Button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Soundex_Button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.Bigram_Button = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
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
            // visitedCount
            // 
            this.visitedCount.AutoSize = true;
            this.visitedCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.visitedCount.Location = new System.Drawing.Point(114, 15);
            this.visitedCount.Name = "visitedCount";
            this.visitedCount.Size = new System.Drawing.Size(0, 16);
            this.visitedCount.TabIndex = 3;
            // 
            // Filter_Documents_button
            // 
            this.Filter_Documents_button.Location = new System.Drawing.Point(12, 66);
            this.Filter_Documents_button.Name = "Filter_Documents_button";
            this.Filter_Documents_button.Size = new System.Drawing.Size(119, 23);
            this.Filter_Documents_button.TabIndex = 4;
            this.Filter_Documents_button.Text = "English Documents";
            this.Filter_Documents_button.UseVisualStyleBackColor = true;
            this.Filter_Documents_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // Tokenize_Button
            // 
            this.Tokenize_Button.Location = new System.Drawing.Point(12, 125);
            this.Tokenize_Button.Name = "Tokenize_Button";
            this.Tokenize_Button.Size = new System.Drawing.Size(119, 23);
            this.Tokenize_Button.TabIndex = 5;
            this.Tokenize_Button.Text = "Start Tokenize";
            this.Tokenize_Button.UseVisualStyleBackColor = true;
            this.Tokenize_Button.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(196, 135);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Tokenize Count";
            // 
            // Soundex_Button
            // 
            this.Soundex_Button.Location = new System.Drawing.Point(12, 206);
            this.Soundex_Button.Name = "Soundex_Button";
            this.Soundex_Button.Size = new System.Drawing.Size(119, 23);
            this.Soundex_Button.TabIndex = 8;
            this.Soundex_Button.Text = "Start Soundex";
            this.Soundex_Button.UseVisualStyleBackColor = true;
            this.Soundex_Button.Click += new System.EventHandler(this.button3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(196, 216);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Soundex Count :";
            // 
            // Bigram_Button
            // 
            this.Bigram_Button.Location = new System.Drawing.Point(12, 289);
            this.Bigram_Button.Name = "Bigram_Button";
            this.Bigram_Button.Size = new System.Drawing.Size(119, 23);
            this.Bigram_Button.TabIndex = 10;
            this.Bigram_Button.Text = "Start Bigrams";
            this.Bigram_Button.UseVisualStyleBackColor = true;
            this.Bigram_Button.Click += new System.EventHandler(this.button4_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(196, 294);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Bigram Count :";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 377);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Bigram_Button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Soundex_Button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Tokenize_Button);
            this.Controls.Add(this.Filter_Documents_button);
            this.Controls.Add(this.visitedCount);
            this.Controls.Add(this.crawl);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button crawl;
        private System.Windows.Forms.Label visitedCount;
        private System.Windows.Forms.Button Filter_Documents_button;
        private System.Windows.Forms.Button Tokenize_Button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Soundex_Button;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Bigram_Button;
        private System.Windows.Forms.Label label3;
    }
}

