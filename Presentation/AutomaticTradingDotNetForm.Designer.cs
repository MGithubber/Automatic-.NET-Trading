namespace Presentation
{
    partial class AutomaticTradingDotNetForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.NrActiveBotsTextBox = new System.Windows.Forms.TextBox();
            this.LiveDetailsPanel = new System.Windows.Forms.Panel();
            this.LiveDetailsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.BackColor = System.Drawing.Color.Black;
            this.OutputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutputTextBox.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.OutputTextBox.ForeColor = System.Drawing.SystemColors.Control;
            this.OutputTextBox.Location = new System.Drawing.Point(12, 223);
            this.OutputTextBox.Multiline = true;
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.ReadOnly = true;
            this.OutputTextBox.Size = new System.Drawing.Size(1060, 346);
            this.OutputTextBox.TabIndex = 0;
            // 
            // StartButton
            // 
            this.StartButton.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.StartButton.ForeColor = System.Drawing.Color.Black;
            this.StartButton.Location = new System.Drawing.Point(899, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(173, 64);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start Trading";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "Nr. active bots =";
            // 
            // NrActiveBotsTextBox
            // 
            this.NrActiveBotsTextBox.BackColor = System.Drawing.Color.Black;
            this.NrActiveBotsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NrActiveBotsTextBox.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.NrActiveBotsTextBox.ForeColor = System.Drawing.SystemColors.Control;
            this.NrActiveBotsTextBox.Location = new System.Drawing.Point(182, 3);
            this.NrActiveBotsTextBox.Name = "NrActiveBotsTextBox";
            this.NrActiveBotsTextBox.ReadOnly = true;
            this.NrActiveBotsTextBox.Size = new System.Drawing.Size(40, 36);
            this.NrActiveBotsTextBox.TabIndex = 3;
            this.NrActiveBotsTextBox.Text = "0";
            this.NrActiveBotsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LiveDetailsPanel
            // 
            this.LiveDetailsPanel.Controls.Add(this.label1);
            this.LiveDetailsPanel.Controls.Add(this.NrActiveBotsTextBox);
            this.LiveDetailsPanel.Location = new System.Drawing.Point(12, 12);
            this.LiveDetailsPanel.Name = "LiveDetailsPanel";
            this.LiveDetailsPanel.Size = new System.Drawing.Size(400, 205);
            this.LiveDetailsPanel.TabIndex = 4;
            // 
            // AutomaticTradingDotNetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1084, 581);
            this.Controls.Add(this.LiveDetailsPanel);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.OutputTextBox);
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AutomaticTradingDotNetForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Automatic Trading .NET";
            this.Load += new System.EventHandler(this.AutomaticTradingDotNetForm_Load);
            this.LiveDetailsPanel.ResumeLayout(false);
            this.LiveDetailsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox OutputTextBox;
        private Button StartButton;
        private Label label1;
        private TextBox NrActiveBotsTextBox;
        private Panel LiveDetailsPanel;
    }
}