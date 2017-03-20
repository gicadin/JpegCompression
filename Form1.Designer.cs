namespace Assignment2
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
            this.LeftImage_Box = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decompressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressPFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decompressPFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RightImg_Box = new System.Windows.Forms.PictureBox();
            this.BttmImg_Box = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LeftImage_Box)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightImg_Box)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BttmImg_Box)).BeginInit();
            this.SuspendLayout();
            // 
            // LeftImage_Box
            // 
            this.LeftImage_Box.Location = new System.Drawing.Point(9, 34);
            this.LeftImage_Box.Margin = new System.Windows.Forms.Padding(2);
            this.LeftImage_Box.Name = "LeftImage_Box";
            this.LeftImage_Box.Size = new System.Drawing.Size(375, 600);
            this.LeftImage_Box.TabIndex = 1;
            this.LeftImage_Box.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(768, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem1,
            this.convertToolStripMenuItem,
            this.compressToolStripMenuItem,
            this.decompressToolStripMenuItem,
            this.compressPFrameToolStripMenuItem,
            this.decompressPFrameToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.openToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem1
            // 
            this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
            this.openToolStripMenuItem1.Size = new System.Drawing.Size(182, 22);
            this.openToolStripMenuItem1.Text = "Open";
            this.openToolStripMenuItem1.Click += new System.EventHandler(this.openToolStripMenuItem1_Click);
            // 
            // convertToolStripMenuItem
            // 
            this.convertToolStripMenuItem.Name = "convertToolStripMenuItem";
            this.convertToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.convertToolStripMenuItem.Text = "Convert";
            this.convertToolStripMenuItem.Click += new System.EventHandler(this.convertToolStripMenuItem_Click);
            // 
            // compressToolStripMenuItem
            // 
            this.compressToolStripMenuItem.Name = "compressToolStripMenuItem";
            this.compressToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.compressToolStripMenuItem.Text = "Compress";
            this.compressToolStripMenuItem.Click += new System.EventHandler(this.compressToolStripMenuItem_Click);
            // 
            // decompressToolStripMenuItem
            // 
            this.decompressToolStripMenuItem.Name = "decompressToolStripMenuItem";
            this.decompressToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.decompressToolStripMenuItem.Text = "Decompress";
            this.decompressToolStripMenuItem.Click += new System.EventHandler(this.decompressToolStripMenuItem_Click);
            // 
            // compressPFrameToolStripMenuItem
            // 
            this.compressPFrameToolStripMenuItem.Name = "compressPFrameToolStripMenuItem";
            this.compressPFrameToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.compressPFrameToolStripMenuItem.Text = "Compress pFrame";
            this.compressPFrameToolStripMenuItem.Click += new System.EventHandler(this.compressPFrameToolStripMenuItem_Click);
            // 
            // decompressPFrameToolStripMenuItem
            // 
            this.decompressPFrameToolStripMenuItem.Name = "decompressPFrameToolStripMenuItem";
            this.decompressPFrameToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.decompressPFrameToolStripMenuItem.Text = "Decompress pFrame";
            this.decompressPFrameToolStripMenuItem.Click += new System.EventHandler(this.decompressPFrameToolStripMenuItem_Click);
            // 
            // RightImg_Box
            // 
            this.RightImg_Box.Location = new System.Drawing.Point(388, 34);
            this.RightImg_Box.Margin = new System.Windows.Forms.Padding(2);
            this.RightImg_Box.Name = "RightImg_Box";
            this.RightImg_Box.Size = new System.Drawing.Size(375, 400);
            this.RightImg_Box.TabIndex = 4;
            this.RightImg_Box.TabStop = false;
            // 
            // BttmImg_Box
            // 
            this.BttmImg_Box.Location = new System.Drawing.Point(388, 439);
            this.BttmImg_Box.Name = "BttmImg_Box";
            this.BttmImg_Box.Size = new System.Drawing.Size(375, 266);
            this.BttmImg_Box.TabIndex = 5;
            this.BttmImg_Box.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 717);
            this.Controls.Add(this.BttmImg_Box);
            this.Controls.Add(this.RightImg_Box);
            this.Controls.Add(this.LeftImage_Box);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.LeftImage_Box)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RightImg_Box)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BttmImg_Box)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox LeftImage_Box;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem convertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decompressToolStripMenuItem;
        private System.Windows.Forms.PictureBox RightImg_Box;
        private System.Windows.Forms.ToolStripMenuItem compressPFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decompressPFrameToolStripMenuItem;
        private System.Windows.Forms.PictureBox BttmImg_Box;
    }
}

