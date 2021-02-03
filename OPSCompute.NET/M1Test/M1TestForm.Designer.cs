namespace M1Test
{
    partial class M1TestForm
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
            this.entradasRegistro = new System.Windows.Forms.ComboBox();
            this.lblEntradaRegistro = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textoEntrada = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.m1Output = new System.Windows.Forms.TreeView();
            this.m50Output = new System.Windows.Forms.TreeView();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCompute = new System.Windows.Forms.Button();
            this.lblState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // entradasRegistro
            // 
            this.entradasRegistro.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.entradasRegistro.FormattingEnabled = true;
            this.entradasRegistro.Location = new System.Drawing.Point(12, 39);
            this.entradasRegistro.Name = "entradasRegistro";
            this.entradasRegistro.Size = new System.Drawing.Size(334, 29);
            this.entradasRegistro.TabIndex = 0;
            // 
            // lblEntradaRegistro
            // 
            this.lblEntradaRegistro.AutoSize = true;
            this.lblEntradaRegistro.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEntradaRegistro.Location = new System.Drawing.Point(12, 14);
            this.lblEntradaRegistro.Name = "lblEntradaRegistro";
            this.lblEntradaRegistro.Size = new System.Drawing.Size(152, 22);
            this.lblEntradaRegistro.TabIndex = 1;
            this.lblEntradaRegistro.Text = "Entrada Registro";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 22);
            this.label2.TabIndex = 2;
            this.label2.Text = "Entrada";
            // 
            // textoEntrada
            // 
            this.textoEntrada.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textoEntrada.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textoEntrada.Location = new System.Drawing.Point(12, 106);
            this.textoEntrada.Name = "textoEntrada";
            this.textoEntrada.Size = new System.Drawing.Size(922, 28);
            this.textoEntrada.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 22);
            this.label3.TabIndex = 4;
            this.label3.Text = "M1 Output";
            // 
            // m1Output
            // 
            this.m1Output.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m1Output.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m1Output.Location = new System.Drawing.Point(12, 175);
            this.m1Output.Name = "m1Output";
            this.m1Output.Size = new System.Drawing.Size(447, 472);
            this.m1Output.TabIndex = 5;
            // 
            // m50Output
            // 
            this.m50Output.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m50Output.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m50Output.Location = new System.Drawing.Point(487, 175);
            this.m50Output.Name = "m50Output";
            this.m50Output.Size = new System.Drawing.Size(447, 472);
            this.m50Output.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(487, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 22);
            this.label4.TabIndex = 6;
            this.label4.Text = "M50 Output";
            // 
            // btnCompute
            // 
            this.btnCompute.Font = new System.Drawing.Font("Arial", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCompute.Location = new System.Drawing.Point(352, 39);
            this.btnCompute.Name = "btnCompute";
            this.btnCompute.Size = new System.Drawing.Size(107, 33);
            this.btnCompute.TabIndex = 8;
            this.btnCompute.Text = "Compute";
            this.btnCompute.UseVisualStyleBackColor = true;
            this.btnCompute.Click += new System.EventHandler(this.btnCompute_Click);
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Font = new System.Drawing.Font("Arial", 10.8F);
            this.lblState.Location = new System.Drawing.Point(524, 44);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(137, 22);
            this.lblState.TabIndex = 9;
            this.lblState.Text = "State Message";
            // 
            // M1TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 659);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.btnCompute);
            this.Controls.Add(this.m50Output);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m1Output);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textoEntrada);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblEntradaRegistro);
            this.Controls.Add(this.entradasRegistro);
            this.Name = "M1TestForm";
            this.Text = "M1 Test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox entradasRegistro;
        private System.Windows.Forms.Label lblEntradaRegistro;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textoEntrada;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TreeView m1Output;
        private System.Windows.Forms.TreeView m50Output;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCompute;
        private System.Windows.Forms.Label lblState;
    }
}