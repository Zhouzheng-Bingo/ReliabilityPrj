namespace DA_DST
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.timer_update = new System.Windows.Forms.Timer(this.components);
            this.pageSetupDialog1 = new System.Windows.Forms.PageSetupDialog();
            this.statusStrip_Status = new System.Windows.Forms.StatusStrip();
            this.treeView_Machine = new System.Windows.Forms.TreeView();
            this.imageList_Tree = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.textBox_sysinfo = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_exit = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.button_login = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // timer_update
            // 
            this.timer_update.Tick += new System.EventHandler(this.timer_update_Tick);
            // 
            // statusStrip_Status
            // 
            this.statusStrip_Status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip_Status.Location = new System.Drawing.Point(0, 682);
            this.statusStrip_Status.Name = "statusStrip_Status";
            this.statusStrip_Status.Size = new System.Drawing.Size(1435, 22);
            this.statusStrip_Status.TabIndex = 11;
            this.statusStrip_Status.Text = "statusStrip1";
            // 
            // treeView_Machine
            // 
            this.treeView_Machine.ImageIndex = 0;
            this.treeView_Machine.ImageList = this.imageList_Tree;
            this.treeView_Machine.Location = new System.Drawing.Point(12, 125);
            this.treeView_Machine.Name = "treeView_Machine";
            this.treeView_Machine.SelectedImageIndex = 0;
            this.treeView_Machine.Size = new System.Drawing.Size(252, 546);
            this.treeView_Machine.TabIndex = 12;
            // 
            // imageList_Tree
            // 
            this.imageList_Tree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_Tree.ImageStream")));
            this.imageList_Tree.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList_Tree.Images.SetKeyName(0, "factory.ico");
            this.imageList_Tree.Images.SetKeyName(1, "section.ico");
            this.imageList_Tree.Images.SetKeyName(2, "shenfei_mach.ico");
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(278, 129);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1115, 355);
            this.listView1.TabIndex = 13;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.Visible = false;
            // 
            // textBox_sysinfo
            // 
            this.textBox_sysinfo.Location = new System.Drawing.Point(278, 525);
            this.textBox_sysinfo.Multiline = true;
            this.textBox_sysinfo.Name = "textBox_sysinfo";
            this.textBox_sysinfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_sysinfo.Size = new System.Drawing.Size(1115, 142);
            this.textBox_sysinfo.TabIndex = 14;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(270, 102);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1133, 394);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "设备状态";
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(270, 499);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1133, 172);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "系统运行信息";
            // 
            // button_exit
            // 
            this.button_exit.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_exit.Image = global::DA_DST.Properties.Resources.exit;
            this.button_exit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_exit.Location = new System.Drawing.Point(209, 32);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(95, 61);
            this.button_exit.TabIndex = 19;
            this.button_exit.Text = "退出";
            this.button_exit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // button_start
            // 
            this.button_start.Enabled = false;
            this.button_start.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_start.Image = global::DA_DST.Properties.Resources.start;
            this.button_start.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_start.Location = new System.Drawing.Point(115, 32);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(95, 61);
            this.button_start.TabIndex = 18;
            this.button_start.Text = "启动";
            this.button_start.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.button_start_Click);
            // 
            // button_login
            // 
            this.button_login.Enabled = false;
            this.button_login.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_login.Image = global::DA_DST.Properties.Resources.Login;
            this.button_login.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_login.Location = new System.Drawing.Point(21, 32);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(95, 61);
            this.button_login.TabIndex = 17;
            this.button_login.Text = "登陆";
            this.button_login.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button_login.UseVisualStyleBackColor = true;
            this.button_login.Click += new System.EventHandler(this.button_login_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(12, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1391, 94);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "操作栏";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1435, 704);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_start);
            this.Controls.Add(this.button_login);
            this.Controls.Add(this.textBox_sysinfo);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.treeView_Machine);
            this.Controls.Add(this.statusStrip_Status);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Main";
            this.Text = "机床数据采集系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.SizeChanged += new System.EventHandler(this.Main_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer_update;
        private System.Windows.Forms.PageSetupDialog pageSetupDialog1;
        private System.Windows.Forms.StatusStrip statusStrip_Status;
        private System.Windows.Forms.TreeView treeView_Machine;
        private System.Windows.Forms.ListView listView1;
        public  System.Windows.Forms.TextBox textBox_sysinfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.ImageList imageList_Tree;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

